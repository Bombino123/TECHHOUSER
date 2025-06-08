using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Utilities;

namespace System.Data.Entity.Core.Common.CommandTrees;

public sealed class DbLimitExpression : DbExpression
{
	private readonly DbExpression _argument;

	private readonly DbExpression _limit;

	private readonly bool _withTies;

	public DbExpression Argument => _argument;

	public DbExpression Limit => _limit;

	public bool WithTies => _withTies;

	internal DbLimitExpression(TypeUsage resultType, DbExpression argument, DbExpression limit, bool withTies)
		: base(DbExpressionKind.Limit, resultType)
	{
		_argument = argument;
		_limit = limit;
		_withTies = withTies;
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
