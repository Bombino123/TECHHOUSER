namespace System.Data.Entity.Core.Query.InternalTrees;

internal abstract class BasicOpVisitorOfNode : BasicOpVisitorOfT<Node>
{
	protected override void VisitChildren(Node n)
	{
		for (int i = 0; i < n.Children.Count; i++)
		{
			n.Children[i] = VisitNode(n.Children[i]);
		}
	}

	protected override void VisitChildrenReverse(Node n)
	{
		for (int num = n.Children.Count - 1; num >= 0; num--)
		{
			n.Children[num] = VisitNode(n.Children[num]);
		}
	}

	protected override Node VisitDefault(Node n)
	{
		VisitChildren(n);
		return n;
	}

	protected override Node VisitAncillaryOpDefault(AncillaryOp op, Node n)
	{
		return VisitDefault(n);
	}

	protected override Node VisitPhysicalOpDefault(PhysicalOp op, Node n)
	{
		return VisitDefault(n);
	}

	protected override Node VisitRelOpDefault(RelOp op, Node n)
	{
		return VisitDefault(n);
	}

	protected override Node VisitScalarOpDefault(ScalarOp op, Node n)
	{
		return VisitDefault(n);
	}
}
