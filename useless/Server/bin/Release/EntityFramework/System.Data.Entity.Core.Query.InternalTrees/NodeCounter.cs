namespace System.Data.Entity.Core.Query.InternalTrees;

internal class NodeCounter : BasicOpVisitorOfT<int>
{
	internal static int Count(Node subTree)
	{
		return new NodeCounter().VisitNode(subTree);
	}

	protected override int VisitDefault(Node n)
	{
		int num = 1;
		foreach (Node child in n.Children)
		{
			num += VisitNode(child);
		}
		return num;
	}
}
