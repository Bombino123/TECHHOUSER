using System.Data.Entity.Core.Common.CommandTrees.Internal;
using System.Data.Entity.Core.Metadata.Edm;

namespace System.Data.Entity.Core.Common.CommandTrees;

public sealed class DbGroupAggregate : DbAggregate
{
	internal DbGroupAggregate(TypeUsage resultType, DbExpressionList arguments)
		: base(resultType, arguments)
	{
	}
}
