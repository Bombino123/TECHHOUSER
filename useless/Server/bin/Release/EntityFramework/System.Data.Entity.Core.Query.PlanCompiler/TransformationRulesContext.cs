using System.Collections.Generic;
using System.Data.Entity.Core.Common;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Core.Query.InternalTrees;
using System.Linq;

namespace System.Data.Entity.Core.Query.PlanCompiler;

internal class TransformationRulesContext : RuleProcessingContext
{
	private readonly PlanCompiler m_compilerState;

	private readonly VarRemapper m_remapper;

	private readonly Dictionary<Node, Node> m_suppressions;

	private readonly VarVec m_remappedVars;

	private bool m_projectionPruningRequired;

	private bool m_reapplyNullabilityRules;

	private readonly Stack<Node> m_relOpAncestors = new Stack<Node>();

	internal PlanCompiler PlanCompiler => m_compilerState;

	internal bool ProjectionPruningRequired => m_projectionPruningRequired;

	internal bool ReapplyNullabilityRules => m_reapplyNullabilityRules;

	internal bool CanChangeNullSentinelValue
	{
		get
		{
			if (m_compilerState.HasSortingOnNullSentinels)
			{
				return false;
			}
			if (m_relOpAncestors.Any((Node a) => IsOpNotSafeForNullSentinelValueChange(a.Op.OpType)))
			{
				return false;
			}
			foreach (Node item in m_relOpAncestors.Where((Node a) => a.Op.OpType == OpType.CrossApply || a.Op.OpType == OpType.OuterApply))
			{
				if (!m_relOpAncestors.Contains(item.Child1) && HasOpNotSafeForNullSentinelValueChange(item.Child1))
				{
					return false;
				}
			}
			return true;
		}
	}

	internal void RemapSubtree(Node subTree)
	{
		m_remapper.RemapSubtree(subTree);
	}

	internal void AddVarMapping(Var oldVar, Var newVar)
	{
		m_remapper.AddMapping(oldVar, newVar);
		m_remappedVars.Set(oldVar);
	}

	internal Node ReMap(Node node, Dictionary<Var, Node> varMap)
	{
		PlanCompiler.Assert(node.Op.IsScalarOp, "Expected a scalarOp: Found " + Dump.AutoString.ToString(node.Op.OpType));
		if (node.Op.OpType == OpType.VarRef)
		{
			VarRefOp varRefOp = node.Op as VarRefOp;
			Node value = null;
			if (varMap.TryGetValue(varRefOp.Var, out value))
			{
				return Copy(value);
			}
			return node;
		}
		for (int i = 0; i < node.Children.Count; i++)
		{
			node.Children[i] = ReMap(node.Children[i], varMap);
		}
		base.Command.RecomputeNodeInfo(node);
		return node;
	}

	internal Node Copy(Node node)
	{
		if (node.Op.OpType == OpType.VarRef)
		{
			VarRefOp varRefOp = node.Op as VarRefOp;
			return base.Command.CreateNode(base.Command.CreateVarRefOp(varRefOp.Var));
		}
		return OpCopier.Copy(base.Command, node);
	}

	internal bool IsScalarOpTree(Node node)
	{
		int nonLeafNodeCount = 0;
		return IsScalarOpTree(node, null, ref nonLeafNodeCount);
	}

	internal bool IsNonNullable(Var variable)
	{
		if (variable.VarType == VarType.Parameter && !TypeSemantics.IsNullable(variable.Type))
		{
			return true;
		}
		foreach (Node relOpAncestor in m_relOpAncestors)
		{
			base.Command.RecomputeNodeInfo(relOpAncestor);
			ExtendedNodeInfo extendedNodeInfo = base.Command.GetExtendedNodeInfo(relOpAncestor);
			if (extendedNodeInfo.NonNullableVisibleDefinitions.IsSet(variable))
			{
				return true;
			}
			if (extendedNodeInfo.LocalDefinitions.IsSet(variable))
			{
				return false;
			}
		}
		return false;
	}

	internal static bool IsOpNotSafeForNullSentinelValueChange(OpType optype)
	{
		if (optype != OpType.Distinct && optype != OpType.GroupBy && optype != OpType.Intersect)
		{
			return optype == OpType.Except;
		}
		return true;
	}

	internal static bool HasOpNotSafeForNullSentinelValueChange(Node n)
	{
		if (IsOpNotSafeForNullSentinelValueChange(n.Op.OpType))
		{
			return true;
		}
		foreach (Node child in n.Children)
		{
			if (HasOpNotSafeForNullSentinelValueChange(child))
			{
				return true;
			}
		}
		return false;
	}

	internal bool IsScalarOpTree(Node node, Dictionary<Var, int> varRefMap)
	{
		PlanCompiler.Assert(varRefMap != null, "Null varRef map");
		int nonLeafNodeCount = 0;
		return IsScalarOpTree(node, varRefMap, ref nonLeafNodeCount);
	}

	internal bool IncludeCustomFunctionOp(Node node, Dictionary<Var, Node> varMap)
	{
		if (!m_compilerState.DisableFilterOverProjectionSimplificationForCustomFunctions)
		{
			return false;
		}
		PlanCompiler.Assert(varMap != null, "Null varRef map");
		if (node.Op.OpType == OpType.VarRef)
		{
			VarRefOp varRefOp = (VarRefOp)node.Op;
			if (varMap.TryGetValue(varRefOp.Var, out var value))
			{
				return IncludeCustomFunctionOp(value, varMap);
			}
		}
		if (node.Op.OpType == OpType.Function && !(node.Op as FunctionOp).Function.BuiltInAttribute)
		{
			return true;
		}
		for (int i = 0; i < node.Children.Count; i++)
		{
			if (IncludeCustomFunctionOp(node.Children[i], varMap))
			{
				return true;
			}
		}
		return false;
	}

	internal Dictionary<Var, Node> GetVarMap(Node varDefListNode, Dictionary<Var, int> varRefMap)
	{
		_ = (VarDefListOp)varDefListNode.Op;
		Dictionary<Var, Node> dictionary = new Dictionary<Var, Node>();
		foreach (Node child in varDefListNode.Children)
		{
			VarDefOp varDefOp = (VarDefOp)child.Op;
			int nonLeafNodeCount = 0;
			int value = 0;
			if (!IsScalarOpTree(child.Child0, null, ref nonLeafNodeCount))
			{
				return null;
			}
			if (nonLeafNodeCount > 100 && varRefMap != null && varRefMap.TryGetValue(varDefOp.Var, out value) && value > 2)
			{
				return null;
			}
			if (dictionary.TryGetValue(varDefOp.Var, out var value2))
			{
				PlanCompiler.Assert(value2 == child.Child0, "reusing varDef for different Node?");
			}
			else
			{
				dictionary.Add(varDefOp.Var, child.Child0);
			}
		}
		return dictionary;
	}

	internal Node BuildNullIfExpression(Var conditionVar, Node expr)
	{
		VarRefOp op = base.Command.CreateVarRefOp(conditionVar);
		Node arg = base.Command.CreateNode(op);
		Node arg2 = base.Command.CreateNode(base.Command.CreateConditionalOp(OpType.IsNull), arg);
		Node arg3 = base.Command.CreateNode(base.Command.CreateNullOp(expr.Op.Type));
		return base.Command.CreateNode(base.Command.CreateCaseOp(expr.Op.Type), arg2, arg3, expr);
	}

	internal void SuppressFilterPushdown(Node n)
	{
		m_suppressions[n] = n;
	}

	internal bool IsFilterPushdownSuppressed(Node n)
	{
		return m_suppressions.ContainsKey(n);
	}

	internal static bool TryGetInt32Var(IEnumerable<Var> varList, out Var int32Var)
	{
		foreach (Var var in varList)
		{
			if (TypeHelpers.TryGetPrimitiveTypeKind(var.Type, out var typeKind) && typeKind == PrimitiveTypeKind.Int32)
			{
				int32Var = var;
				return true;
			}
		}
		int32Var = null;
		return false;
	}

	internal TransformationRulesContext(PlanCompiler compilerState)
		: base(compilerState.Command)
	{
		m_compilerState = compilerState;
		m_remapper = new VarRemapper(compilerState.Command);
		m_suppressions = new Dictionary<Node, Node>();
		m_remappedVars = compilerState.Command.CreateVarVec();
	}

	internal override void PreProcess(Node n)
	{
		m_remapper.RemapNode(n);
		base.Command.RecomputeNodeInfo(n);
	}

	internal override void PreProcessSubTree(Node subTree)
	{
		if (subTree.Op.IsRelOp)
		{
			m_relOpAncestors.Push(subTree);
		}
		if (m_remappedVars.IsEmpty)
		{
			return;
		}
		foreach (Var externalReference in base.Command.GetNodeInfo(subTree).ExternalReferences)
		{
			if (m_remappedVars.IsSet(externalReference))
			{
				m_remapper.RemapSubtree(subTree);
				break;
			}
		}
	}

	internal override void PostProcessSubTree(Node subtree)
	{
		if (subtree.Op.IsRelOp)
		{
			PlanCompiler.Assert(m_relOpAncestors.Count != 0, "The RelOp ancestors stack is empty when post processing a RelOp subtree");
			Node node = m_relOpAncestors.Pop();
			PlanCompiler.Assert(subtree == node, "The popped ancestor is not equal to the root of the subtree being post processed");
		}
	}

	internal override void PostProcess(Node n, System.Data.Entity.Core.Query.InternalTrees.Rule rule)
	{
		if (rule != null)
		{
			if (!m_projectionPruningRequired && TransformationRules.RulesRequiringProjectionPruning.Contains(rule))
			{
				m_projectionPruningRequired = true;
			}
			if (!m_reapplyNullabilityRules && TransformationRules.RulesRequiringNullabilityRulesToBeReapplied.Contains(rule))
			{
				m_reapplyNullabilityRules = true;
			}
			base.Command.RecomputeNodeInfo(n);
		}
	}

	internal override int GetHashCode(Node node)
	{
		return base.Command.GetNodeInfo(node).HashValue;
	}

	private bool IsScalarOpTree(Node node, Dictionary<Var, int> varRefMap, ref int nonLeafNodeCount)
	{
		if (!node.Op.IsScalarOp)
		{
			return false;
		}
		if (node.HasChild0)
		{
			nonLeafNodeCount++;
		}
		if (varRefMap != null && node.Op.OpType == OpType.VarRef)
		{
			VarRefOp varRefOp = (VarRefOp)node.Op;
			int value = ((!varRefMap.TryGetValue(varRefOp.Var, out value)) ? 1 : (value + 1));
			varRefMap[varRefOp.Var] = value;
		}
		foreach (Node child in node.Children)
		{
			if (!IsScalarOpTree(child, varRefMap, ref nonLeafNodeCount))
			{
				return false;
			}
		}
		return true;
	}
}
