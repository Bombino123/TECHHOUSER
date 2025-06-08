using System.Collections.Generic;
using System.Data.Entity.Core.Query.InternalTrees;

namespace System.Data.Entity.Core.Query.PlanCompiler;

internal class Predicate
{
	private readonly Command m_command;

	private readonly List<Node> m_parts;

	internal Predicate(Command command)
	{
		m_command = command;
		m_parts = new List<Node>();
	}

	internal Predicate(Command command, Node andTree)
		: this(command)
	{
		PlanCompiler.Assert(andTree != null, "null node passed to Predicate() constructor");
		InitFromAndTree(andTree);
	}

	internal void AddPart(Node n)
	{
		m_parts.Add(n);
	}

	internal Node BuildAndTree()
	{
		Node node = null;
		foreach (Node part in m_parts)
		{
			node = ((node != null) ? m_command.CreateNode(m_command.CreateConditionalOp(OpType.And), node, part) : part);
		}
		return node;
	}

	internal Predicate GetSingleTablePredicates(VarVec tableDefinitions, out Predicate otherPredicates)
	{
		List<VarVec> list = new List<VarVec>();
		list.Add(tableDefinitions);
		GetSingleTablePredicates(list, out var singleTablePredicates, out otherPredicates);
		return singleTablePredicates[0];
	}

	internal void GetEquiJoinPredicates(VarVec leftTableDefinitions, VarVec rightTableDefinitions, out List<Var> leftTableEquiJoinColumns, out List<Var> rightTableEquiJoinColumns, out Predicate otherPredicates)
	{
		otherPredicates = new Predicate(m_command);
		leftTableEquiJoinColumns = new List<Var>();
		rightTableEquiJoinColumns = new List<Var>();
		foreach (Node part in m_parts)
		{
			if (IsEquiJoinPredicate(part, leftTableDefinitions, rightTableDefinitions, out var leftVar, out var rightVar))
			{
				leftTableEquiJoinColumns.Add(leftVar);
				rightTableEquiJoinColumns.Add(rightVar);
			}
			else
			{
				otherPredicates.AddPart(part);
			}
		}
	}

	internal Predicate GetJoinPredicates(VarVec leftTableDefinitions, VarVec rightTableDefinitions, out Predicate otherPredicates)
	{
		Predicate predicate = new Predicate(m_command);
		otherPredicates = new Predicate(m_command);
		foreach (Node part in m_parts)
		{
			if (IsEquiJoinPredicate(part, leftTableDefinitions, rightTableDefinitions, out var _, out var _))
			{
				predicate.AddPart(part);
			}
			else
			{
				otherPredicates.AddPart(part);
			}
		}
		return predicate;
	}

	internal bool SatisfiesKey(VarVec keyVars, VarVec definitions)
	{
		if (keyVars.Count > 0)
		{
			VarVec varVec = keyVars.Clone();
			foreach (Node part in m_parts)
			{
				if (part.Op.OpType == OpType.EQ)
				{
					if (IsKeyPredicate(part.Child0, part.Child1, keyVars, definitions, out var keyVar))
					{
						varVec.Clear(keyVar);
					}
					else if (IsKeyPredicate(part.Child1, part.Child0, keyVars, definitions, out keyVar))
					{
						varVec.Clear(keyVar);
					}
				}
			}
			return varVec.IsEmpty;
		}
		return false;
	}

	internal bool PreservesNulls(VarVec tableColumns, bool ansiNullSemantics)
	{
		if (!ansiNullSemantics)
		{
			return true;
		}
		foreach (Node part in m_parts)
		{
			if (!PreservesNulls(part, tableColumns))
			{
				return false;
			}
		}
		return true;
	}

	private void InitFromAndTree(Node andTree)
	{
		if (andTree.Op.OpType == OpType.And)
		{
			InitFromAndTree(andTree.Child0);
			InitFromAndTree(andTree.Child1);
		}
		else
		{
			m_parts.Add(andTree);
		}
	}

	private void GetSingleTablePredicates(List<VarVec> tableDefinitions, out List<Predicate> singleTablePredicates, out Predicate otherPredicates)
	{
		singleTablePredicates = new List<Predicate>();
		foreach (VarVec tableDefinition in tableDefinitions)
		{
			_ = tableDefinition;
			singleTablePredicates.Add(new Predicate(m_command));
		}
		otherPredicates = new Predicate(m_command);
		VarVec varVec = m_command.CreateVarVec();
		foreach (Node part in m_parts)
		{
			NodeInfo nodeInfo = m_command.GetNodeInfo(part);
			bool flag = false;
			for (int i = 0; i < tableDefinitions.Count; i++)
			{
				VarVec varVec2 = tableDefinitions[i];
				if (varVec2 != null)
				{
					varVec.InitFrom(nodeInfo.ExternalReferences);
					varVec.Minus(varVec2);
					if (varVec.IsEmpty)
					{
						flag = true;
						singleTablePredicates[i].AddPart(part);
						break;
					}
				}
			}
			if (!flag)
			{
				otherPredicates.AddPart(part);
			}
		}
	}

	private static bool IsEquiJoinPredicate(Node simplePredicateNode, out Var leftVar, out Var rightVar)
	{
		leftVar = null;
		rightVar = null;
		if (simplePredicateNode.Op.OpType != OpType.EQ)
		{
			return false;
		}
		if (!(simplePredicateNode.Child0.Op is VarRefOp varRefOp))
		{
			return false;
		}
		if (!(simplePredicateNode.Child1.Op is VarRefOp varRefOp2))
		{
			return false;
		}
		leftVar = varRefOp.Var;
		rightVar = varRefOp2.Var;
		return true;
	}

	private static bool IsEquiJoinPredicate(Node simplePredicateNode, VarVec leftTableDefinitions, VarVec rightTableDefinitions, out Var leftVar, out Var rightVar)
	{
		leftVar = null;
		rightVar = null;
		if (!IsEquiJoinPredicate(simplePredicateNode, out var leftVar2, out var rightVar2))
		{
			return false;
		}
		if (leftTableDefinitions.IsSet(leftVar2) && rightTableDefinitions.IsSet(rightVar2))
		{
			leftVar = leftVar2;
			rightVar = rightVar2;
		}
		else
		{
			if (!leftTableDefinitions.IsSet(rightVar2) || !rightTableDefinitions.IsSet(leftVar2))
			{
				return false;
			}
			leftVar = rightVar2;
			rightVar = leftVar2;
		}
		return true;
	}

	private static bool PreservesNulls(Node simplePredNode, VarVec tableColumns)
	{
		switch (simplePredNode.Op.OpType)
		{
		case OpType.GT:
		case OpType.GE:
		case OpType.LE:
		case OpType.LT:
		case OpType.EQ:
		case OpType.NE:
			if (simplePredNode.Child0.Op is VarRefOp varRefOp2 && tableColumns.IsSet(varRefOp2.Var))
			{
				return false;
			}
			if (simplePredNode.Child1.Op is VarRefOp varRefOp3 && tableColumns.IsSet(varRefOp3.Var))
			{
				return false;
			}
			return true;
		case OpType.Not:
			if (simplePredNode.Child0.Op.OpType != OpType.IsNull)
			{
				return true;
			}
			if (simplePredNode.Child0.Child0.Op is VarRefOp varRefOp4)
			{
				return !tableColumns.IsSet(varRefOp4.Var);
			}
			return true;
		case OpType.Like:
			if (!(simplePredNode.Child1.Op is ConstantBaseOp { OpType: not OpType.Null }))
			{
				return true;
			}
			if (simplePredNode.Child0.Op is VarRefOp varRefOp && tableColumns.IsSet(varRefOp.Var))
			{
				return false;
			}
			return true;
		default:
			return true;
		}
	}

	private bool IsKeyPredicate(Node left, Node right, VarVec keyVars, VarVec definitions, out Var keyVar)
	{
		keyVar = null;
		if (left.Op.OpType != OpType.VarRef)
		{
			return false;
		}
		VarRefOp varRefOp = (VarRefOp)left.Op;
		keyVar = varRefOp.Var;
		if (!keyVars.IsSet(keyVar))
		{
			return false;
		}
		VarVec varVec = m_command.GetNodeInfo(right).ExternalReferences.Clone();
		varVec.And(definitions);
		return varVec.IsEmpty;
	}
}
