using System.Collections.Generic;
using System.Data.Entity.Core.Query.InternalTrees;

namespace System.Data.Entity.Core.Query.PlanCompiler;

internal class GroupAggregateRefComputingVisitor : BasicOpVisitor
{
	private readonly Command _command;

	private readonly GroupAggregateVarInfoManager _groupAggregateVarInfoManager = new GroupAggregateVarInfoManager();

	private readonly Dictionary<Node, Node> _childToParent = new Dictionary<Node, Node>();

	internal static IEnumerable<GroupAggregateVarInfo> Process(Command itree, out TryGetValue tryGetParent)
	{
		GroupAggregateRefComputingVisitor groupAggregateRefComputingVisitor = new GroupAggregateRefComputingVisitor(itree);
		groupAggregateRefComputingVisitor.VisitNode(itree.Root);
		tryGetParent = groupAggregateRefComputingVisitor._childToParent.TryGetValue;
		return groupAggregateRefComputingVisitor._groupAggregateVarInfoManager.GroupAggregateVarInfos;
	}

	private GroupAggregateRefComputingVisitor(Command itree)
	{
		_command = itree;
	}

	public override void Visit(VarDefOp op, Node n)
	{
		VisitDefault(n);
		Node child = n.Child0;
		Op op2 = child.Op;
		if (GroupAggregateVarComputationTranslator.TryTranslateOverGroupAggregateVar(child, isVarDefinition: true, _command, _groupAggregateVarInfoManager, out var groupAggregateVarInfo, out var templateNode, out var isUnnested))
		{
			_groupAggregateVarInfoManager.Add(op.Var, groupAggregateVarInfo, templateNode, isUnnested);
		}
		else
		{
			if (op2.OpType != OpType.NewRecord)
			{
				return;
			}
			NewRecordOp newRecordOp = (NewRecordOp)op2;
			for (int i = 0; i < child.Children.Count; i++)
			{
				if (GroupAggregateVarComputationTranslator.TryTranslateOverGroupAggregateVar(child.Children[i], isVarDefinition: true, _command, _groupAggregateVarInfoManager, out groupAggregateVarInfo, out templateNode, out isUnnested))
				{
					_groupAggregateVarInfoManager.Add(op.Var, groupAggregateVarInfo, templateNode, isUnnested, newRecordOp.Properties[i]);
				}
			}
		}
	}

	public override void Visit(GroupByIntoOp op, Node n)
	{
		VisitGroupByOp(op, n);
		foreach (Node child in n.Child3.Children)
		{
			Var var = ((VarDefOp)child.Op).Var;
			if (!_groupAggregateVarInfoManager.TryGetReferencedGroupAggregateVarInfo(var, out var _))
			{
				_groupAggregateVarInfoManager.Add(var, new GroupAggregateVarInfo(n, var), _command.CreateNode(_command.CreateVarRefOp(var)), isUnnested: false);
			}
		}
	}

	public override void Visit(UnnestOp op, Node n)
	{
		VisitDefault(n);
		if (_groupAggregateVarInfoManager.TryGetReferencedGroupAggregateVarInfo(op.Var, out var groupAggregateVarRefInfo))
		{
			PlanCompiler.Assert(op.Table.Columns.Count == 1, "Expected one column before NTE");
			_groupAggregateVarInfoManager.Add(op.Table.Columns[0], groupAggregateVarRefInfo.GroupAggregateVarInfo, groupAggregateVarRefInfo.Computation, isUnnested: true);
		}
	}

	public override void Visit(FunctionOp op, Node n)
	{
		VisitDefault(n);
		if (PlanCompilerUtil.IsCollectionAggregateFunction(op, n) && n.Children.Count <= 1)
		{
			_ = n.Child0;
			if (GroupAggregateVarComputationTranslator.TryTranslateOverGroupAggregateVar(n.Child0, isVarDefinition: false, _command, _groupAggregateVarInfoManager, out var groupAggregateVarInfo, out var templateNode, out var isUnnested) && (isUnnested || AggregatePushdownUtil.IsVarRefOverGivenVar(templateNode, groupAggregateVarInfo.GroupAggregateVar)))
			{
				groupAggregateVarInfo.CandidateAggregateNodes.Add(new KeyValuePair<Node, List<Node>>(n, new List<Node> { templateNode }));
			}
		}
	}

	protected override void VisitDefault(Node n)
	{
		VisitChildren(n);
		foreach (Node child in n.Children)
		{
			if (child.Op.Arity != 0)
			{
				_childToParent.Add(child, n);
			}
		}
	}
}
