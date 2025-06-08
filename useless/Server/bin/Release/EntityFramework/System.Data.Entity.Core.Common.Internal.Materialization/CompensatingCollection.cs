using System.Collections;
using System.Collections.Generic;
using System.Data.Entity.Resources;
using System.Linq;
using System.Linq.Expressions;

namespace System.Data.Entity.Core.Common.Internal.Materialization;

internal class CompensatingCollection<TElement> : IOrderedQueryable<TElement>, IQueryable<TElement>, IEnumerable<TElement>, IEnumerable, IQueryable, IOrderedQueryable, IOrderedEnumerable<TElement>
{
	private readonly IEnumerable<TElement> _source;

	private readonly Expression _expression;

	Type IQueryable.ElementType => typeof(TElement);

	Expression IQueryable.Expression => _expression;

	IQueryProvider IQueryable.Provider
	{
		get
		{
			throw new NotSupportedException(Strings.ELinq_UnsupportedQueryableMethod);
		}
	}

	public CompensatingCollection(IEnumerable<TElement> source)
	{
		_source = source;
		_expression = Expression.Constant(source);
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return _source.GetEnumerator();
	}

	IEnumerator<TElement> IEnumerable<TElement>.GetEnumerator()
	{
		return _source.GetEnumerator();
	}

	IOrderedEnumerable<TElement> IOrderedEnumerable<TElement>.CreateOrderedEnumerable<K>(Func<TElement, K> keySelector, IComparer<K> comparer, bool descending)
	{
		throw new NotSupportedException(Strings.ELinq_CreateOrderedEnumerableNotSupported);
	}
}
