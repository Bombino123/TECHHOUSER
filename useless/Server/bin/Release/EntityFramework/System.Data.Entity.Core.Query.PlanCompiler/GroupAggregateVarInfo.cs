using System.Collections.Generic;
using System.Data.Entity.Core.Query.InternalTrees;

namespace System.Data.Entity.Core.Query.PlanCompiler;

internal class GroupAggregateVarInfo
{
	private readonly Node _definingGroupByNode;

	private HashSet<KeyValuePair<Node, List<Node>>> _candidateAggregateNodes;

	private readonly Var _groupAggregateVar;

	internal HashSet<KeyValuePair<Node, List<Node>>> CandidateAggregateNodes
	{
		get
		{
			if (_candidateAggregateNodes == null)
			{
				_candidateAggregateNodes = new HashSet<KeyValuePair<Node, List<Node>>>();
			}
			return _candidateAggregateNodes;
		}
	}

	internal bool HasCandidateAggregateNodes
	{
		get
		{
			if (_candidateAggregateNodes != null)
			{
				return _candidateAggregateNodes.Count != 0;
			}
			return false;
		}
	}

	internal Node DefiningGroupNode => _definingGroupByNode;

	internal Var GroupAggregateVar => _groupAggregateVar;

	internal GroupAggregateVarInfo(Node defingingGroupNode, Var groupAggregateVar)
	{
		_definingGroupByNode = defingingGroupNode;
		_groupAggregateVar = groupAggregateVar;
	}
}
