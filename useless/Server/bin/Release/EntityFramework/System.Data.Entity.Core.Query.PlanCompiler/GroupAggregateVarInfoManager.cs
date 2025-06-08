using System.Collections.Generic;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Core.Query.InternalTrees;

namespace System.Data.Entity.Core.Query.PlanCompiler;

internal class GroupAggregateVarInfoManager
{
	private readonly Dictionary<Var, GroupAggregateVarRefInfo> _groupAggregateVarRelatedVarToInfo = new Dictionary<Var, GroupAggregateVarRefInfo>();

	private Dictionary<Var, Dictionary<EdmMember, GroupAggregateVarRefInfo>> _groupAggregateVarRelatedVarPropertyToInfo;

	private readonly HashSet<GroupAggregateVarInfo> _groupAggregateVarInfos = new HashSet<GroupAggregateVarInfo>();

	internal IEnumerable<GroupAggregateVarInfo> GroupAggregateVarInfos => _groupAggregateVarInfos;

	internal void Add(Var var, GroupAggregateVarInfo groupAggregateVarInfo, Node computationTemplate, bool isUnnested)
	{
		_groupAggregateVarRelatedVarToInfo.Add(var, new GroupAggregateVarRefInfo(groupAggregateVarInfo, computationTemplate, isUnnested));
		_groupAggregateVarInfos.Add(groupAggregateVarInfo);
	}

	internal void Add(Var var, GroupAggregateVarInfo groupAggregateVarInfo, Node computationTemplate, bool isUnnested, EdmMember property)
	{
		if (property == null)
		{
			Add(var, groupAggregateVarInfo, computationTemplate, isUnnested);
			return;
		}
		if (_groupAggregateVarRelatedVarPropertyToInfo == null)
		{
			_groupAggregateVarRelatedVarPropertyToInfo = new Dictionary<Var, Dictionary<EdmMember, GroupAggregateVarRefInfo>>();
		}
		if (!_groupAggregateVarRelatedVarPropertyToInfo.TryGetValue(var, out var value))
		{
			value = new Dictionary<EdmMember, GroupAggregateVarRefInfo>();
			_groupAggregateVarRelatedVarPropertyToInfo.Add(var, value);
		}
		value.Add(property, new GroupAggregateVarRefInfo(groupAggregateVarInfo, computationTemplate, isUnnested));
		_groupAggregateVarInfos.Add(groupAggregateVarInfo);
	}

	internal bool TryGetReferencedGroupAggregateVarInfo(Var var, out GroupAggregateVarRefInfo groupAggregateVarRefInfo)
	{
		return _groupAggregateVarRelatedVarToInfo.TryGetValue(var, out groupAggregateVarRefInfo);
	}

	internal bool TryGetReferencedGroupAggregateVarInfo(Var var, EdmMember property, out GroupAggregateVarRefInfo groupAggregateVarRefInfo)
	{
		if (property == null)
		{
			return TryGetReferencedGroupAggregateVarInfo(var, out groupAggregateVarRefInfo);
		}
		if (_groupAggregateVarRelatedVarPropertyToInfo == null || !_groupAggregateVarRelatedVarPropertyToInfo.TryGetValue(var, out var value))
		{
			groupAggregateVarRefInfo = null;
			return false;
		}
		return value.TryGetValue(property, out groupAggregateVarRefInfo);
	}
}
