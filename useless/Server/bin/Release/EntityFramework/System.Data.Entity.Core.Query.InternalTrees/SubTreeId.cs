namespace System.Data.Entity.Core.Query.InternalTrees;

internal class SubTreeId
{
	public Node m_subTreeRoot;

	private readonly int m_hashCode;

	private readonly Node m_parent;

	private readonly int m_childIndex;

	internal SubTreeId(RuleProcessingContext context, Node node, Node parent, int childIndex)
	{
		m_subTreeRoot = node;
		m_parent = parent;
		m_childIndex = childIndex;
		m_hashCode = context.GetHashCode(node);
	}

	public override int GetHashCode()
	{
		return m_hashCode;
	}

	public override bool Equals(object obj)
	{
		if (obj is SubTreeId subTreeId && m_hashCode == subTreeId.m_hashCode)
		{
			if (subTreeId.m_subTreeRoot != m_subTreeRoot)
			{
				if (subTreeId.m_parent == m_parent)
				{
					return subTreeId.m_childIndex == m_childIndex;
				}
				return false;
			}
			return true;
		}
		return false;
	}
}
