using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Utilities;

namespace System.Data.Entity.Core.Common.CommandTrees;

public sealed class DbLikeExpression : DbExpression
{
	private readonly DbExpression _argument;

	private readonly DbExpression _pattern;

	private readonly DbExpression _escape;

	public DbExpression Argument => _argument;

	public DbExpression Pattern => _pattern;

	public DbExpression Escape => _escape;

	internal DbLikeExpression(TypeUsage booleanResultType, DbExpression input, DbExpression pattern, DbExpression escape)
		: base(DbExpressionKind.Like, booleanResultType)
	{
		_argument = input;
		_pattern = pattern;
		_escape = escape;
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
