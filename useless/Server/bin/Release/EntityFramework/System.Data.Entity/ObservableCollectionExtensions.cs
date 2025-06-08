using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.Entity.Internal;
using System.Data.Entity.Utilities;

namespace System.Data.Entity;

public static class ObservableCollectionExtensions
{
	public static BindingList<T> ToBindingList<T>(this ObservableCollection<T> source) where T : class
	{
		Check.NotNull(source, "source");
		if (!(source is DbLocalView<T> dbLocalView))
		{
			return new ObservableBackedBindingList<T>(source);
		}
		return dbLocalView.BindingList;
	}
}
