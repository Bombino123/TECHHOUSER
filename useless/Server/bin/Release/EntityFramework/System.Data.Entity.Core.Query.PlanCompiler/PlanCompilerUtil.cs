using System.Collections.Generic;
using System.Data.Entity.Core.Common.Utils;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Core.Query.InternalTrees;

namespace System.Data.Entity.Core.Query.PlanCompiler;

internal static class PlanCompilerUtil
{
	internal static bool IsRowTypeCaseOpWithNullability(CaseOp op, Node n, out bool thenClauseIsNull)
	{
		thenClauseIsNull = false;
		if (!TypeSemantics.IsRowType(op.Type))
		{
			return false;
		}
		if (n.Children.Count != 3)
		{
			return false;
		}
		if (!n.Child1.Op.Type.EdmEquals(op.Type) || !n.Child2.Op.Type.EdmEquals(op.Type))
		{
			return false;
		}
		if (n.Child1.Op.OpType == OpType.Null)
		{
			thenClauseIsNull = true;
			return true;
		}
		if (n.Child2.Op.OpType == OpType.Null)
		{
			return true;
		}
		return false;
	}

	internal static bool IsCollectionAggregateFunction(FunctionOp op, Node n)
	{
		if (n.Children.Count >= 1 && TypeSemantics.IsCollectionType(n.Child0.Op.Type))
		{
			return TypeSemantics.IsAggregateFunction(op.Function);
		}
		return false;
	}

	internal static bool IsConstantBaseOp(OpType opType)
	{
		if (opType != 0 && opType != OpType.InternalConstant && opType != OpType.Null)
		{
			return opType == OpType.NullSentinel;
		}
		return true;
	}

	internal static Node CombinePredicates(Node predicate1, Node predicate2, Command command)
	{
		IEnumerable<Node> enumerable = BreakIntoAndParts(predicate1);
		IEnumerable<Node> enumerable2 = BreakIntoAndParts(predicate2);
		Node node = predicate1;
		foreach (Node item in enumerable2)
		{
			bool flag = false;
			foreach (Node item2 in enumerable)
			{
				if (item2.IsEquivalent(item))
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				node = command.CreateNode(command.CreateConditionalOp(OpType.And), node, item);
			}
		}
		return node;
	}

	private static IEnumerable<Node> BreakIntoAndParts(Node predicate)
	{
		return Helpers.GetLeafNodes(predicate, (Node node) => node.Op.OpType != OpType.And, (Node node) => new Node[2] { node.Child0, node.Child1 });
	}
}
