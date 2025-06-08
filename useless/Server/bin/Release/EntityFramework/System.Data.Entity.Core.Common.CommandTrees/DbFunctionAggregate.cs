using System.Data.Entity.Core.Common.CommandTrees.Internal;
using System.Data.Entity.Core.Metadata.Edm;

namespace System.Data.Entity.Core.Common.CommandTrees;

public sealed class DbFunctionAggregate : DbAggregate
{
	private readonly bool _distinct;

	private readonly EdmFunction _aggregateFunction;

	public bool Distinct => _distinct;

	public EdmFunction Function => _aggregateFunction;

	internal DbFunctionAggregate(TypeUsage resultType, DbExpressionList arguments, EdmFunction function, bool isDistinct)
		: base(resultType, arguments)
	{
		_aggregateFunction = function;
		_distinct = isDistinct;
	}
}
