using System.Collections.Generic;
using System.Data.Entity.Core.Common;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Core.Query.InternalTrees;

namespace System.Data.Entity.Core.Query.PlanCompiler;

internal class GroupAggregateVarComputationTranslator : BasicOpVisitorOfNode
{
	private GroupAggregateVarInfo _targetGroupAggregateVarInfo;

	private bool _isUnnested;

	private readonly Command _command;

	private readonly GroupAggregateVarInfoManager _groupAggregateVarInfoManager;

	private GroupAggregateVarComputationTranslator(Command command, GroupAggregateVarInfoManager groupAggregateVarInfoManager)
	{
		_command = command;
		_groupAggregateVarInfoManager = groupAggregateVarInfoManager;
	}

	public static bool TryTranslateOverGroupAggregateVar(Node subtree, bool isVarDefinition, Command command, GroupAggregateVarInfoManager groupAggregateVarInfoManager, out GroupAggregateVarInfo groupAggregateVarInfo, out Node templateNode, out bool isUnnested)
	{
		GroupAggregateVarComputationTranslator groupAggregateVarComputationTranslator = new GroupAggregateVarComputationTranslator(command, groupAggregateVarInfoManager);
		Node node = subtree;
		SoftCastOp softCastOp = null;
		if (node.Op.OpType == OpType.SoftCast)
		{
			softCastOp = (SoftCastOp)node.Op;
			node = node.Child0;
		}
		bool flag;
		if (node.Op.OpType == OpType.Collect)
		{
			templateNode = groupAggregateVarComputationTranslator.VisitCollect(node);
			flag = true;
		}
		else
		{
			templateNode = groupAggregateVarComputationTranslator.VisitNode(node);
			flag = false;
		}
		groupAggregateVarInfo = groupAggregateVarComputationTranslator._targetGroupAggregateVarInfo;
		isUnnested = groupAggregateVarComputationTranslator._isUnnested;
		if (groupAggregateVarComputationTranslator._targetGroupAggregateVarInfo == null || templateNode == null)
		{
			return false;
		}
		if (softCastOp != null)
		{
			SoftCastOp op = ((!flag && (isVarDefinition || !AggregatePushdownUtil.IsVarRefOverGivenVar(templateNode, groupAggregateVarComputationTranslator._targetGroupAggregateVarInfo.GroupAggregateVar))) ? softCastOp : command.CreateSoftCastOp(TypeHelpers.GetEdmType<CollectionType>(softCastOp.Type).TypeUsage));
			templateNode = command.CreateNode(op, templateNode);
		}
		return true;
	}

	public override Node Visit(VarRefOp op, Node n)
	{
		return TranslateOverGroupAggregateVar(op.Var, null);
	}

	public override Node Visit(PropertyOp op, Node n)
	{
		if (n.Child0.Op.OpType != OpType.VarRef)
		{
			return base.Visit(op, n);
		}
		VarRefOp varRefOp = (VarRefOp)n.Child0.Op;
		return TranslateOverGroupAggregateVar(varRefOp.Var, op.PropertyInfo);
	}

	private Node VisitCollect(Node n)
	{
		Node child = n.Child0;
		Dictionary<Var, Node> dictionary = new Dictionary<Var, Node>();
		while (child.Child0.Op.OpType == OpType.Project)
		{
			child = child.Child0;
			if (VisitDefault(child.Child1) == null)
			{
				return null;
			}
			foreach (Node child2 in child.Child1.Children)
			{
				if (IsConstant(child2.Child0))
				{
					dictionary.Add(((VarDefOp)child2.Op).Var, child2.Child0);
				}
			}
		}
		if (child.Child0.Op.OpType != OpType.Unnest)
		{
			return null;
		}
		UnnestOp unnestOp = (UnnestOp)child.Child0.Op;
		if (_groupAggregateVarInfoManager.TryGetReferencedGroupAggregateVarInfo(unnestOp.Var, out var groupAggregateVarRefInfo))
		{
			if (_targetGroupAggregateVarInfo == null)
			{
				_targetGroupAggregateVarInfo = groupAggregateVarRefInfo.GroupAggregateVarInfo;
			}
			else if (_targetGroupAggregateVarInfo != groupAggregateVarRefInfo.GroupAggregateVarInfo)
			{
				return null;
			}
			if (!_isUnnested)
			{
				return null;
			}
			PhysicalProjectOp obj = (PhysicalProjectOp)n.Child0.Op;
			PlanCompiler.Assert(obj.Outputs.Count == 1, "Physical project should only have one output at this stage");
			Var var = obj.Outputs[0];
			Node node = TranslateOverGroupAggregateVar(var, null);
			if (node != null)
			{
				_isUnnested = true;
				return node;
			}
			if (dictionary.TryGetValue(var, out var value))
			{
				_isUnnested = true;
				return value;
			}
			return null;
		}
		return null;
	}

	private static bool IsConstant(Node node)
	{
		Node node2 = node;
		while (node2.Op.OpType == OpType.Cast)
		{
			node2 = node2.Child0;
		}
		return PlanCompilerUtil.IsConstantBaseOp(node2.Op.OpType);
	}

	private Node TranslateOverGroupAggregateVar(Var var, EdmMember property)
	{
		EdmMember edmMember;
		if (_groupAggregateVarInfoManager.TryGetReferencedGroupAggregateVarInfo(var, out var groupAggregateVarRefInfo))
		{
			edmMember = property;
		}
		else
		{
			if (!_groupAggregateVarInfoManager.TryGetReferencedGroupAggregateVarInfo(var, property, out groupAggregateVarRefInfo))
			{
				return null;
			}
			edmMember = null;
		}
		if (_targetGroupAggregateVarInfo == null)
		{
			_targetGroupAggregateVarInfo = groupAggregateVarRefInfo.GroupAggregateVarInfo;
			_isUnnested = groupAggregateVarRefInfo.IsUnnested;
		}
		else if (_targetGroupAggregateVarInfo != groupAggregateVarRefInfo.GroupAggregateVarInfo || _isUnnested != groupAggregateVarRefInfo.IsUnnested)
		{
			return null;
		}
		Node node = groupAggregateVarRefInfo.Computation;
		if (edmMember != null)
		{
			node = _command.CreateNode(_command.CreatePropertyOp(edmMember), node);
		}
		return node;
	}

	protected override Node VisitDefault(Node n)
	{
		List<Node> list = new List<Node>(n.Children.Count);
		bool flag = false;
		for (int i = 0; i < n.Children.Count; i++)
		{
			Node node = VisitNode(n.Children[i]);
			if (node == null)
			{
				return null;
			}
			if (!flag && n.Children[i] != node)
			{
				flag = true;
			}
			list.Add(node);
		}
		if (!flag)
		{
			return n;
		}
		return _command.CreateNode(n.Op, list);
	}

	protected override Node VisitRelOpDefault(RelOp op, Node n)
	{
		return null;
	}

	public override Node Visit(AggregateOp op, Node n)
	{
		return null;
	}

	public override Node Visit(CollectOp op, Node n)
	{
		return null;
	}

	public override Node Visit(ElementOp op, Node n)
	{
		return null;
	}
}
