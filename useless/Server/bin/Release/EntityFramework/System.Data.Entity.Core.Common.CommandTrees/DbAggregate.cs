using System.Collections.Generic;
using System.Data.Entity.Core.Common.CommandTrees.Internal;
using System.Data.Entity.Core.Metadata.Edm;

namespace System.Data.Entity.Core.Common.CommandTrees;

public abstract class DbAggregate
{
	private readonly DbExpressionList _args;

	private readonly TypeUsage _type;

	public TypeUsage ResultType => _type;

	public IList<DbExpression> Arguments => _args;

	internal DbAggregate(TypeUsage resultType, DbExpressionList arguments)
	{
		_type = resultType;
		_args = arguments;
	}
}
