using System.Collections.ObjectModel;

namespace System.Net.Http.Headers;

internal class ObjectCollection<T> : Collection<T> where T : class
{
	public extern ObjectCollection();

	public extern ObjectCollection(Action<T> validator);

	protected override extern void InsertItem(int index, T item);

	protected override extern void SetItem(int index, T item);
}
