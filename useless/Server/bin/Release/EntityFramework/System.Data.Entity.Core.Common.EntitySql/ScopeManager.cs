using System.Collections.Generic;
using System.Data.Entity.Resources;

namespace System.Data.Entity.Core.Common.EntitySql;

internal sealed class ScopeManager
{
	private readonly IEqualityComparer<string> _keyComparer;

	private readonly List<Scope> _scopes = new List<Scope>();

	internal int CurrentScopeIndex => _scopes.Count - 1;

	internal Scope CurrentScope => _scopes[CurrentScopeIndex];

	internal ScopeManager(IEqualityComparer<string> keyComparer)
	{
		_keyComparer = keyComparer;
	}

	internal void EnterScope()
	{
		_scopes.Add(new Scope(_keyComparer));
	}

	internal void LeaveScope()
	{
		_scopes.RemoveAt(CurrentScopeIndex);
	}

	internal Scope GetScopeByIndex(int scopeIndex)
	{
		if (0 > scopeIndex || scopeIndex > CurrentScopeIndex)
		{
			throw new EntitySqlException(Strings.InvalidScopeIndex);
		}
		return _scopes[scopeIndex];
	}

	internal void RollbackToScope(int scopeIndex)
	{
		if (scopeIndex > CurrentScopeIndex || scopeIndex < 0 || CurrentScopeIndex < 0)
		{
			throw new EntitySqlException(Strings.InvalidSavePoint);
		}
		if (CurrentScopeIndex - scopeIndex > 0)
		{
			_scopes.RemoveRange(scopeIndex + 1, CurrentScopeIndex - scopeIndex);
		}
	}

	internal bool IsInCurrentScope(string key)
	{
		return CurrentScope.Contains(key);
	}
}
