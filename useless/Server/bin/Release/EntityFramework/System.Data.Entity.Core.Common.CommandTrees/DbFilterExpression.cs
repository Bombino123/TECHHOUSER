using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Utilities;

namespace System.Data.Entity.Core.Common.CommandTrees;

public sealed class DbFilterExpression : DbExpression
{
	private readonly DbExpressionBinding _input;

	private readonly DbExpression _predicate;

	public DbExpressionBinding Input => _input;

	public DbExpression Predicate => _predicate;

	internal DbFilterExpression(TypeUsage resultType, DbExpressionBinding input, DbExpression predicate)
		: base(DbExpressionKind.Filter, resultType)
	{
		_input = input;
		_predicate = predicate;
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
