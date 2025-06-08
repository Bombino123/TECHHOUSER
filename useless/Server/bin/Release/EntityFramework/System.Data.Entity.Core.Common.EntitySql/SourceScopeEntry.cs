using System.Collections.Generic;
using System.Data.Entity.Core.Common.CommandTrees;
using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;

namespace System.Data.Entity.Core.Common.EntitySql;

internal sealed class SourceScopeEntry : ScopeEntry, IGroupExpressionExtendedInfo, IGetAlternativeName
{
	private readonly string[] _alternativeName;

	private List<string> _propRefs;

	private DbExpression _varBasedExpression;

	private DbExpression _groupVarBasedExpression;

	private DbExpression _groupAggBasedExpression;

	DbExpression IGroupExpressionExtendedInfo.GroupVarBasedExpression => _groupVarBasedExpression;

	DbExpression IGroupExpressionExtendedInfo.GroupAggBasedExpression => _groupAggBasedExpression;

	internal bool IsJoinClauseLeftExpr { get; set; }

	string[] IGetAlternativeName.AlternativeName => _alternativeName;

	internal SourceScopeEntry(DbVariableReferenceExpression varRef)
		: this(varRef, null)
	{
	}

	internal SourceScopeEntry(DbVariableReferenceExpression varRef, string[] alternativeName)
		: base(ScopeEntryKind.SourceVar)
	{
		_varBasedExpression = varRef;
		_alternativeName = alternativeName;
	}

	internal override DbExpression GetExpression(string refName, ErrorContext errCtx)
	{
		return _varBasedExpression;
	}

	internal SourceScopeEntry AddParentVar(DbVariableReferenceExpression parentVarRef)
	{
		if (_propRefs == null)
		{
			_propRefs = new List<string>(2);
			_propRefs.Add(((DbVariableReferenceExpression)_varBasedExpression).VariableName);
		}
		_varBasedExpression = parentVarRef;
		for (int num = _propRefs.Count - 1; num >= 0; num--)
		{
			_varBasedExpression = _varBasedExpression.Property(_propRefs[num]);
		}
		_propRefs.Add(parentVarRef.VariableName);
		return this;
	}

	internal void ReplaceParentVar(DbVariableReferenceExpression parentVarRef)
	{
		if (_propRefs == null)
		{
			_varBasedExpression = parentVarRef;
			return;
		}
		_propRefs.RemoveAt(_propRefs.Count - 1);
		AddParentVar(parentVarRef);
	}

	internal void AdjustToGroupVar(DbVariableReferenceExpression parentVarRef, DbVariableReferenceExpression parentGroupVarRef, DbVariableReferenceExpression groupAggRef)
	{
		ReplaceParentVar(parentVarRef);
		_groupVarBasedExpression = parentGroupVarRef;
		_groupAggBasedExpression = groupAggRef;
		if (_propRefs != null)
		{
			for (int num = _propRefs.Count - 2; num >= 0; num--)
			{
				_groupVarBasedExpression = _groupVarBasedExpression.Property(_propRefs[num]);
				_groupAggBasedExpression = _groupAggBasedExpression.Property(_propRefs[num]);
			}
		}
	}

	internal void RollbackAdjustmentToGroupVar(DbVariableReferenceExpression pregroupParentVarRef)
	{
		_groupVarBasedExpression = null;
		_groupAggBasedExpression = null;
		ReplaceParentVar(pregroupParentVarRef);
	}
}
