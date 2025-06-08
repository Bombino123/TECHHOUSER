using System.Collections.Generic;
using System.Data.Entity.Core.Metadata.Edm;
using System.Linq;

namespace System.Data.Entity.Core.Common.CommandTrees.Internal;

internal static class Patterns
{
	internal static Func<DbExpression, bool> AnyExpression => (DbExpression e) => true;

	internal static Func<IEnumerable<DbExpression>, bool> AnyExpressions => (IEnumerable<DbExpression> elems) => true;

	internal static Func<DbExpression, bool> MatchComplexType => (DbExpression e) => TypeSemantics.IsComplexType(e.ResultType);

	internal static Func<DbExpression, bool> MatchEntityType => (DbExpression e) => TypeSemantics.IsEntityType(e.ResultType);

	internal static Func<DbExpression, bool> MatchRowType => (DbExpression e) => TypeSemantics.IsRowType(e.ResultType);

	internal static Func<DbExpression, bool> And(Func<DbExpression, bool> pattern1, Func<DbExpression, bool> pattern2)
	{
		return (DbExpression e) => pattern1(e) && pattern2(e);
	}

	internal static Func<DbExpression, bool> And(Func<DbExpression, bool> pattern1, Func<DbExpression, bool> pattern2, Func<DbExpression, bool> pattern3)
	{
		return (DbExpression e) => pattern1(e) && pattern2(e) && pattern3(e);
	}

	internal static Func<DbExpression, bool> Or(Func<DbExpression, bool> pattern1, Func<DbExpression, bool> pattern2)
	{
		return (DbExpression e) => pattern1(e) || pattern2(e);
	}

	internal static Func<DbExpression, bool> Or(Func<DbExpression, bool> pattern1, Func<DbExpression, bool> pattern2, Func<DbExpression, bool> pattern3)
	{
		return (DbExpression e) => pattern1(e) || pattern2(e) || pattern3(e);
	}

	internal static Func<DbExpression, bool> MatchKind(DbExpressionKind kindToMatch)
	{
		return (DbExpression e) => e.ExpressionKind == kindToMatch;
	}

	internal static Func<IEnumerable<DbExpression>, bool> MatchForAll(Func<DbExpression, bool> elementPattern)
	{
		return (IEnumerable<DbExpression> elems) => elems.FirstOrDefault((DbExpression e) => !elementPattern(e)) == null;
	}

	internal static Func<DbExpression, bool> MatchBinary()
	{
		return (DbExpression e) => e is DbBinaryExpression;
	}

	internal static Func<DbExpression, bool> MatchFilter(Func<DbExpression, bool> inputPattern, Func<DbExpression, bool> predicatePattern)
	{
		return delegate(DbExpression e)
		{
			if (e.ExpressionKind != DbExpressionKind.Filter)
			{
				return false;
			}
			DbFilterExpression dbFilterExpression = (DbFilterExpression)e;
			return inputPattern(dbFilterExpression.Input.Expression) && predicatePattern(dbFilterExpression.Predicate);
		};
	}

	internal static Func<DbExpression, bool> MatchProject(Func<DbExpression, bool> inputPattern, Func<DbExpression, bool> projectionPattern)
	{
		return delegate(DbExpression e)
		{
			if (e.ExpressionKind != DbExpressionKind.Project)
			{
				return false;
			}
			DbProjectExpression dbProjectExpression = (DbProjectExpression)e;
			return inputPattern(dbProjectExpression.Input.Expression) && projectionPattern(dbProjectExpression.Projection);
		};
	}

	internal static Func<DbExpression, bool> MatchCase(Func<IEnumerable<DbExpression>, bool> whenPattern, Func<IEnumerable<DbExpression>, bool> thenPattern, Func<DbExpression, bool> elsePattern)
	{
		return delegate(DbExpression e)
		{
			if (e.ExpressionKind != DbExpressionKind.Case)
			{
				return false;
			}
			DbCaseExpression dbCaseExpression = (DbCaseExpression)e;
			return whenPattern(dbCaseExpression.When) && thenPattern(dbCaseExpression.Then) && elsePattern(dbCaseExpression.Else);
		};
	}

	internal static Func<DbExpression, bool> MatchNewInstance()
	{
		return (DbExpression e) => e.ExpressionKind == DbExpressionKind.NewInstance;
	}

	internal static Func<DbExpression, bool> MatchNewInstance(Func<IEnumerable<DbExpression>, bool> argumentsPattern)
	{
		return delegate(DbExpression e)
		{
			if (e.ExpressionKind != DbExpressionKind.NewInstance)
			{
				return false;
			}
			DbNewInstanceExpression dbNewInstanceExpression = (DbNewInstanceExpression)e;
			return argumentsPattern(dbNewInstanceExpression.Arguments);
		};
	}
}
