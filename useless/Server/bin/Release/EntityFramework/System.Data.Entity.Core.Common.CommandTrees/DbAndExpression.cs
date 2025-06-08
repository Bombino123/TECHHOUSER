using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Utilities;

namespace System.Data.Entity.Core.Common.CommandTrees;

public sealed class DbAndExpression : DbBinaryExpression
{
	internal DbAndExpression(TypeUsage booleanResultType, DbExpression left, DbExpression right)
		: base(DbExpressionKind.And, booleanResultType, left, right)
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
