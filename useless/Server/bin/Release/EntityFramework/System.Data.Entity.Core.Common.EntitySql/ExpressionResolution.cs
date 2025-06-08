namespace System.Data.Entity.Core.Common.EntitySql;

internal abstract class ExpressionResolution
{
	internal readonly ExpressionResolutionClass ExpressionClass;

	internal abstract string ExpressionClassName { get; }

	protected ExpressionResolution(ExpressionResolutionClass @class)
	{
		ExpressionClass = @class;
	}
}
