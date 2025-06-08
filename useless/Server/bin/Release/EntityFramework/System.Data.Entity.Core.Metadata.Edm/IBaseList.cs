using System.Collections;

namespace System.Data.Entity.Core.Metadata.Edm;

internal interface IBaseList<T> : IList, ICollection, IEnumerable
{
	T this[string identity] { get; }

	new T this[int index] { get; }

	int IndexOf(T item);
}
