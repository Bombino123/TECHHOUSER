namespace System.Data.Entity.Core.Query.InternalTrees;

internal abstract class RuleProcessingContext
{
	private readonly Command m_command;

	internal Command Command => m_command;

	internal virtual void PreProcess(Node node)
	{
	}

	internal virtual void PreProcessSubTree(Node node)
	{
	}

	internal virtual void PostProcess(Node node, Rule rule)
	{
	}

	internal virtual void PostProcessSubTree(Node node)
	{
	}

	internal virtual int GetHashCode(Node node)
	{
		return node.GetHashCode();
	}

	internal RuleProcessingContext(Command command)
	{
		m_command = command;
	}
}
