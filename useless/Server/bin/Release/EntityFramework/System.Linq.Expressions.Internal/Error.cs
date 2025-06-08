using System.Data.Entity.Resources;

namespace System.Linq.Expressions.Internal;

internal static class Error
{
	internal static Exception UnhandledExpressionType(ExpressionType expressionType)
	{
		return new NotSupportedException(Strings.ELinq_UnhandledExpressionType(expressionType));
	}

	internal static Exception UnhandledBindingType(MemberBindingType memberBindingType)
	{
		return new NotSupportedException(Strings.ELinq_UnhandledBindingType(memberBindingType));
	}
}
