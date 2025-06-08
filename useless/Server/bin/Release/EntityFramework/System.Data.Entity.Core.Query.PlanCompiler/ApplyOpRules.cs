using System.Data.Entity.Core.Query.InternalTrees;

namespace System.Data.Entity.Core.Query.PlanCompiler;

internal static class ApplyOpRules
{
	internal class OutputCountVisitor : BasicOpVisitorOfT<int>
	{
		internal static int CountOutputs(Node node)
		{
			return new OutputCountVisitor().VisitNode(node);
		}

		internal new int VisitChildren(Node n)
		{
			int num = 0;
			foreach (Node child in n.Children)
			{
				num += VisitNode(child);
			}
			return num;
		}

		protected override int VisitDefault(Node n)
		{
			return VisitChildren(n);
		}

		protected override int VisitSetOp(SetOp op, Node n)
		{
			return op.Outputs.Count;
		}

		public override int Visit(DistinctOp op, Node n)
		{
			return op.Keys.Count;
		}

		public override int Visit(FilterOp op, Node n)
		{
			return VisitNode(n.Child0);
		}

		public override int Visit(GroupByOp op, Node n)
		{
			return op.Outputs.Count;
		}

		public override int Visit(ProjectOp op, Node n)
		{
			return op.Outputs.Count;
		}

		public override int Visit(ScanTableOp op, Node n)
		{
			return op.Table.Columns.Count;
		}

		public override int Visit(SingleRowTableOp op, Node n)
		{
			return 0;
		}

		protected override int VisitSortOp(SortBaseOp op, Node n)
		{
			return VisitNode(n.Child0);
		}
	}

	internal class VarDefinitionRemapper : VarRemapper
	{
		private readonly Var m_oldVar;

		private VarDefinitionRemapper(Var oldVar, Command command)
			: base(command)
		{
			m_oldVar = oldVar;
		}

		internal static void RemapSubtree(Node root, Command command, Var oldVar)
		{
			new VarDefinitionRemapper(oldVar, command).RemapSubtree(root);
		}

		internal override void RemapSubtree(Node subTree)
		{
			foreach (Node child in subTree.Children)
			{
				RemapSubtree(child);
			}
			VisitNode(subTree);
			m_command.RecomputeNodeInfo(subTree);
		}

		public override void Visit(VarDefOp op, Node n)
		{
			if (op.Var == m_oldVar)
			{
				Var var = m_command.CreateComputedVar(n.Child0.Op.Type);
				n.Op = m_command.CreateVarDefOp(var);
				AddMapping(m_oldVar, var);
			}
		}

		public override void Visit(ScanTableOp op, Node n)
		{
			if (op.Table.Columns.Contains(m_oldVar))
			{
				ScanTableOp scanTableOp = m_command.CreateScanTableOp(op.Table.TableMetadata);
				for (int i = 0; i < op.Table.Columns.Count; i++)
				{
					AddMapping(op.Table.Columns[i], scanTableOp.Table.Columns[i]);
				}
				n.Op = scanTableOp;
			}
		}

		protected override void VisitSetOp(SetOp op, Node n)
		{
			base.VisitSetOp(op, n);
			if (op.Outputs.IsSet(m_oldVar))
			{
				Var var = m_command.CreateSetOpVar(m_oldVar.Type);
				op.Outputs.Clear(m_oldVar);
				op.Outputs.Set(var);
				RemapVarMapKey(op.VarMap[0], var);
				RemapVarMapKey(op.VarMap[1], var);
				AddMapping(m_oldVar, var);
			}
		}

		private void RemapVarMapKey(VarMap varMap, Var newVar)
		{
			Var value = varMap[m_oldVar];
			varMap.Remove(m_oldVar);
			varMap.Add(newVar, value);
		}
	}

	internal static readonly PatternMatchRule Rule_CrossApplyOverFilter = new PatternMatchRule(new Node(CrossApplyOp.Pattern, new Node(LeafOp.Pattern), new Node(FilterOp.Pattern, new Node(LeafOp.Pattern), new Node(LeafOp.Pattern))), ProcessApplyOverFilter);

	internal static readonly PatternMatchRule Rule_OuterApplyOverFilter = new PatternMatchRule(new Node(OuterApplyOp.Pattern, new Node(LeafOp.Pattern), new Node(FilterOp.Pattern, new Node(LeafOp.Pattern), new Node(LeafOp.Pattern))), ProcessApplyOverFilter);

	internal static readonly PatternMatchRule Rule_OuterApplyOverProjectInternalConstantOverFilter = new PatternMatchRule(new Node(OuterApplyOp.Pattern, new Node(LeafOp.Pattern), new Node(ProjectOp.Pattern, new Node(FilterOp.Pattern, new Node(LeafOp.Pattern), new Node(LeafOp.Pattern)), new Node(VarDefListOp.Pattern, new Node(VarDefOp.Pattern, new Node(InternalConstantOp.Pattern))))), ProcessOuterApplyOverDummyProjectOverFilter);

	internal static readonly PatternMatchRule Rule_OuterApplyOverProjectNullSentinelOverFilter = new PatternMatchRule(new Node(OuterApplyOp.Pattern, new Node(LeafOp.Pattern), new Node(ProjectOp.Pattern, new Node(FilterOp.Pattern, new Node(LeafOp.Pattern), new Node(LeafOp.Pattern)), new Node(VarDefListOp.Pattern, new Node(VarDefOp.Pattern, new Node(NullSentinelOp.Pattern))))), ProcessOuterApplyOverDummyProjectOverFilter);

	internal static readonly PatternMatchRule Rule_CrossApplyOverProject = new PatternMatchRule(new Node(CrossApplyOp.Pattern, new Node(LeafOp.Pattern), new Node(ProjectOp.Pattern, new Node(LeafOp.Pattern), new Node(LeafOp.Pattern))), ProcessCrossApplyOverProject);

	internal static readonly PatternMatchRule Rule_OuterApplyOverProject = new PatternMatchRule(new Node(OuterApplyOp.Pattern, new Node(LeafOp.Pattern), new Node(ProjectOp.Pattern, new Node(LeafOp.Pattern), new Node(LeafOp.Pattern))), ProcessOuterApplyOverProject);

	internal static readonly PatternMatchRule Rule_CrossApplyOverAnything = new PatternMatchRule(new Node(CrossApplyOp.Pattern, new Node(LeafOp.Pattern), new Node(LeafOp.Pattern)), ProcessApplyOverAnything);

	internal static readonly PatternMatchRule Rule_OuterApplyOverAnything = new PatternMatchRule(new Node(OuterApplyOp.Pattern, new Node(LeafOp.Pattern), new Node(LeafOp.Pattern)), ProcessApplyOverAnything);

	internal static readonly PatternMatchRule Rule_CrossApplyIntoScalarSubquery = new PatternMatchRule(new Node(CrossApplyOp.Pattern, new Node(LeafOp.Pattern), new Node(LeafOp.Pattern)), ProcessApplyIntoScalarSubquery);

	internal static readonly PatternMatchRule Rule_OuterApplyIntoScalarSubquery = new PatternMatchRule(new Node(OuterApplyOp.Pattern, new Node(LeafOp.Pattern), new Node(LeafOp.Pattern)), ProcessApplyIntoScalarSubquery);

	internal static readonly PatternMatchRule Rule_CrossApplyOverLeftOuterJoinOverSingleRowTable = new PatternMatchRule(new Node(CrossApplyOp.Pattern, new Node(LeafOp.Pattern), new Node(LeftOuterJoinOp.Pattern, new Node(SingleRowTableOp.Pattern), new Node(LeafOp.Pattern), new Node(ConstantPredicateOp.Pattern))), ProcessCrossApplyOverLeftOuterJoinOverSingleRowTable);

	internal static readonly System.Data.Entity.Core.Query.InternalTrees.Rule[] Rules = new System.Data.Entity.Core.Query.InternalTrees.Rule[11]
	{
		Rule_CrossApplyOverAnything, Rule_CrossApplyOverFilter, Rule_CrossApplyOverProject, Rule_OuterApplyOverAnything, Rule_OuterApplyOverProjectInternalConstantOverFilter, Rule_OuterApplyOverProjectNullSentinelOverFilter, Rule_OuterApplyOverProject, Rule_OuterApplyOverFilter, Rule_CrossApplyOverLeftOuterJoinOverSingleRowTable, Rule_CrossApplyIntoScalarSubquery,
		Rule_OuterApplyIntoScalarSubquery
	};

	private static bool ProcessApplyOverFilter(RuleProcessingContext context, Node applyNode, out Node newNode)
	{
		newNode = applyNode;
		if (((TransformationRulesContext)context).PlanCompiler.TransformationsDeferred)
		{
			return false;
		}
		Node child = applyNode.Child1;
		Command command = context.Command;
		NodeInfo nodeInfo = command.GetNodeInfo(child.Child0);
		ExtendedNodeInfo extendedNodeInfo = command.GetExtendedNodeInfo(applyNode.Child0);
		if (nodeInfo.ExternalReferences.Overlaps(extendedNodeInfo.Definitions))
		{
			return false;
		}
		JoinBaseOp joinBaseOp = null;
		joinBaseOp = ((applyNode.Op.OpType != OpType.CrossApply) ? ((JoinBaseOp)command.CreateLeftOuterJoinOp()) : ((JoinBaseOp)command.CreateInnerJoinOp()));
		newNode = command.CreateNode(joinBaseOp, applyNode.Child0, child.Child0, child.Child1);
		return true;
	}

	private static bool ProcessOuterApplyOverDummyProjectOverFilter(RuleProcessingContext context, Node applyNode, out Node newNode)
	{
		newNode = applyNode;
		Node child = applyNode.Child1;
		ProjectOp projectOp = (ProjectOp)child.Op;
		Node child2 = child.Child0;
		Node child3 = child2.Child0;
		Command command = context.Command;
		ExtendedNodeInfo extendedNodeInfo = command.GetExtendedNodeInfo(child3);
		ExtendedNodeInfo extendedNodeInfo2 = command.GetExtendedNodeInfo(applyNode.Child0);
		if (projectOp.Outputs.Overlaps(extendedNodeInfo2.Definitions) || extendedNodeInfo.ExternalReferences.Overlaps(extendedNodeInfo2.Definitions))
		{
			return false;
		}
		bool flag = false;
		Node node = null;
		TransformationRulesContext transformationRulesContext = (TransformationRulesContext)context;
		bool flag2;
		if (TransformationRulesContext.TryGetInt32Var(extendedNodeInfo.NonNullableDefinitions, out var int32Var))
		{
			flag2 = true;
		}
		else
		{
			int32Var = extendedNodeInfo.NonNullableDefinitions.First;
			flag2 = false;
		}
		if (int32Var != null)
		{
			flag = true;
			Node child4 = child.Child1.Child0;
			if (child4.Child0.Op.OpType == OpType.NullSentinel && flag2 && transformationRulesContext.CanChangeNullSentinelValue)
			{
				child4.Child0 = context.Command.CreateNode(context.Command.CreateVarRefOp(int32Var));
			}
			else
			{
				child4.Child0 = transformationRulesContext.BuildNullIfExpression(int32Var, child4.Child0);
			}
			command.RecomputeNodeInfo(child4);
			command.RecomputeNodeInfo(child.Child1);
			node = child3;
		}
		else
		{
			node = child;
			foreach (Var externalReference in command.GetNodeInfo(child2.Child1).ExternalReferences)
			{
				if (extendedNodeInfo.Definitions.IsSet(externalReference))
				{
					projectOp.Outputs.Set(externalReference);
				}
			}
			child.Child0 = child3;
		}
		context.Command.RecomputeNodeInfo(child);
		Node node2 = command.CreateNode(command.CreateLeftOuterJoinOp(), applyNode.Child0, node, child2.Child1);
		if (flag)
		{
			ExtendedNodeInfo extendedNodeInfo3 = command.GetExtendedNodeInfo(node2);
			child.Child0 = node2;
			projectOp.Outputs.Or(extendedNodeInfo3.Definitions);
			newNode = child;
		}
		else
		{
			newNode = node2;
		}
		return true;
	}

	private static bool ProcessCrossApplyOverProject(RuleProcessingContext context, Node applyNode, out Node newNode)
	{
		newNode = applyNode;
		Node child = applyNode.Child1;
		ProjectOp projectOp = (ProjectOp)child.Op;
		Command command = context.Command;
		ExtendedNodeInfo extendedNodeInfo = command.GetExtendedNodeInfo(applyNode);
		VarVec varVec = command.CreateVarVec(projectOp.Outputs);
		varVec.Or(extendedNodeInfo.Definitions);
		projectOp.Outputs.InitFrom(varVec);
		applyNode.Child1 = child.Child0;
		context.Command.RecomputeNodeInfo(applyNode);
		child.Child0 = applyNode;
		newNode = child;
		return true;
	}

	private static bool ProcessOuterApplyOverProject(RuleProcessingContext context, Node applyNode, out Node newNode)
	{
		newNode = applyNode;
		Node child = applyNode.Child1;
		Node child2 = child.Child1;
		TransformationRulesContext transformationRulesContext = (TransformationRulesContext)context;
		Var computedVar = context.Command.GetExtendedNodeInfo(child.Child0).NonNullableDefinitions.First;
		if (computedVar == null && child2.Children.Count == 1 && (child2.Child0.Child0.Op.OpType == OpType.InternalConstant || child2.Child0.Child0.Op.OpType == OpType.NullSentinel))
		{
			return false;
		}
		Command command = context.Command;
		Node node = null;
		InternalConstantOp internalConstantOp = null;
		ExtendedNodeInfo extendedNodeInfo = command.GetExtendedNodeInfo(child.Child0);
		bool flag = false;
		foreach (Node child4 in child2.Children)
		{
			PlanCompiler.Assert(child4.Op.OpType == OpType.VarDef, "Expected VarDefOp. Found " + child4.Op.OpType.ToString() + " instead");
			if (!(child4.Child0.Op is VarRefOp varRefOp) || !extendedNodeInfo.Definitions.IsSet(varRefOp.Var))
			{
				if (computedVar == null)
				{
					internalConstantOp = command.CreateInternalConstantOp(command.IntegerType, 1);
					Node definingExpr = command.CreateNode(internalConstantOp);
					Node arg = command.CreateVarDefListNode(definingExpr, out computedVar);
					ProjectOp projectOp = command.CreateProjectOp(computedVar);
					projectOp.Outputs.Or(extendedNodeInfo.Definitions);
					node = command.CreateNode(projectOp, child.Child0, arg);
				}
				Node child3 = ((internalConstantOp == null || (!internalConstantOp.IsEquivalent(child4.Child0.Op) && child4.Child0.Op.OpType != OpType.NullSentinel)) ? transformationRulesContext.BuildNullIfExpression(computedVar, child4.Child0) : command.CreateNode(command.CreateVarRefOp(computedVar)));
				child4.Child0 = child3;
				command.RecomputeNodeInfo(child4);
				flag = true;
			}
		}
		if (flag)
		{
			command.RecomputeNodeInfo(child2);
		}
		applyNode.Child1 = ((node != null) ? node : child.Child0);
		command.RecomputeNodeInfo(applyNode);
		child.Child0 = applyNode;
		ExtendedNodeInfo extendedNodeInfo2 = command.GetExtendedNodeInfo(applyNode.Child0);
		((ProjectOp)child.Op).Outputs.Or(extendedNodeInfo2.Definitions);
		newNode = child;
		return true;
	}

	private static bool ProcessApplyOverAnything(RuleProcessingContext context, Node applyNode, out Node newNode)
	{
		newNode = applyNode;
		Node child = applyNode.Child0;
		Node child2 = applyNode.Child1;
		ApplyBaseOp applyBaseOp = (ApplyBaseOp)applyNode.Op;
		Command command = context.Command;
		ExtendedNodeInfo extendedNodeInfo = command.GetExtendedNodeInfo(child2);
		ExtendedNodeInfo extendedNodeInfo2 = command.GetExtendedNodeInfo(child);
		bool flag = false;
		if (applyBaseOp.OpType == OpType.OuterApply && (int)extendedNodeInfo.MinRows >= 1)
		{
			applyBaseOp = command.CreateCrossApplyOp();
			flag = true;
		}
		if (extendedNodeInfo.ExternalReferences.Overlaps(extendedNodeInfo2.Definitions))
		{
			if (flag)
			{
				newNode = command.CreateNode(applyBaseOp, child, child2);
				return true;
			}
			return false;
		}
		if (applyBaseOp.OpType == OpType.CrossApply)
		{
			newNode = command.CreateNode(command.CreateCrossJoinOp(), child, child2);
		}
		else
		{
			LeftOuterJoinOp op = command.CreateLeftOuterJoinOp();
			ConstantPredicateOp op2 = command.CreateTrueOp();
			Node arg = command.CreateNode(op2);
			newNode = command.CreateNode(op, child, child2, arg);
		}
		return true;
	}

	private static bool ProcessApplyIntoScalarSubquery(RuleProcessingContext context, Node applyNode, out Node newNode)
	{
		Command command = context.Command;
		ExtendedNodeInfo extendedNodeInfo = command.GetExtendedNodeInfo(applyNode.Child1);
		OpType opType = applyNode.Op.OpType;
		if (!CanRewriteApply(applyNode.Child1, extendedNodeInfo, opType))
		{
			newNode = applyNode;
			return false;
		}
		ExtendedNodeInfo extendedNodeInfo2 = command.GetExtendedNodeInfo(applyNode.Child0);
		Var first = extendedNodeInfo.Definitions.First;
		VarVec varVec = command.CreateVarVec(extendedNodeInfo2.Definitions);
		TransformationRulesContext obj = (TransformationRulesContext)context;
		obj.RemapSubtree(applyNode.Child1);
		VarDefinitionRemapper.RemapSubtree(applyNode.Child1, command, first);
		Node definingExpr = command.CreateNode(command.CreateElementOp(first.Type), applyNode.Child1);
		Var computedVar;
		Node arg = command.CreateVarDefListNode(definingExpr, out computedVar);
		varVec.Set(computedVar);
		newNode = command.CreateNode(command.CreateProjectOp(varVec), applyNode.Child0, arg);
		obj.AddVarMapping(first, computedVar);
		return true;
	}

	private static bool CanRewriteApply(Node rightChild, ExtendedNodeInfo applyRightChildNodeInfo, OpType applyKind)
	{
		if (applyRightChildNodeInfo.Definitions.Count != 1)
		{
			return false;
		}
		if (applyRightChildNodeInfo.MaxRows != RowCount.One)
		{
			return false;
		}
		if (applyKind == OpType.CrossApply && applyRightChildNodeInfo.MinRows != RowCount.One)
		{
			return false;
		}
		if (OutputCountVisitor.CountOutputs(rightChild) != 1)
		{
			return false;
		}
		return true;
	}

	private static bool ProcessCrossApplyOverLeftOuterJoinOverSingleRowTable(RuleProcessingContext context, Node applyNode, out Node newNode)
	{
		newNode = applyNode;
		Node child = applyNode.Child1;
		if (((ConstantPredicateOp)child.Child2.Op).IsFalse)
		{
			return false;
		}
		applyNode.Op = context.Command.CreateOuterApplyOp();
		applyNode.Child1 = child.Child1;
		return true;
	}
}
