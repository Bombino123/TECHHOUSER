using System.Collections.Generic;
using System.Data.Entity.Core.Common.CommandTrees;
using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
using System.Data.Entity.Core.Common.EntitySql.AST;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Resources;
using System.Linq;

namespace System.Data.Entity.Core.Common.EntitySql;

internal abstract class GroupAggregateInfo
{
	private ScopeRegion _innermostReferencedScopeRegion;

	private List<GroupAggregateInfo> _containedAggregates;

	internal readonly GroupAggregateKind AggregateKind;

	internal readonly GroupAggregateExpr AstNode;

	internal readonly ErrorContext ErrCtx;

	internal readonly ScopeRegion DefiningScopeRegion;

	private ScopeRegion _evaluatingScopeRegion;

	private GroupAggregateInfo _containingAggregate;

	internal string AggregateName;

	internal DbNullExpression AggregateStubExpression;

	internal ScopeRegion InnermostReferencedScopeRegion
	{
		get
		{
			return _innermostReferencedScopeRegion;
		}
		set
		{
			_innermostReferencedScopeRegion = value;
		}
	}

	internal ScopeRegion EvaluatingScopeRegion => _evaluatingScopeRegion;

	internal GroupAggregateInfo ContainingAggregate => _containingAggregate;

	protected GroupAggregateInfo(GroupAggregateKind aggregateKind, GroupAggregateExpr astNode, ErrorContext errCtx, GroupAggregateInfo containingAggregate, ScopeRegion definingScopeRegion)
	{
		AggregateKind = aggregateKind;
		AstNode = astNode;
		ErrCtx = errCtx;
		DefiningScopeRegion = definingScopeRegion;
		SetContainingAggregate(containingAggregate);
	}

	protected void AttachToAstNode(string aggregateName, TypeUsage resultType)
	{
		AggregateName = aggregateName;
		AggregateStubExpression = resultType.Null();
		AstNode.AggregateInfo = this;
	}

	internal void DetachFromAstNode()
	{
		AstNode.AggregateInfo = null;
	}

	internal void UpdateScopeIndex(int referencedScopeIndex, SemanticResolver sr)
	{
		ScopeRegion definingScopeRegion = sr.GetDefiningScopeRegion(referencedScopeIndex);
		if (_innermostReferencedScopeRegion == null || _innermostReferencedScopeRegion.ScopeRegionIndex < definingScopeRegion.ScopeRegionIndex)
		{
			_innermostReferencedScopeRegion = definingScopeRegion;
		}
	}

	internal void ValidateAndComputeEvaluatingScopeRegion(SemanticResolver sr)
	{
		_evaluatingScopeRegion = _innermostReferencedScopeRegion ?? DefiningScopeRegion;
		if (!_evaluatingScopeRegion.IsAggregating)
		{
			int scopeRegionIndex = _evaluatingScopeRegion.ScopeRegionIndex;
			_evaluatingScopeRegion = null;
			foreach (ScopeRegion item in sr.ScopeRegions.Skip(scopeRegionIndex))
			{
				if (item.IsAggregating)
				{
					_evaluatingScopeRegion = item;
					break;
				}
			}
			if (_evaluatingScopeRegion == null)
			{
				throw new EntitySqlException(Strings.GroupVarNotFoundInScope);
			}
		}
		ValidateContainedAggregates(_evaluatingScopeRegion.ScopeRegionIndex, DefiningScopeRegion.ScopeRegionIndex);
	}

	private void ValidateContainedAggregates(int outerBoundaryScopeRegionIndex, int innerBoundaryScopeRegionIndex)
	{
		if (_containedAggregates == null)
		{
			return;
		}
		foreach (GroupAggregateInfo containedAggregate in _containedAggregates)
		{
			if (containedAggregate.EvaluatingScopeRegion.ScopeRegionIndex >= outerBoundaryScopeRegionIndex && containedAggregate.EvaluatingScopeRegion.ScopeRegionIndex <= innerBoundaryScopeRegionIndex)
			{
				int lineNumber;
				int columnNumber;
				string p = EntitySqlException.FormatErrorContext(ErrCtx.CommandText, ErrCtx.InputPosition, ErrCtx.ErrorContextInfo, ErrCtx.UseContextInfoAsResourceIdentifier, out lineNumber, out columnNumber);
				throw new EntitySqlException(Strings.NestedAggregateCannotBeUsedInAggregate(EntitySqlException.FormatErrorContext(containedAggregate.ErrCtx.CommandText, containedAggregate.ErrCtx.InputPosition, containedAggregate.ErrCtx.ErrorContextInfo, containedAggregate.ErrCtx.UseContextInfoAsResourceIdentifier, out lineNumber, out columnNumber), p));
			}
			containedAggregate.ValidateContainedAggregates(outerBoundaryScopeRegionIndex, innerBoundaryScopeRegionIndex);
		}
	}

	internal void SetContainingAggregate(GroupAggregateInfo containingAggregate)
	{
		if (_containingAggregate != null)
		{
			_containingAggregate.RemoveContainedAggregate(this);
		}
		_containingAggregate = containingAggregate;
		if (_containingAggregate != null)
		{
			_containingAggregate.AddContainedAggregate(this);
		}
	}

	private void AddContainedAggregate(GroupAggregateInfo containedAggregate)
	{
		if (_containedAggregates == null)
		{
			_containedAggregates = new List<GroupAggregateInfo>();
		}
		_containedAggregates.Add(containedAggregate);
	}

	private void RemoveContainedAggregate(GroupAggregateInfo containedAggregate)
	{
		_containedAggregates.Remove(containedAggregate);
	}
}
