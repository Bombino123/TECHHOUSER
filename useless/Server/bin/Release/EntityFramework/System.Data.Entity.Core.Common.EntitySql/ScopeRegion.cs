using System.Collections.Generic;
using System.Data.Entity.Core.Common.CommandTrees;
using System.Data.Entity.Utilities;

namespace System.Data.Entity.Core.Common.EntitySql;

internal sealed class ScopeRegion
{
	private readonly ScopeManager _scopeManager;

	private readonly int _firstScopeIndex;

	private readonly int _scopeRegionIndex;

	private DbExpressionBinding _groupAggregateBinding;

	private readonly List<GroupAggregateInfo> _groupAggregateInfos = new List<GroupAggregateInfo>();

	private readonly HashSet<string> _groupAggregateNames = new HashSet<string>();

	internal int FirstScopeIndex => _firstScopeIndex;

	internal int ScopeRegionIndex => _scopeRegionIndex;

	internal bool IsAggregating => _groupAggregateBinding != null;

	internal DbExpressionBinding GroupAggregateBinding => _groupAggregateBinding;

	internal List<GroupAggregateInfo> GroupAggregateInfos => _groupAggregateInfos;

	internal bool WasResolutionCorrelated { get; set; }

	internal ScopeRegion(ScopeManager scopeManager, int firstScopeIndex, int scopeRegionIndex)
	{
		_scopeManager = scopeManager;
		_firstScopeIndex = firstScopeIndex;
		_scopeRegionIndex = scopeRegionIndex;
	}

	internal bool ContainsScope(int scopeIndex)
	{
		return scopeIndex >= _firstScopeIndex;
	}

	internal void EnterGroupOperation(DbExpressionBinding groupAggregateBinding)
	{
		_groupAggregateBinding = groupAggregateBinding;
	}

	internal void RollbackGroupOperation()
	{
		_groupAggregateBinding = null;
	}

	internal void RegisterGroupAggregateName(string groupAggregateName)
	{
		_groupAggregateNames.Add(groupAggregateName);
	}

	internal bool ContainsGroupAggregate(string groupAggregateName)
	{
		return _groupAggregateNames.Contains(groupAggregateName);
	}

	internal void ApplyToScopeEntries(Action<ScopeEntry> action)
	{
		for (int i = FirstScopeIndex; i <= _scopeManager.CurrentScopeIndex; i++)
		{
			foreach (KeyValuePair<string, ScopeEntry> item in _scopeManager.GetScopeByIndex(i))
			{
				action(item.Value);
			}
		}
	}

	internal void ApplyToScopeEntries(Func<ScopeEntry, ScopeEntry> action)
	{
		for (int i = FirstScopeIndex; i <= _scopeManager.CurrentScopeIndex; i++)
		{
			Scope scope = _scopeManager.GetScopeByIndex(i);
			List<KeyValuePair<string, ScopeEntry>> list = null;
			foreach (KeyValuePair<string, ScopeEntry> item in scope)
			{
				ScopeEntry scopeEntry = action(item.Value);
				if (item.Value != scopeEntry)
				{
					if (list == null)
					{
						list = new List<KeyValuePair<string, ScopeEntry>>();
					}
					list.Add(new KeyValuePair<string, ScopeEntry>(item.Key, scopeEntry));
				}
			}
			list?.Each(delegate(KeyValuePair<string, ScopeEntry> updatedScopeEntry)
			{
				scope.Replace(updatedScopeEntry.Key, updatedScopeEntry.Value);
			});
		}
	}

	internal void RollbackAllScopes()
	{
		_scopeManager.RollbackToScope(FirstScopeIndex - 1);
	}
}
