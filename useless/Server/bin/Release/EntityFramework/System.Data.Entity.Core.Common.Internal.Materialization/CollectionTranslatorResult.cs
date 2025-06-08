using System.Linq.Expressions;

namespace System.Data.Entity.Core.Common.Internal.Materialization;

internal class CollectionTranslatorResult : TranslatorResult
{
	internal readonly Expression ExpressionToGetCoordinator;

	internal CollectionTranslatorResult(Expression returnedExpression, Type requestedType, Expression expressionToGetCoordinator)
		: base(returnedExpression, requestedType)
	{
		ExpressionToGetCoordinator = expressionToGetCoordinator;
	}
}
