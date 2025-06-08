using System.Data.Entity.Core.Common.CommandTrees;
using System.Linq.Expressions;

namespace System.Data.Entity.Core.Objects.ELinq;

internal sealed class Binding
{
	internal readonly Expression LinqExpression;

	internal readonly DbExpression CqtExpression;

	internal Binding(Expression linqExpression, DbExpression cqtExpression)
	{
		LinqExpression = linqExpression;
		CqtExpression = cqtExpression;
	}
}
