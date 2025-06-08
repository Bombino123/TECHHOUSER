using System.Data.Entity.Core.Common.CommandTrees;
using System.Data.Entity.Resources;

namespace System.Data.Entity.Core.Common.EntitySql;

internal sealed class ValueExpression : ExpressionResolution
{
	internal readonly DbExpression Value;

	internal override string ExpressionClassName => ValueClassName;

	internal static string ValueClassName => Strings.LocalizedValueExpression;

	internal ValueExpression(DbExpression value)
		: base(ExpressionResolutionClass.Value)
	{
		Value = value;
	}
}
