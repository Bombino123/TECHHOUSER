namespace System.Data.Entity.Core.Query.InternalTrees;

internal abstract class Rule
{
	internal delegate bool ProcessNodeDelegate(RuleProcessingContext context, Node subTree, out Node newSubTree);

	private readonly ProcessNodeDelegate m_nodeDelegate;

	private readonly OpType m_opType;

	internal OpType RuleOpType => m_opType;

	protected Rule(OpType opType, ProcessNodeDelegate nodeProcessDelegate)
	{
		m_opType = opType;
		m_nodeDelegate = nodeProcessDelegate;
	}

	internal abstract bool Match(Node node);

	internal bool Apply(RuleProcessingContext ruleProcessingContext, Node node, out Node newNode)
	{
		return m_nodeDelegate(ruleProcessingContext, node, out newNode);
	}
}
