using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Utilities;

namespace System.Data.Entity.Core.Common.CommandTrees;

public sealed class DbJoinExpression : DbExpression
{
	private readonly DbExpressionBinding _left;

	private readonly DbExpressionBinding _right;

	private readonly DbExpression _condition;

	public DbExpressionBinding Left => _left;

	public DbExpressionBinding Right => _right;

	public DbExpression JoinCondition => _condition;

	internal DbJoinExpression(DbExpressionKind joinKind, TypeUsage collectionOfRowResultType, DbExpressionBinding left, DbExpressionBinding right, DbExpression condition)
		: base(joinKind, collectionOfRowResultType)
	{
		_left = left;
		_right = right;
		_condition = condition;
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
