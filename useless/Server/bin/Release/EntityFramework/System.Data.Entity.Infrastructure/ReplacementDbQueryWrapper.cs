using System.Data.Entity.Core.Objects;

namespace System.Data.Entity.Infrastructure;

public sealed class ReplacementDbQueryWrapper<TElement>
{
	private readonly ObjectQuery<TElement> _query;

	public ObjectQuery<TElement> Query => _query;

	private ReplacementDbQueryWrapper(ObjectQuery<TElement> query)
	{
		_query = query;
	}

	internal static ReplacementDbQueryWrapper<TElement> Create(ObjectQuery query)
	{
		return new ReplacementDbQueryWrapper<TElement>((ObjectQuery<TElement>)query);
	}
}
