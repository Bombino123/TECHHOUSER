using System.Collections.Generic;
using System.Data.Entity.Core.Query.InternalTrees;

namespace System.Data.Entity.Core.Query.PlanCompiler;

internal class VarRefManager
{
	private readonly Dictionary<Node, Node> m_nodeToParentMap;

	private readonly Dictionary<Node, int> m_nodeToSiblingNumber;

	private readonly Command m_command;

	internal VarRefManager(Command command)
	{
		m_nodeToParentMap = new Dictionary<Node, Node>();
		m_nodeToSiblingNumber = new Dictionary<Node, int>();
		m_command = command;
	}

	internal void AddChildren(Node parent)
	{
		for (int i = 0; i < parent.Children.Count; i++)
		{
			m_nodeToParentMap[parent.Children[i]] = parent;
			m_nodeToSiblingNumber[parent.Children[i]] = i;
		}
	}

	internal bool HasKeyReferences(VarVec keys, Node definingNode, Node targetJoinNode)
	{
		Node key = definingNode;
		bool continueUp = true;
		Node value;
		while (continueUp & m_nodeToParentMap.TryGetValue(key, out value))
		{
			if (value != targetJoinNode)
			{
				if (HasVarReferencesShallow(value, keys, m_nodeToSiblingNumber[key], out continueUp))
				{
					return true;
				}
				for (int i = m_nodeToSiblingNumber[key] + 1; i < value.Children.Count; i++)
				{
					if (value.Children[i].GetNodeInfo(m_command).ExternalReferences.Overlaps(keys))
					{
						return true;
					}
				}
			}
			key = value;
		}
		return false;
	}

	private static bool HasVarReferencesShallow(Node node, VarVec vars, int childIndex, out bool continueUp)
	{
		switch (node.Op.OpType)
		{
		case OpType.Sort:
		case OpType.ConstrainedSort:
			continueUp = true;
			return HasVarReferences(((SortBaseOp)node.Op).Keys, vars);
		case OpType.Distinct:
			continueUp = false;
			return HasVarReferences(((DistinctOp)node.Op).Keys, vars);
		case OpType.UnionAll:
		case OpType.Intersect:
		case OpType.Except:
			continueUp = false;
			return HasVarReferences((SetOp)node.Op, vars, childIndex);
		case OpType.GroupBy:
			continueUp = false;
			return HasVarReferences(((GroupByOp)node.Op).Keys, vars);
		case OpType.PhysicalProject:
			continueUp = false;
			return HasVarReferences(((PhysicalProjectOp)node.Op).Outputs, vars);
		case OpType.Project:
			continueUp = false;
			return HasVarReferences(((ProjectOp)node.Op).Outputs, vars);
		default:
			continueUp = true;
			return false;
		}
	}

	private static bool HasVarReferences(VarList listToCheck, VarVec vars)
	{
		foreach (Var var in vars)
		{
			if (listToCheck.Contains(var))
			{
				return true;
			}
		}
		return false;
	}

	private static bool HasVarReferences(VarVec listToCheck, VarVec vars)
	{
		return listToCheck.Overlaps(vars);
	}

	private static bool HasVarReferences(List<SortKey> listToCheck, VarVec vars)
	{
		foreach (SortKey item in listToCheck)
		{
			if (vars.IsSet(item.Var))
			{
				return true;
			}
		}
		return false;
	}

	private static bool HasVarReferences(SetOp op, VarVec vars, int index)
	{
		foreach (Var value in op.VarMap[index].Values)
		{
			if (vars.IsSet(value))
			{
				return true;
			}
		}
		return false;
	}
}
