using System.Collections.Generic;
using System.Data.Entity.Core.Common.CommandTrees.Internal;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Utilities;

namespace System.Data.Entity.Core.Common.CommandTrees;

public sealed class DbArithmeticExpression : DbExpression
{
	private readonly DbExpressionList _args;

	public IList<DbExpression> Arguments => _args;

	internal DbArithmeticExpression(DbExpressionKind kind, TypeUsage numericResultType, DbExpressionList args)
		: base(kind, numericResultType)
	{
		_args = args;
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
