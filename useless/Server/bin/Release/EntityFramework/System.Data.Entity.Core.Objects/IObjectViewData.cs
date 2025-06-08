using System.Collections.Generic;
using System.ComponentModel;

namespace System.Data.Entity.Core.Objects;

internal interface IObjectViewData<T>
{
	IList<T> List { get; }

	bool AllowNew { get; }

	bool AllowEdit { get; }

	bool AllowRemove { get; }

	bool FiresEventOnAdd { get; }

	bool FiresEventOnRemove { get; }

	bool FiresEventOnClear { get; }

	void EnsureCanAddNew();

	int Add(T item, bool isAddNew);

	void CommitItemAt(int index);

	void Clear();

	bool Remove(T item, bool isCancelNew);

	ListChangedEventArgs OnCollectionChanged(object sender, CollectionChangeEventArgs e, ObjectViewListener listener);
}
