using System.Collections.Generic;
using System.Data.Entity.Core.Query.InternalTrees;
using System.Linq;

namespace System.Data.Entity.Core.Query.PlanCompiler;

internal static class ProjectOpRules
{
	internal static readonly PatternMatchRule Rule_ProjectOverProject = new PatternMatchRule(new Node(ProjectOp.Pattern, new Node(ProjectOp.Pattern, new Node(LeafOp.Pattern), new Node(LeafOp.Pattern)), new Node(LeafOp.Pattern)), ProcessProjectOverProject);

	internal static readonly PatternMatchRule Rule_ProjectWithNoLocalDefs = new PatternMatchRule(new Node(ProjectOp.Pattern, new Node(LeafOp.Pattern), new Node(VarDefListOp.Pattern)), ProcessProjectWithNoLocalDefinitions);

	internal static readonly SimpleRule Rule_ProjectOpWithSimpleVarRedefinitions = new SimpleRule(OpType.Project, ProcessProjectWithSimpleVarRedefinitions);

	internal static readonly SimpleRule Rule_ProjectOpWithNullSentinel = new SimpleRule(OpType.Project, ProcessProjectOpWithNullSentinel);

	internal static readonly System.Data.Entity.Core.Query.InternalTrees.Rule[] Rules = new System.Data.Entity.Core.Query.InternalTrees.Rule[4] { Rule_ProjectOpWithNullSentinel, Rule_ProjectOpWithSimpleVarRedefinitions, Rule_ProjectOverProject, Rule_ProjectWithNoLocalDefs };

	private static bool ProcessProjectOverProject(RuleProcessingContext context, Node projectNode, out Node newNode)
	{
		newNode = projectNode;
		Node child = projectNode.Child1;
		Node child2 = projectNode.Child0;
		TransformationRulesContext transformationRulesContext = (TransformationRulesContext)context;
		Dictionary<Var, int> varRefMap = new Dictionary<Var, int>();
		foreach (Node child3 in child.Children)
		{
			if (!transformationRulesContext.IsScalarOpTree(child3.Child0, varRefMap))
			{
				return false;
			}
		}
		Dictionary<Var, Node> varMap = transformationRulesContext.GetVarMap(child2.Child1, varRefMap);
		if (varMap == null)
		{
			return false;
		}
		Node node = transformationRulesContext.Command.CreateNode(transformationRulesContext.Command.CreateVarDefListOp());
		foreach (Node child4 in child.Children)
		{
			child4.Child0 = transformationRulesContext.ReMap(child4.Child0, varMap);
			transformationRulesContext.Command.RecomputeNodeInfo(child4);
			node.Children.Add(child4);
		}
		ExtendedNodeInfo extendedNodeInfo = transformationRulesContext.Command.GetExtendedNodeInfo(projectNode);
		foreach (Node child5 in child2.Child1.Children)
		{
			VarDefOp varDefOp = (VarDefOp)child5.Op;
			if (extendedNodeInfo.Definitions.IsSet(varDefOp.Var))
			{
				node.Children.Add(child5);
			}
		}
		projectNode.Child0 = child2.Child0;
		projectNode.Child1 = node;
		return true;
	}

	private static bool ProcessProjectWithNoLocalDefinitions(RuleProcessingContext context, Node n, out Node newNode)
	{
		newNode = n;
		if (!context.Command.GetNodeInfo(n).ExternalReferences.IsEmpty)
		{
			return false;
		}
		newNode = n.Child0;
		return true;
	}

	private static bool ProcessProjectWithSimpleVarRedefinitions(RuleProcessingContext context, Node n, out Node newNode)
	{
		newNode = n;
		ProjectOp projectOp = (ProjectOp)n.Op;
		if (n.Child1.Children.Count == 0)
		{
			return false;
		}
		TransformationRulesContext transformationRulesContext = (TransformationRulesContext)context;
		Command command = transformationRulesContext.Command;
		ExtendedNodeInfo extendedNodeInfo = command.GetExtendedNodeInfo(n);
		bool flag = false;
		foreach (Node child3 in n.Child1.Children)
		{
			Node child = child3.Child0;
			if (child.Op.OpType == OpType.VarRef)
			{
				VarRefOp varRefOp = (VarRefOp)child.Op;
				if (!extendedNodeInfo.ExternalReferences.IsSet(varRefOp.Var))
				{
					flag = true;
					break;
				}
			}
		}
		if (!flag)
		{
			return false;
		}
		List<Node> list = new List<Node>();
		foreach (Node child4 in n.Child1.Children)
		{
			VarDefOp varDefOp = (VarDefOp)child4.Op;
			if (child4.Child0.Op is VarRefOp varRefOp2 && !extendedNodeInfo.ExternalReferences.IsSet(varRefOp2.Var))
			{
				projectOp.Outputs.Clear(varDefOp.Var);
				projectOp.Outputs.Set(varRefOp2.Var);
				transformationRulesContext.AddVarMapping(varDefOp.Var, varRefOp2.Var);
			}
			else
			{
				list.Add(child4);
			}
		}
		Node child2 = command.CreateNode(command.CreateVarDefListOp(), list);
		n.Child1 = child2;
		return true;
	}

	private static bool ProcessProjectOpWithNullSentinel(RuleProcessingContext context, Node n, out Node newNode)
	{
		newNode = n;
		ProjectOp projectOp = (ProjectOp)n.Op;
		if (n.Child1.Children.Where((Node c) => c.Child0.Op.OpType == OpType.NullSentinel).Count() == 0)
		{
			return false;
		}
		TransformationRulesContext transformationRulesContext = (TransformationRulesContext)context;
		Command command = transformationRulesContext.Command;
		ExtendedNodeInfo extendedNodeInfo = command.GetExtendedNodeInfo(n.Child0);
		bool flag = false;
		bool canChangeNullSentinelValue = transformationRulesContext.CanChangeNullSentinelValue;
		if (!canChangeNullSentinelValue || !TransformationRulesContext.TryGetInt32Var(extendedNodeInfo.NonNullableDefinitions, out var int32Var))
		{
			flag = true;
			if (!canChangeNullSentinelValue || !TransformationRulesContext.TryGetInt32Var(from child in n.Child1.Children
				where child.Child0.Op.OpType == OpType.Constant || child.Child0.Op.OpType == OpType.InternalConstant
				select ((VarDefOp)child.Op).Var, out int32Var))
			{
				int32Var = (from child in n.Child1.Children
					where child.Child0.Op.OpType == OpType.NullSentinel
					select ((VarDefOp)child.Op).Var).FirstOrDefault();
				if (int32Var == null)
				{
					return false;
				}
			}
		}
		bool flag2 = false;
		for (int num = n.Child1.Children.Count - 1; num >= 0; num--)
		{
			Node node = n.Child1.Children[num];
			if (node.Child0.Op.OpType == OpType.NullSentinel)
			{
				if (!flag)
				{
					VarRefOp op = command.CreateVarRefOp(int32Var);
					node.Child0 = command.CreateNode(op);
					command.RecomputeNodeInfo(node);
					flag2 = true;
				}
				else if (!int32Var.Equals(((VarDefOp)node.Op).Var))
				{
					projectOp.Outputs.Clear(((VarDefOp)node.Op).Var);
					n.Child1.Children.RemoveAt(num);
					transformationRulesContext.AddVarMapping(((VarDefOp)node.Op).Var, int32Var);
					flag2 = true;
				}
			}
		}
		if (flag2)
		{
			command.RecomputeNodeInfo(n.Child1);
		}
		return flag2;
	}
}
