using System.Data.Entity.Core.Objects;
using System.Data.Entity.Utilities;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace System.Data.Entity.Internal.Linq;

internal class NonGenericDbQueryProvider : DbQueryProvider
{
	public NonGenericDbQueryProvider(InternalContext internalContext, IInternalQuery internalQuery)
		: base(internalContext, internalQuery)
	{
	}

	public override IQueryable<TElement> CreateQuery<TElement>(Expression expression)
	{
		Check.NotNull(expression, "expression");
		ObjectQuery objectQuery = CreateObjectQuery(expression);
		if (typeof(TElement) != ((IQueryable)objectQuery).ElementType)
		{
			return (IQueryable<TElement>)CreateQuery(objectQuery);
		}
		return new InternalDbQuery<TElement>(new InternalQuery<TElement>(base.InternalContext, objectQuery));
	}

	public override IQueryable CreateQuery(Expression expression)
	{
		Check.NotNull(expression, "expression");
		return CreateQuery(CreateObjectQuery(expression));
	}

	private IQueryable CreateQuery(ObjectQuery objectQuery)
	{
		IInternalQuery internalQuery = CreateInternalQuery(objectQuery);
		return (IQueryable)typeof(InternalDbQuery<>).MakeGenericType(internalQuery.ElementType).GetConstructors(BindingFlags.Instance | BindingFlags.Public).Single()
			.Invoke(new object[1] { internalQuery });
	}
}
