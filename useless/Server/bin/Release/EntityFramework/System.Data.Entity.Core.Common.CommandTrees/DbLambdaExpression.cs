using System.Collections.Generic;
using System.Data.Entity.Core.Common.CommandTrees.Internal;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Utilities;

namespace System.Data.Entity.Core.Common.CommandTrees;

public sealed class DbLambdaExpression : DbExpression
{
	private readonly DbLambda _lambda;

	private readonly DbExpressionList _arguments;

	public DbLambda Lambda => _lambda;

	public IList<DbExpression> Arguments => _arguments;

	internal DbLambdaExpression(TypeUsage resultType, DbLambda lambda, DbExpressionList args)
		: base(DbExpressionKind.Lambda, resultType)
	{
		_lambda = lambda;
		_arguments = args;
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
