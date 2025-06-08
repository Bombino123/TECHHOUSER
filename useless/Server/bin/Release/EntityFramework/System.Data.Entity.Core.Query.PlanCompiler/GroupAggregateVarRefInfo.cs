using System.Data.Entity.Core.Query.InternalTrees;

namespace System.Data.Entity.Core.Query.PlanCompiler;

internal class GroupAggregateVarRefInfo
{
	private readonly Node _computation;

	private readonly GroupAggregateVarInfo _groupAggregateVarInfo;

	private readonly bool _isUnnested;

	internal Node Computation => _computation;

	internal GroupAggregateVarInfo GroupAggregateVarInfo => _groupAggregateVarInfo;

	internal bool IsUnnested => _isUnnested;

	internal GroupAggregateVarRefInfo(GroupAggregateVarInfo groupAggregateVarInfo, Node computation, bool isUnnested)
	{
		_groupAggregateVarInfo = groupAggregateVarInfo;
		_computation = computation;
		_isUnnested = isUnnested;
	}
}
