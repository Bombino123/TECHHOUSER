using System.Collections.Generic;
using System.Data.Entity.Core.Common.CommandTrees.Internal;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Utilities;

namespace System.Data.Entity.Core.Common.CommandTrees;

public class DbFunctionExpression : DbExpression
{
	private readonly EdmFunction _functionInfo;

	private readonly DbExpressionList _arguments;

	public virtual EdmFunction Function => _functionInfo;

	public virtual IList<DbExpression> Arguments => _arguments;

	internal DbFunctionExpression()
	{
	}

	internal DbFunctionExpression(TypeUsage resultType, EdmFunction function, DbExpressionList arguments)
		: base(DbExpressionKind.Function, resultType)
	{
		_functionInfo = function;
		_arguments = arguments;
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
