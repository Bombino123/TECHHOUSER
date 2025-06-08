using System.Collections.Generic;
using System.Data.Entity.Core.Common.CommandTrees.Internal;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Utilities;

namespace System.Data.Entity.Core.Common.CommandTrees;

public sealed class DbCaseExpression : DbExpression
{
	private readonly DbExpressionList _when;

	private readonly DbExpressionList _then;

	private readonly DbExpression _else;

	public IList<DbExpression> When => _when;

	public IList<DbExpression> Then => _then;

	public DbExpression Else => _else;

	internal DbCaseExpression(TypeUsage commonResultType, DbExpressionList whens, DbExpressionList thens, DbExpression elseExpr)
		: base(DbExpressionKind.Case, commonResultType)
	{
		_when = whens;
		_then = thens;
		_else = elseExpr;
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
