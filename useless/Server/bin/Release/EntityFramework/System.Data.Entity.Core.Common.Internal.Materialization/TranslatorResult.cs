using System.Data.Entity.Core.Objects.Internal;
using System.Linq.Expressions;

namespace System.Data.Entity.Core.Common.Internal.Materialization;

internal class TranslatorResult
{
	private readonly Expression ReturnedExpression;

	private readonly Type RequestedType;

	internal Expression Expression => CodeGenEmitter.Emit_EnsureType(ReturnedExpression, RequestedType);

	internal Expression UnconvertedExpression => ReturnedExpression;

	internal Expression UnwrappedExpression
	{
		get
		{
			if (!typeof(IEntityWrapper).IsAssignableFrom(ReturnedExpression.Type))
			{
				return ReturnedExpression;
			}
			return CodeGenEmitter.Emit_UnwrapAndEnsureType(ReturnedExpression, RequestedType);
		}
	}

	internal TranslatorResult(Expression returnedExpression, Type requestedType)
	{
		RequestedType = requestedType;
		ReturnedExpression = returnedExpression;
	}
}
