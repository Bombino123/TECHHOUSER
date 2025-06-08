using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Utilities;

namespace System.Data.Entity.Core.Common.CommandTrees;

public class DbCastExpression : DbUnaryExpression
{
	internal DbCastExpression()
	{
	}

	internal DbCastExpression(TypeUsage type, DbExpression argument)
		: base(DbExpressionKind.Cast, type, argument)
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
