using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace System.Data.Entity.Core.Query.InternalTrees;

internal class RuleProcessor
{
	private readonly Dictionary<SubTreeId, SubTreeId> m_processedNodeMap;

	internal RuleProcessor()
	{
		m_processedNodeMap = new Dictionary<SubTreeId, SubTreeId>();
	}

	private static bool ApplyRulesToNode(RuleProcessingContext context, ReadOnlyCollection<ReadOnlyCollection<Rule>> rules, Node currentNode, out Node newNode)
	{
		newNode = currentNode;
		context.PreProcess(currentNode);
		foreach (Rule item in rules[(int)currentNode.Op.OpType])
		{
			if (item.Match(currentNode) && item.Apply(context, currentNode, out newNode))
			{
				context.PostProcess(newNode, item);
				return true;
			}
		}
		context.PostProcess(currentNode, null);
		return false;
	}

	private Node ApplyRulesToSubtree(RuleProcessingContext context, ReadOnlyCollection<ReadOnlyCollection<Rule>> rules, Node subTreeRoot, Node parent, int childIndexInParent)
	{
		int num = 0;
		Dictionary<SubTreeId, SubTreeId> dictionary = new Dictionary<SubTreeId, SubTreeId>();
		while (true)
		{
			num++;
			context.PreProcessSubTree(subTreeRoot);
			SubTreeId subTreeId = new SubTreeId(context, subTreeRoot, parent, childIndexInParent);
			if (m_processedNodeMap.ContainsKey(subTreeId))
			{
				break;
			}
			if (dictionary.ContainsKey(subTreeId))
			{
				m_processedNodeMap[subTreeId] = subTreeId;
				break;
			}
			dictionary[subTreeId] = subTreeId;
			for (int i = 0; i < subTreeRoot.Children.Count; i++)
			{
				Node node = subTreeRoot.Children[i];
				if (ShouldApplyRules(node, subTreeRoot))
				{
					subTreeRoot.Children[i] = ApplyRulesToSubtree(context, rules, node, subTreeRoot, i);
				}
			}
			if (!ApplyRulesToNode(context, rules, subTreeRoot, out var newNode))
			{
				m_processedNodeMap[subTreeId] = subTreeId;
				break;
			}
			context.PostProcessSubTree(subTreeRoot);
			subTreeRoot = newNode;
		}
		context.PostProcessSubTree(subTreeRoot);
		return subTreeRoot;
	}

	private static bool ShouldApplyRules(Node node, Node parent)
	{
		if (parent.Op.OpType == OpType.In)
		{
			return node.Op.OpType != OpType.Constant;
		}
		return true;
	}

	internal Node ApplyRulesToSubtree(RuleProcessingContext context, ReadOnlyCollection<ReadOnlyCollection<Rule>> rules, Node subTreeRoot)
	{
		return ApplyRulesToSubtree(context, rules, subTreeRoot, null, 0);
	}
}
