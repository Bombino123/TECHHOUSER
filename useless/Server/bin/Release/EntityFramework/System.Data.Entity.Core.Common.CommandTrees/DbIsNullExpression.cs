using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Utilities;

namespace System.Data.Entity.Core.Common.CommandTrees;

public class DbIsNullExpression : DbUnaryExpression
{
	internal DbIsNullExpression()
	{
	}

	internal DbIsNullExpression(TypeUsage booleanResultType, DbExpression arg)
		: base(DbExpressionKind.IsNull, booleanResultType, arg)
	{
	}

	public override void Accept(DbExpressionVisitor visitor)
	{
		Check.NotNull(visitor, "visitor");
		visitor.Visit(this);
	}

	public override TResultType Accept<TResultType>(DbExpressionVisitor<TResultType> visitor)
	{
		Check.NotNull(visitor, "visitor");
		return visitor.Visit(this);
	}
}
