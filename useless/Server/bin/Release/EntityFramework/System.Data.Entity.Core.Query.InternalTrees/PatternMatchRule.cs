namespace System.Data.Entity.Core.Query.InternalTrees;

internal sealed class PatternMatchRule : Rule
{
	private readonly Node m_pattern;

	internal PatternMatchRule(Node pattern, ProcessNodeDelegate processDelegate)
		: base(pattern.Op.OpType, processDelegate)
	{
		m_pattern = pattern;
	}

	private bool Match(Node pattern, Node original)
	{
		if (pattern.Op.OpType == OpType.Leaf)
		{
			return true;
		}
		if (pattern.Op.OpType != original.Op.OpType)
		{
			return false;
		}
		if (pattern.Children.Count != original.Children.Count)
		{
			return false;
		}
		for (int i = 0; i < pattern.Children.Count; i++)
		{
			if (!Match(pattern.Children[i], original.Children[i]))
			{
				return false;
			}
		}
		return true;
	}

	internal override bool Match(Node node)
	{
		return Match(m_pattern, node);
	}
}
