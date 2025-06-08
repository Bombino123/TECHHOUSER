using System.Collections.Generic;
using System.Data.Entity.Core.Query.InternalTrees;
using System.Data.Entity.Resources;
using System.Linq;

namespace System.Data.Entity.Core.Query.PlanCompiler;

internal class NestPullup : BasicOpVisitorOfNode
{
	private readonly PlanCompiler m_compilerState;

	private readonly Dictionary<Var, Node> m_definingNodeMap = new Dictionary<Var, Node>();

	private readonly VarRemapper m_varRemapper;

	private readonly Dictionary<Var, Var> m_varRefMap = new Dictionary<Var, Var>();

	private bool m_foundSortUnderUnnest;

	private Command Command => m_compilerState.Command;

	private NestPullup(PlanCompiler compilerState)
	{
		m_compilerState = compilerState;
		m_varRemapper = new VarRemapper(compilerState.Command);
	}

	internal static void Process(PlanCompiler compilerState)
	{
		new NestPullup(compilerState).Process();
	}

	private void Process()
	{
		PlanCompiler.Assert(Command.Root.Op.OpType == OpType.PhysicalProject, "root node is not physicalProject?");
		Command.Root = VisitNode(Command.Root);
		if (m_foundSortUnderUnnest)
		{
			SortRemover.Process(Command);
		}
	}

	private static bool IsNestOpNode(Node n)
	{
		PlanCompiler.Assert(n.Op.OpType != OpType.SingleStreamNest, "illegal singleStreamNest?");
		if (n.Op.OpType != OpType.SingleStreamNest)
		{
			return n.Op.OpType == OpType.MultiStreamNest;
		}
		return true;
	}

	private Node NestingNotSupported(Op op, Node n)
	{
		VisitChildren(n);
		m_varRemapper.RemapNode(n);
		foreach (Node child in n.Children)
		{
			if (IsNestOpNode(child))
			{
				throw new NotSupportedException(Strings.ADP_NestingNotSupported(op.OpType.ToString(), child.Op.OpType.ToString()));
			}
		}
		return n;
	}

	private Var ResolveVarReference(Var refVar)
	{
		Var value = refVar;
		while (m_varRefMap.TryGetValue(value, out value))
		{
			refVar = value;
		}
		return refVar;
	}

	private void UpdateReplacementVarMap(IEnumerable<Var> fromVars, IEnumerable<Var> toVars)
	{
		IEnumerator<Var> enumerator = toVars.GetEnumerator();
		foreach (Var fromVar in fromVars)
		{
			if (!enumerator.MoveNext())
			{
				throw EntityUtil.InternalError(EntityUtil.InternalErrorCode.ColumnCountMismatch, 2, null);
			}
			m_varRemapper.AddMapping(fromVar, enumerator.Current);
		}
		if (enumerator.MoveNext())
		{
			throw EntityUtil.InternalError(EntityUtil.InternalErrorCode.ColumnCountMismatch, 3, null);
		}
	}

	private static void RemapSortKeys(List<SortKey> sortKeys, Dictionary<Var, Var> varMap)
	{
		if (sortKeys == null)
		{
			return;
		}
		foreach (SortKey sortKey in sortKeys)
		{
			if (varMap.TryGetValue(sortKey.Var, out var value))
			{
				sortKey.Var = value;
			}
		}
	}

	private static IEnumerable<Var> RemapVars(IEnumerable<Var> vars, Dictionary<Var, Var> varMap)
	{
		foreach (Var var in vars)
		{
			if (varMap.TryGetValue(var, out var value))
			{
				yield return value;
			}
			else
			{
				yield return var;
			}
		}
	}

	private static VarList RemapVarList(VarList varList, Dictionary<Var, Var> varMap)
	{
		return Command.CreateVarList(RemapVars(varList, varMap));
	}

	private VarVec RemapVarVec(VarVec varVec, Dictionary<Var, Var> varMap)
	{
		return Command.CreateVarVec(RemapVars(varVec, varMap));
	}

	public override Node Visit(VarDefOp op, Node n)
	{
		VisitChildren(n);
		m_varRemapper.RemapNode(n);
		if (n.Child0.Op.OpType == OpType.VarRef)
		{
			m_varRefMap.Add(op.Var, ((VarRefOp)n.Child0.Op).Var);
		}
		return n;
	}

	public override Node Visit(VarRefOp op, Node n)
	{
		VisitChildren(n);
		m_varRemapper.RemapNode(n);
		return n;
	}

	public override Node Visit(CaseOp op, Node n)
	{
		foreach (Node child in n.Children)
		{
			if (child.Op.OpType == OpType.Collect)
			{
				throw new NotSupportedException(Strings.ADP_NestingNotSupported(op.OpType.ToString(), child.Op.OpType.ToString()));
			}
			if (child.Op.OpType == OpType.VarRef)
			{
				Var var = ((VarRefOp)child.Op).Var;
				if (m_definingNodeMap.ContainsKey(var))
				{
					throw new NotSupportedException(Strings.ADP_NestingNotSupported(op.OpType.ToString(), child.Op.OpType.ToString()));
				}
			}
		}
		return VisitDefault(n);
	}

	public override Node Visit(ExistsOp op, Node n)
	{
		Var first = ((ProjectOp)n.Child0.Op).Outputs.First;
		VisitChildren(n);
		VarVec outputs = ((ProjectOp)n.Child0.Op).Outputs;
		if (outputs.Count > 1)
		{
			PlanCompiler.Assert(outputs.IsSet(first), "The constant var is not present after NestPull up over the input of ExistsOp.");
			outputs.Clear();
			outputs.Set(first);
		}
		return n;
	}

	protected override Node VisitRelOpDefault(RelOp op, Node n)
	{
		return NestingNotSupported(op, n);
	}

	private Node ApplyOpJoinOp(Op op, Node n)
	{
		VisitChildren(n);
		int num = 0;
		foreach (Node child in n.Children)
		{
			if (child.Op is NestBaseOp)
			{
				num++;
				if (OpType.SingleStreamNest == child.Op.OpType)
				{
					throw new InvalidOperationException(Strings.ADP_InternalProviderError(1012));
				}
			}
		}
		if (num == 0)
		{
			return n;
		}
		foreach (Node child2 in n.Children)
		{
			if (op.OpType != OpType.MultiStreamNest && child2.Op.IsRelOp)
			{
				KeyVec keyVec = Command.PullupKeys(child2);
				if (keyVec == null || keyVec.NoKeys)
				{
					throw new NotSupportedException(Strings.ADP_KeysRequiredForJoinOverNest(op.OpType.ToString()));
				}
			}
		}
		List<Node> list = new List<Node>();
		List<Node> list2 = new List<Node>();
		List<CollectionInfo> list3 = new List<CollectionInfo>();
		foreach (Node child3 in n.Children)
		{
			if (child3.Op.OpType == OpType.MultiStreamNest)
			{
				list3.AddRange(((MultiStreamNestOp)child3.Op).CollectionInfo);
				if (op.OpType == OpType.FullOuterJoin || ((op.OpType == OpType.LeftOuterJoin || op.OpType == OpType.OuterApply) && n.Child1.Op.OpType == OpType.MultiStreamNest))
				{
					Var constantVar = null;
					list2.Add(AugmentNodeWithConstant(child3.Child0, () => Command.CreateNullSentinelOp(), out constantVar));
					foreach (CollectionInfo item2 in ((MultiStreamNestOp)child3.Op).CollectionInfo)
					{
						m_definingNodeMap[item2.CollectionVar].Child0 = ApplyIsNotNullFilter(m_definingNodeMap[item2.CollectionVar].Child0, constantVar);
					}
					for (int i = 1; i < child3.Children.Count; i++)
					{
						Node item = ApplyIsNotNullFilter(child3.Children[i], constantVar);
						list.Add(item);
					}
				}
				else
				{
					list2.Add(child3.Child0);
					for (int j = 1; j < child3.Children.Count; j++)
					{
						list.Add(child3.Children[j]);
					}
				}
			}
			else
			{
				list2.Add(child3);
			}
		}
		Node node = Command.CreateNode(op, list2);
		list.Insert(0, node);
		ExtendedNodeInfo extendedNodeInfo = node.GetExtendedNodeInfo(Command);
		VarVec varVec = Command.CreateVarVec(extendedNodeInfo.Definitions);
		foreach (CollectionInfo item3 in list3)
		{
			varVec.Set(item3.CollectionVar);
		}
		NestBaseOp op2 = Command.CreateMultiStreamNestOp(new List<SortKey>(), varVec, list3);
		return Command.CreateNode(op2, list);
	}

	private Node ApplyIsNotNullFilter(Node node, Var sentinelVar)
	{
		Node node2 = node;
		Node node3 = null;
		while (node2.Op.OpType == OpType.MultiStreamNest)
		{
			node3 = node2;
			node2 = node2.Child0;
		}
		Node node4 = CapWithIsNotNullFilter(node2, sentinelVar);
		if (node3 != null)
		{
			node3.Child0 = node4;
			return node;
		}
		return node4;
	}

	private Node CapWithIsNotNullFilter(Node input, Var var)
	{
		Node arg = Command.CreateNode(Command.CreateVarRefOp(var));
		Node arg2 = Command.CreateNode(Command.CreateConditionalOp(OpType.Not), Command.CreateNode(Command.CreateConditionalOp(OpType.IsNull), arg));
		return Command.CreateNode(Command.CreateFilterOp(), input, arg2);
	}

	protected override Node VisitApplyOp(ApplyBaseOp op, Node n)
	{
		return ApplyOpJoinOp(op, n);
	}

	public override Node Visit(DistinctOp op, Node n)
	{
		return NestingNotSupported(op, n);
	}

	public override Node Visit(FilterOp op, Node n)
	{
		VisitChildren(n);
		if (n.Child0.Op is NestBaseOp)
		{
			Node child = n.Child0;
			Node child2 = child.Child0;
			n.Child0 = child2;
			child.Child0 = n;
			Command.RecomputeNodeInfo(n);
			Command.RecomputeNodeInfo(child);
			return child;
		}
		return n;
	}

	public override Node Visit(GroupByOp op, Node n)
	{
		return NestingNotSupported(op, n);
	}

	public override Node Visit(GroupByIntoOp op, Node n)
	{
		PlanCompiler.Assert(n.HasChild3 && n.Child3.Children.Count > 0, "GroupByIntoOp with no group aggregates?");
		Node child = n.Child3;
		VarVec vars = Command.CreateVarVec(op.Outputs);
		VarVec outputs = op.Outputs;
		foreach (Node child2 in child.Children)
		{
			VarDefOp varDefOp = child2.Op as VarDefOp;
			outputs.Clear(varDefOp.Var);
		}
		Node arg = Command.CreateNode(Command.CreateGroupByOp(op.Keys, outputs), n.Child0, n.Child1, n.Child2);
		Node n2 = Command.CreateNode(Command.CreateProjectOp(vars), arg, child);
		return VisitNode(n2);
	}

	protected override Node VisitJoinOp(JoinBaseOp op, Node n)
	{
		return ApplyOpJoinOp(op, n);
	}

	public override Node Visit(ProjectOp op, Node n)
	{
		VisitChildren(n);
		m_varRemapper.RemapNode(n);
		if (n.Child0.Op.OpType == OpType.Sort)
		{
			Node child = n.Child0;
			foreach (SortKey key in ((SortOp)child.Op).Keys)
			{
				if (!Command.GetExtendedNodeInfo(child).ExternalReferences.IsSet(key.Var))
				{
					op.Outputs.Set(key.Var);
				}
			}
			n.Child0 = child.Child0;
			Command.RecomputeNodeInfo(n);
			child.Child0 = HandleProjectNode(n);
			Command.RecomputeNodeInfo(child);
			return child;
		}
		return HandleProjectNode(n);
	}

	private Node HandleProjectNode(Node n)
	{
		Node node = ProjectOpCase1(n);
		if (node.Op.OpType == OpType.Project && IsNestOpNode(node.Child0))
		{
			node = ProjectOpCase2(node);
		}
		return MergeNestedNestOps(node);
	}

	private Node MergeNestedNestOps(Node nestNode)
	{
		if (!IsNestOpNode(nestNode) || !IsNestOpNode(nestNode.Child0))
		{
			return nestNode;
		}
		NestBaseOp nestBaseOp = (NestBaseOp)nestNode.Op;
		Node child = nestNode.Child0;
		NestBaseOp nestBaseOp2 = (NestBaseOp)child.Op;
		VarVec varVec = Command.CreateVarVec();
		foreach (CollectionInfo item in nestBaseOp.CollectionInfo)
		{
			varVec.Set(item.CollectionVar);
		}
		List<Node> list = new List<Node>();
		List<CollectionInfo> list2 = new List<CollectionInfo>();
		VarVec varVec2 = Command.CreateVarVec(nestBaseOp.Outputs);
		list.Add(child.Child0);
		for (int i = 1; i < child.Children.Count; i++)
		{
			CollectionInfo collectionInfo = nestBaseOp2.CollectionInfo[i - 1];
			if (varVec.IsSet(collectionInfo.CollectionVar) || varVec2.IsSet(collectionInfo.CollectionVar))
			{
				list2.Add(collectionInfo);
				list.Add(child.Children[i]);
				PlanCompiler.Assert(varVec2.IsSet(collectionInfo.CollectionVar), "collectionVar not in output Vars?");
			}
		}
		for (int j = 1; j < nestNode.Children.Count; j++)
		{
			CollectionInfo collectionInfo2 = nestBaseOp.CollectionInfo[j - 1];
			list2.Add(collectionInfo2);
			list.Add(nestNode.Children[j]);
			PlanCompiler.Assert(varVec2.IsSet(collectionInfo2.CollectionVar), "collectionVar not in output Vars?");
		}
		List<SortKey> list3 = ConsolidateSortKeys(nestBaseOp.PrefixSortKeys, nestBaseOp2.PrefixSortKeys);
		foreach (SortKey item2 in list3)
		{
			varVec2.Set(item2.Var);
		}
		MultiStreamNestOp op = Command.CreateMultiStreamNestOp(list3, varVec2, list2);
		Node node = Command.CreateNode(op, list);
		Command.RecomputeNodeInfo(node);
		return node;
	}

	private Node ProjectOpCase1(Node projectNode)
	{
		ProjectOp projectOp = (ProjectOp)projectNode.Op;
		List<CollectionInfo> collectionInfoList = new List<CollectionInfo>();
		List<Node> list = new List<Node>();
		List<Node> list2 = new List<Node>();
		VarVec varVec = Command.CreateVarVec();
		VarVec varVec2 = Command.CreateVarVec();
		List<Node> list3 = new List<Node>();
		List<Node> list4 = new List<Node>();
		foreach (Node child3 in projectNode.Child1.Children)
		{
			VarDefOp varDefOp = (VarDefOp)child3.Op;
			Node child = child3.Child0;
			if (OpType.Collect == child.Op.OpType)
			{
				PlanCompiler.Assert(child.HasChild0, "collect without input?");
				PlanCompiler.Assert(OpType.PhysicalProject == child.Child0.Op.OpType, "collect without physicalProject?");
				Node child2 = child.Child0;
				m_definingNodeMap.Add(varDefOp.Var, child2);
				ConvertToNestOpInput(child2, varDefOp.Var, collectionInfoList, list2, varVec, varVec2);
			}
			else if (OpType.VarRef == child.Op.OpType)
			{
				Var var = ((VarRefOp)child.Op).Var;
				if (m_definingNodeMap.TryGetValue(var, out var value))
				{
					value = CopyCollectionVarDefinition(value);
					m_definingNodeMap.Add(varDefOp.Var, value);
					ConvertToNestOpInput(value, varDefOp.Var, collectionInfoList, list2, varVec, varVec2);
				}
				else
				{
					list4.Add(child3);
					list.Add(child3);
				}
			}
			else
			{
				list3.Add(child3);
				list.Add(child3);
			}
		}
		if (list2.Count == 0)
		{
			return projectNode;
		}
		VarVec varVec3 = Command.CreateVarVec(projectOp.Outputs);
		VarVec varVec4 = Command.CreateVarVec(projectOp.Outputs);
		varVec4.Minus(varVec2);
		varVec4.Or(varVec);
		if (!varVec4.IsEmpty)
		{
			if (IsNestOpNode(projectNode.Child0))
			{
				if (list3.Count == 0 && list4.Count == 0)
				{
					projectNode = projectNode.Child0;
					EnsureReferencedVarsAreRemoved(list4, varVec3);
				}
				else
				{
					NestBaseOp nestBaseOp = (NestBaseOp)projectNode.Child0.Op;
					List<Node> list5 = new List<Node>();
					list5.Add(projectNode.Child0.Child0);
					list4.AddRange(list3);
					list5.Add(Command.CreateNode(Command.CreateVarDefListOp(), list4));
					VarVec varVec5 = Command.CreateVarVec(nestBaseOp.Outputs);
					foreach (CollectionInfo item2 in nestBaseOp.CollectionInfo)
					{
						varVec5.Clear(item2.CollectionVar);
					}
					foreach (Node item3 in list4)
					{
						varVec5.Set(((VarDefOp)item3.Op).Var);
					}
					Node item = Command.CreateNode(Command.CreateProjectOp(varVec5), list5);
					VarVec varVec6 = Command.CreateVarVec(varVec5);
					varVec6.Or(nestBaseOp.Outputs);
					MultiStreamNestOp op = Command.CreateMultiStreamNestOp(nestBaseOp.PrefixSortKeys, varVec6, nestBaseOp.CollectionInfo);
					List<Node> list6 = new List<Node>();
					list6.Add(item);
					for (int i = 1; i < projectNode.Child0.Children.Count; i++)
					{
						list6.Add(projectNode.Child0.Children[i]);
					}
					projectNode = Command.CreateNode(op, list6);
				}
			}
			else
			{
				ProjectOp op2 = Command.CreateProjectOp(varVec4);
				projectNode.Child1 = Command.CreateNode(projectNode.Child1.Op, list);
				projectNode.Op = op2;
				EnsureReferencedVarsAreRemapped(list4);
			}
		}
		else
		{
			projectNode = projectNode.Child0;
			EnsureReferencedVarsAreRemoved(list4, varVec3);
		}
		varVec.And(projectNode.GetExtendedNodeInfo(Command).Definitions);
		varVec3.Or(varVec);
		MultiStreamNestOp op3 = Command.CreateMultiStreamNestOp(new List<SortKey>(), varVec3, collectionInfoList);
		list2.Insert(0, projectNode);
		Node node = Command.CreateNode(op3, list2);
		Command.RecomputeNodeInfo(projectNode);
		Command.RecomputeNodeInfo(node);
		return node;
	}

	private void EnsureReferencedVarsAreRemoved(List<Node> referencedVars, VarVec outputVars)
	{
		foreach (Node referencedVar in referencedVars)
		{
			Var var = ((VarDefOp)referencedVar.Op).Var;
			Var var2 = ResolveVarReference(var);
			m_varRemapper.AddMapping(var, var2);
			outputVars.Clear(var);
			outputVars.Set(var2);
		}
	}

	private void EnsureReferencedVarsAreRemapped(List<Node> referencedVars)
	{
		foreach (Node referencedVar in referencedVars)
		{
			Var var = ((VarDefOp)referencedVar.Op).Var;
			Var oldVar = ResolveVarReference(var);
			m_varRemapper.AddMapping(oldVar, var);
		}
	}

	private void ConvertToNestOpInput(Node physicalProjectNode, Var collectionVar, List<CollectionInfo> collectionInfoList, List<Node> collectionNodes, VarVec externalReferences, VarVec collectionReferences)
	{
		externalReferences.Or(Command.GetNodeInfo(physicalProjectNode).ExternalReferences);
		Node child = physicalProjectNode.Child0;
		PhysicalProjectOp physicalProjectOp = (PhysicalProjectOp)physicalProjectNode.Op;
		VarList varList = Command.CreateVarList(physicalProjectOp.Outputs);
		VarVec varVec = Command.CreateVarVec(varList);
		List<SortKey> list = null;
		if (OpType.Sort == child.Op.OpType)
		{
			SortOp sortOp = (SortOp)child.Op;
			list = OpCopier.Copy(Command, sortOp.Keys);
			foreach (SortKey item2 in list)
			{
				if (!varVec.IsSet(item2.Var))
				{
					varList.Add(item2.Var);
					varVec.Set(item2.Var);
				}
			}
		}
		else
		{
			list = new List<SortKey>();
		}
		VarVec keyVars = Command.GetExtendedNodeInfo(child).Keys.KeyVars;
		VarVec varVec2 = keyVars.Clone();
		varVec2.Minus(varVec);
		VarVec keys = (varVec2.IsEmpty ? keyVars.Clone() : Command.CreateVarVec());
		CollectionInfo item = Command.CreateCollectionInfo(collectionVar, physicalProjectOp.ColumnMap.Element, varList, keys, list, null);
		collectionInfoList.Add(item);
		collectionNodes.Add(child);
		collectionReferences.Set(collectionVar);
	}

	private Node ProjectOpCase2(Node projectNode)
	{
		ProjectOp projectOp = (ProjectOp)projectNode.Op;
		Node child = projectNode.Child0;
		NestBaseOp nestBaseOp = child.Op as NestBaseOp;
		VarVec varVec = Command.CreateVarVec();
		foreach (CollectionInfo item2 in nestBaseOp.CollectionInfo)
		{
			varVec.Set(item2.CollectionVar);
		}
		VarVec varVec2 = Command.CreateVarVec(nestBaseOp.Outputs);
		varVec2.Minus(varVec);
		VarVec varVec3 = Command.CreateVarVec(projectOp.Outputs);
		varVec3.Minus(varVec);
		VarVec varVec4 = Command.CreateVarVec(projectOp.Outputs);
		varVec4.Minus(varVec3);
		VarVec varVec5 = Command.CreateVarVec(varVec);
		varVec5.Minus(varVec4);
		List<CollectionInfo> list;
		List<Node> list2;
		if (varVec5.IsEmpty)
		{
			list = nestBaseOp.CollectionInfo;
			list2 = new List<Node>(child.Children);
		}
		else
		{
			list = new List<CollectionInfo>();
			list2 = new List<Node>();
			list2.Add(child.Child0);
			int num = 1;
			foreach (CollectionInfo item3 in nestBaseOp.CollectionInfo)
			{
				if (!varVec5.IsSet(item3.CollectionVar))
				{
					list.Add(item3);
					list2.Add(child.Children[num]);
				}
				num++;
			}
		}
		VarVec varVec6 = Command.CreateVarVec();
		for (int i = 1; i < child.Children.Count; i++)
		{
			varVec6.Or(child.Children[i].GetExtendedNodeInfo(Command).ExternalReferences);
		}
		varVec6.And(child.Child0.GetExtendedNodeInfo(Command).Definitions);
		VarVec varVec7 = Command.CreateVarVec(varVec3);
		varVec7.Or(varVec2);
		varVec7.Or(varVec6);
		List<Node> list3 = new List<Node>(projectNode.Child1.Children.Count);
		foreach (Node child2 in projectNode.Child1.Children)
		{
			VarDefOp varDefOp = (VarDefOp)child2.Op;
			if (varVec7.IsSet(varDefOp.Var))
			{
				list3.Add(child2);
			}
		}
		if (list.Count != 0 && varVec7.IsEmpty)
		{
			PlanCompiler.Assert(list3.Count == 0, "outputs is empty with non-zero count of children?");
			NullOp op = Command.CreateNullOp(Command.StringType);
			Node definingExpr = Command.CreateNode(op);
			Var computedVar;
			Node item = Command.CreateVarDefNode(definingExpr, out computedVar);
			list3.Add(item);
			varVec7.Set(computedVar);
		}
		projectNode.Op = Command.CreateProjectOp(Command.CreateVarVec(varVec7));
		projectNode.Child1 = Command.CreateNode(projectNode.Child1.Op, list3);
		if (list.Count == 0)
		{
			projectNode.Child0 = child.Child0;
			child = projectNode;
		}
		else
		{
			VarVec varVec8 = Command.CreateVarVec(projectOp.Outputs);
			for (int j = 1; j < list2.Count; j++)
			{
				varVec8.Or(list2[j].GetNodeInfo(Command).ExternalReferences);
			}
			foreach (SortKey prefixSortKey in nestBaseOp.PrefixSortKeys)
			{
				varVec8.Set(prefixSortKey.Var);
			}
			child.Op = Command.CreateMultiStreamNestOp(nestBaseOp.PrefixSortKeys, varVec8, list);
			child = Command.CreateNode(child.Op, list2);
			projectNode.Child0 = child.Child0;
			child.Child0 = projectNode;
			Command.RecomputeNodeInfo(projectNode);
		}
		Command.RecomputeNodeInfo(child);
		return child;
	}

	protected override Node VisitSetOp(SetOp op, Node n)
	{
		return NestingNotSupported(op, n);
	}

	public override Node Visit(SingleRowOp op, Node n)
	{
		VisitChildren(n);
		if (IsNestOpNode(n.Child0))
		{
			n = n.Child0;
			Node child = Command.CreateNode(op, n.Child0);
			n.Child0 = child;
			Command.RecomputeNodeInfo(n);
		}
		return n;
	}

	public override Node Visit(SortOp op, Node n)
	{
		VisitChildren(n);
		m_varRemapper.RemapNode(n);
		if (n.Child0.Op is NestBaseOp inputNestOp)
		{
			n.Child0.Op = GetNestOpWithConsolidatedSortKeys(inputNestOp, op.Keys);
			return n.Child0;
		}
		return n;
	}

	public override Node Visit(ConstrainedSortOp op, Node n)
	{
		VisitChildren(n);
		if (n.Child0.Op is NestBaseOp inputNestOp)
		{
			Node child = n.Child0;
			n.Child0 = child.Child0;
			child.Child0 = n;
			child.Op = GetNestOpWithConsolidatedSortKeys(inputNestOp, op.Keys);
			n = child;
		}
		return n;
	}

	private NestBaseOp GetNestOpWithConsolidatedSortKeys(NestBaseOp inputNestOp, List<SortKey> sortKeys)
	{
		if (inputNestOp.PrefixSortKeys.Count == 0)
		{
			foreach (SortKey sortKey in sortKeys)
			{
				inputNestOp.PrefixSortKeys.Add(Command.CreateSortKey(sortKey.Var, sortKey.AscendingSort, sortKey.Collation));
			}
			return inputNestOp;
		}
		List<SortKey> prefixSortKeys = ConsolidateSortKeys(sortKeys, inputNestOp.PrefixSortKeys);
		PlanCompiler.Assert(inputNestOp is MultiStreamNestOp, "Unexpected SingleStreamNestOp?");
		return Command.CreateMultiStreamNestOp(prefixSortKeys, inputNestOp.Outputs, inputNestOp.CollectionInfo);
	}

	private List<SortKey> ConsolidateSortKeys(List<SortKey> sortKeyList1, List<SortKey> sortKeyList2)
	{
		VarVec varVec = Command.CreateVarVec();
		List<SortKey> list = new List<SortKey>();
		foreach (SortKey item in sortKeyList1)
		{
			if (!varVec.IsSet(item.Var))
			{
				varVec.Set(item.Var);
				list.Add(Command.CreateSortKey(item.Var, item.AscendingSort, item.Collation));
			}
		}
		foreach (SortKey item2 in sortKeyList2)
		{
			if (!varVec.IsSet(item2.Var))
			{
				varVec.Set(item2.Var);
				list.Add(Command.CreateSortKey(item2.Var, item2.AscendingSort, item2.Collation));
			}
		}
		return list;
	}

	public override Node Visit(UnnestOp op, Node n)
	{
		VisitChildren(n);
		PlanCompiler.Assert(n.Child0.Op.OpType == OpType.VarDef, "Un-nest without VarDef input?");
		PlanCompiler.Assert(((VarDefOp)n.Child0.Op).Var == op.Var, "Un-nest var not found?");
		PlanCompiler.Assert(n.Child0.HasChild0, "VarDef without input?");
		Node child = n.Child0.Child0;
		if (OpType.Function == child.Op.OpType)
		{
			return n;
		}
		if (OpType.Collect == child.Op.OpType)
		{
			PlanCompiler.Assert(child.HasChild0, "collect without input?");
			child = child.Child0;
			PlanCompiler.Assert(child.Op.OpType == OpType.PhysicalProject, "collect without physicalProject?");
			m_definingNodeMap.Add(op.Var, child);
		}
		else
		{
			if (OpType.VarRef != child.Op.OpType)
			{
				throw EntityUtil.InternalError(EntityUtil.InternalErrorCode.InvalidInternalTree, 2, child.Op.OpType);
			}
			Var var = ((VarRefOp)child.Op).Var;
			PlanCompiler.Assert(m_definingNodeMap.TryGetValue(var, out var value), "Could not find a definition for a referenced collection var");
			child = CopyCollectionVarDefinition(value);
			PlanCompiler.Assert(child.Op.OpType == OpType.PhysicalProject, "driving node is not physicalProject?");
		}
		IEnumerable<Var> outputs = ((PhysicalProjectOp)child.Op).Outputs;
		PlanCompiler.Assert(child.HasChild0, "physicalProject without input?");
		child = child.Child0;
		if (child.Op.OpType == OpType.Sort)
		{
			m_foundSortUnderUnnest = true;
		}
		UpdateReplacementVarMap(op.Table.Columns, outputs);
		return child;
	}

	private Node CopyCollectionVarDefinition(Node refVarDefiningNode)
	{
		VarMap varMap;
		Dictionary<Var, Node> newCollectionVarDefinitions;
		Node result = OpCopierTrackingCollectionVars.Copy(Command, refVarDefiningNode, out varMap, out newCollectionVarDefinitions);
		if (newCollectionVarDefinitions.Count != 0)
		{
			VarMap reverseMap = varMap.GetReverseMap();
			foreach (KeyValuePair<Var, Node> item in newCollectionVarDefinitions)
			{
				Var key = reverseMap[item.Key];
				if (m_definingNodeMap.TryGetValue(key, out var value))
				{
					PhysicalProjectOp physicalProjectOp = (PhysicalProjectOp)value.Op;
					VarList outputVars = VarRemapper.RemapVarList(Command, varMap, physicalProjectOp.Outputs);
					SimpleCollectionColumnMap columnMap = (SimpleCollectionColumnMap)ColumnMapCopier.Copy(physicalProjectOp.ColumnMap, varMap);
					PhysicalProjectOp op = Command.CreatePhysicalProjectOp(outputVars, columnMap);
					Node value2 = Command.CreateNode(op, item.Value);
					m_definingNodeMap.Add(item.Key, value2);
				}
			}
		}
		return result;
	}

	protected override Node VisitNestOp(NestBaseOp op, Node n)
	{
		VisitChildren(n);
		foreach (Node child in n.Children)
		{
			if (IsNestOpNode(child))
			{
				throw new InvalidOperationException(Strings.ADP_InternalProviderError(1002));
			}
		}
		return n;
	}

	public override Node Visit(PhysicalProjectOp op, Node n)
	{
		PlanCompiler.Assert(n.Children.Count == 1, "multiple inputs to physicalProject?");
		VisitChildren(n);
		m_varRemapper.RemapNode(n);
		if (n != Command.Root || !IsNestOpNode(n.Child0))
		{
			return n;
		}
		Node child = n.Child0;
		Dictionary<Var, ColumnMap> dictionary = new Dictionary<Var, ColumnMap>();
		VarList varList = Command.CreateVarList(op.Outputs.Where((Var v) => v.VarType == VarType.Parameter));
		child = ConvertToSingleStreamNest(child, dictionary, varList, out var parentKeyColumnMaps);
		SingleStreamNestOp ssnOp = (SingleStreamNestOp)child.Op;
		Node child2 = BuildSortForNestElimination(ssnOp, child);
		SimpleCollectionColumnMap simpleCollectionColumnMap = (SimpleCollectionColumnMap)ColumnMapTranslator.Translate(((PhysicalProjectOp)n.Op).ColumnMap, dictionary);
		simpleCollectionColumnMap = new SimpleCollectionColumnMap(simpleCollectionColumnMap.Type, simpleCollectionColumnMap.Name, simpleCollectionColumnMap.Element, parentKeyColumnMaps, null);
		n.Op = Command.CreatePhysicalProjectOp(varList, simpleCollectionColumnMap);
		n.Child0 = child2;
		return n;
	}

	private Node BuildSortForNestElimination(SingleStreamNestOp ssnOp, Node nestNode)
	{
		List<SortKey> list = BuildSortKeyList(ssnOp);
		if (list.Count > 0)
		{
			SortOp op = Command.CreateSortOp(list);
			return Command.CreateNode(op, nestNode.Child0);
		}
		return nestNode.Child0;
	}

	private List<SortKey> BuildSortKeyList(SingleStreamNestOp ssnOp)
	{
		VarVec varVec = Command.CreateVarVec();
		List<SortKey> list = new List<SortKey>();
		foreach (SortKey prefixSortKey in ssnOp.PrefixSortKeys)
		{
			if (!varVec.IsSet(prefixSortKey.Var))
			{
				varVec.Set(prefixSortKey.Var);
				list.Add(prefixSortKey);
			}
		}
		foreach (Var key in ssnOp.Keys)
		{
			if (!varVec.IsSet(key))
			{
				varVec.Set(key);
				SortKey item = Command.CreateSortKey(key);
				list.Add(item);
			}
		}
		PlanCompiler.Assert(!varVec.IsSet(ssnOp.Discriminator), "prefix sort on discriminator?");
		list.Add(Command.CreateSortKey(ssnOp.Discriminator));
		foreach (SortKey postfixSortKey in ssnOp.PostfixSortKeys)
		{
			if (!varVec.IsSet(postfixSortKey.Var))
			{
				varVec.Set(postfixSortKey.Var);
				list.Add(postfixSortKey);
			}
		}
		return list;
	}

	private Node ConvertToSingleStreamNest(Node nestNode, Dictionary<Var, ColumnMap> varRefReplacementMap, VarList flattenedOutputVarList, out SimpleColumnMap[] parentKeyColumnMaps)
	{
		MultiStreamNestOp multiStreamNestOp = (MultiStreamNestOp)nestNode.Op;
		for (int i = 1; i < nestNode.Children.Count; i++)
		{
			Node node = nestNode.Children[i];
			if (node.Op.OpType == OpType.MultiStreamNest)
			{
				CollectionInfo collectionInfo = multiStreamNestOp.CollectionInfo[i - 1];
				VarList varList = Command.CreateVarList();
				nestNode.Children[i] = ConvertToSingleStreamNest(node, varRefReplacementMap, varList, out var _);
				ColumnMap columnMap = ColumnMapTranslator.Translate(collectionInfo.ColumnMap, varRefReplacementMap);
				VarVec keys = Command.CreateVarVec(((SingleStreamNestOp)nestNode.Children[i].Op).Keys);
				multiStreamNestOp.CollectionInfo[i - 1] = Command.CreateCollectionInfo(collectionInfo.CollectionVar, columnMap, varList, keys, collectionInfo.SortKeys, null);
			}
		}
		Node child = nestNode.Child0;
		KeyVec keyVec = Command.PullupKeys(child);
		if (keyVec.NoKeys)
		{
			throw new NotSupportedException(Strings.ADP_KeysRequiredForNesting);
		}
		VarList varList2 = Command.CreateVarList(Command.GetExtendedNodeInfo(child).Definitions);
		NormalizeNestOpInputs(multiStreamNestOp, nestNode, out var discriminatorVarList, out var sortKeys);
		Var discriminatorVar;
		List<Dictionary<Var, Var>> varMapList;
		Node arg = BuildUnionAllSubqueryForNestOp(multiStreamNestOp, nestNode, varList2, discriminatorVarList, out discriminatorVar, out varMapList);
		Dictionary<Var, Var> dictionary = varMapList[0];
		flattenedOutputVarList.AddRange(RemapVars(varList2, dictionary));
		VarVec varVec = Command.CreateVarVec(flattenedOutputVarList);
		VarVec varVec2 = Command.CreateVarVec(varVec);
		foreach (KeyValuePair<Var, Var> item in dictionary)
		{
			if (item.Key != item.Value)
			{
				varRefReplacementMap[item.Key] = new VarRefColumnMap(item.Value);
			}
		}
		RemapSortKeys(multiStreamNestOp.PrefixSortKeys, dictionary);
		List<SortKey> list = new List<SortKey>();
		List<CollectionInfo> list2 = new List<CollectionInfo>();
		VarRefColumnMap discriminator = new VarRefColumnMap(discriminatorVar);
		varVec2.Set(discriminatorVar);
		if (!varVec.IsSet(discriminatorVar))
		{
			flattenedOutputVarList.Add(discriminatorVar);
			varVec.Set(discriminatorVar);
		}
		VarVec varVec3 = RemapVarVec(keyVec.KeyVars, dictionary);
		parentKeyColumnMaps = new SimpleColumnMap[varVec3.Count];
		int num = 0;
		foreach (Var item2 in varVec3)
		{
			parentKeyColumnMaps[num] = new VarRefColumnMap(item2);
			num++;
			if (!varVec.IsSet(item2))
			{
				flattenedOutputVarList.Add(item2);
				varVec.Set(item2);
			}
		}
		for (int j = 1; j < nestNode.Children.Count; j++)
		{
			CollectionInfo collectionInfo2 = multiStreamNestOp.CollectionInfo[j - 1];
			List<SortKey> list3 = sortKeys[j];
			RemapSortKeys(list3, varMapList[j]);
			list.AddRange(list3);
			ColumnMap columnMap2 = ColumnMapTranslator.Translate(collectionInfo2.ColumnMap, varMapList[j]);
			VarList varList3 = RemapVarList(collectionInfo2.FlattenedElementVars, varMapList[j]);
			VarVec keys2 = RemapVarVec(collectionInfo2.Keys, varMapList[j]);
			RemapSortKeys(collectionInfo2.SortKeys, varMapList[j]);
			CollectionInfo collectionInfo3 = Command.CreateCollectionInfo(collectionInfo2.CollectionVar, columnMap2, varList3, keys2, collectionInfo2.SortKeys, j);
			list2.Add(collectionInfo3);
			foreach (Var item3 in varList3)
			{
				if (!varVec.IsSet(item3))
				{
					flattenedOutputVarList.Add(item3);
					varVec.Set(item3);
				}
			}
			varVec2.Set(collectionInfo2.CollectionVar);
			int num2 = 0;
			SimpleColumnMap[] array = new SimpleColumnMap[collectionInfo3.Keys.Count];
			foreach (Var key in collectionInfo3.Keys)
			{
				array[num2] = new VarRefColumnMap(key);
				num2++;
			}
			DiscriminatedCollectionColumnMap value = new DiscriminatedCollectionColumnMap(TypeUtils.CreateCollectionType(collectionInfo3.ColumnMap.Type), collectionInfo3.ColumnMap.Name, collectionInfo3.ColumnMap, array, parentKeyColumnMaps, discriminator, collectionInfo3.DiscriminatorValue);
			varRefReplacementMap[collectionInfo2.CollectionVar] = value;
		}
		SingleStreamNestOp op = Command.CreateSingleStreamNestOp(varVec3, multiStreamNestOp.PrefixSortKeys, list, varVec2, list2, discriminatorVar);
		return Command.CreateNode(op, arg);
	}

	private void NormalizeNestOpInputs(NestBaseOp nestOp, Node nestNode, out VarList discriminatorVarList, out List<List<SortKey>> sortKeys)
	{
		discriminatorVarList = Command.CreateVarList();
		discriminatorVarList.Add(null);
		sortKeys = new List<List<SortKey>>();
		sortKeys.Add(nestOp.PrefixSortKeys);
		for (int i = 1; i < nestNode.Children.Count; i++)
		{
			Node node = nestNode.Children[i];
			if (node.Op is SingleStreamNestOp ssnOp)
			{
				List<SortKey> item = BuildSortKeyList(ssnOp);
				sortKeys.Add(item);
				node = node.Child0;
			}
			else if (node.Op is SortOp sortOp)
			{
				node = node.Child0;
				sortKeys.Add(sortOp.Keys);
			}
			else
			{
				sortKeys.Add(new List<SortKey>());
			}
			VarList flattenedElementVars = nestOp.CollectionInfo[i - 1].FlattenedElementVars;
			foreach (SortKey item2 in sortKeys[i])
			{
				if (!flattenedElementVars.Contains(item2.Var))
				{
					flattenedElementVars.Add(item2.Var);
				}
			}
			Var internalConstantVar;
			Node value = AugmentNodeWithInternalIntegerConstant(node, i, out internalConstantVar);
			nestNode.Children[i] = value;
			discriminatorVarList.Add(internalConstantVar);
		}
	}

	private Node AugmentNodeWithInternalIntegerConstant(Node input, int value, out Var internalConstantVar)
	{
		return AugmentNodeWithConstant(input, () => Command.CreateInternalConstantOp(Command.IntegerType, value), out internalConstantVar);
	}

	private Node AugmentNodeWithConstant(Node input, Func<ConstantBaseOp> createOp, out Var constantVar)
	{
		ConstantBaseOp op = createOp();
		Node definingExpr = Command.CreateNode(op);
		Node arg = Command.CreateVarDefListNode(definingExpr, out constantVar);
		ExtendedNodeInfo extendedNodeInfo = Command.GetExtendedNodeInfo(input);
		VarVec varVec = Command.CreateVarVec(extendedNodeInfo.Definitions);
		varVec.Set(constantVar);
		ProjectOp op2 = Command.CreateProjectOp(varVec);
		return Command.CreateNode(op2, input, arg);
	}

	private Node BuildUnionAllSubqueryForNestOp(NestBaseOp nestOp, Node nestNode, VarList drivingNodeVars, VarList discriminatorVarList, out Var discriminatorVar, out List<Dictionary<Var, Var>> varMapList)
	{
		Node child = nestNode.Child0;
		Node node = null;
		VarList varList = null;
		for (int i = 1; i < nestNode.Children.Count; i++)
		{
			Node arg;
			VarList newVarList;
			VarList collection;
			Op op;
			if (i > 1)
			{
				arg = OpCopier.Copy(Command, child, drivingNodeVars, out newVarList);
				VarRemapper varRemapper = new VarRemapper(Command);
				for (int j = 0; j < drivingNodeVars.Count; j++)
				{
					varRemapper.AddMapping(drivingNodeVars[j], newVarList[j]);
				}
				varRemapper.RemapSubtree(nestNode.Children[i]);
				collection = varRemapper.RemapVarList(nestOp.CollectionInfo[i - 1].FlattenedElementVars);
				op = Command.CreateCrossApplyOp();
			}
			else
			{
				arg = child;
				newVarList = drivingNodeVars;
				collection = nestOp.CollectionInfo[i - 1].FlattenedElementVars;
				op = Command.CreateOuterApplyOp();
			}
			Node arg2 = Command.CreateNode(op, arg, nestNode.Children[i]);
			List<Node> list = new List<Node>();
			VarList varList2 = Command.CreateVarList();
			varList2.Add(discriminatorVarList[i]);
			varList2.AddRange(newVarList);
			for (int k = 1; k < nestNode.Children.Count; k++)
			{
				CollectionInfo collectionInfo = nestOp.CollectionInfo[k - 1];
				if (i == k)
				{
					varList2.AddRange(collection);
					continue;
				}
				foreach (Var flattenedElementVar in collectionInfo.FlattenedElementVars)
				{
					NullOp op2 = Command.CreateNullOp(flattenedElementVar.Type);
					Node definingExpr = Command.CreateNode(op2);
					Var computedVar;
					Node item = Command.CreateVarDefNode(definingExpr, out computedVar);
					list.Add(item);
					varList2.Add(computedVar);
				}
			}
			Node arg3 = Command.CreateNode(Command.CreateVarDefListOp(), list);
			VarVec vars = Command.CreateVarVec(varList2);
			ProjectOp op3 = Command.CreateProjectOp(vars);
			Node node2 = Command.CreateNode(op3, arg2, arg3);
			if (node == null)
			{
				node = node2;
				varList = varList2;
				continue;
			}
			VarMap varMap = new VarMap();
			VarMap varMap2 = new VarMap();
			for (int l = 0; l < varList.Count; l++)
			{
				Var key = Command.CreateSetOpVar(varList[l].Type);
				varMap.Add(key, varList[l]);
				varMap2.Add(key, varList2[l]);
			}
			UnionAllOp unionAllOp = Command.CreateUnionAllOp(varMap, varMap2);
			node = Command.CreateNode(unionAllOp, node, node2);
			varList = GetUnionOutputs(unionAllOp, varList);
		}
		varMapList = new List<Dictionary<Var, Var>>();
		IEnumerator<Var> enumerator2 = varList.GetEnumerator();
		if (!enumerator2.MoveNext())
		{
			throw EntityUtil.InternalError(EntityUtil.InternalErrorCode.ColumnCountMismatch, 4, null);
		}
		discriminatorVar = enumerator2.Current;
		for (int m = 0; m < nestNode.Children.Count; m++)
		{
			Dictionary<Var, Var> dictionary = new Dictionary<Var, Var>();
			foreach (Var item2 in (m == 0) ? drivingNodeVars : nestOp.CollectionInfo[m - 1].FlattenedElementVars)
			{
				if (!enumerator2.MoveNext())
				{
					throw EntityUtil.InternalError(EntityUtil.InternalErrorCode.ColumnCountMismatch, 5, null);
				}
				dictionary[item2] = enumerator2.Current;
			}
			varMapList.Add(dictionary);
		}
		if (enumerator2.MoveNext())
		{
			throw EntityUtil.InternalError(EntityUtil.InternalErrorCode.ColumnCountMismatch, 6, null);
		}
		return node;
	}

	private static VarList GetUnionOutputs(UnionAllOp unionOp, VarList leftVars)
	{
		IDictionary<Var, Var> reverseMap = unionOp.VarMap[0].GetReverseMap();
		VarList varList = Command.CreateVarList();
		foreach (Var leftVar in leftVars)
		{
			Var item = reverseMap[leftVar];
			varList.Add(item);
		}
		return varList;
	}
}
