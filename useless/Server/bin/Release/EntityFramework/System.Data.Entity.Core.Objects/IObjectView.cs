using System.ComponentModel;

namespace System.Data.Entity.Core.Objects;

internal interface IObjectView
{
	void EntityPropertyChanged(object sender, PropertyChangedEventArgs e);

	void CollectionChanged(object sender, CollectionChangeEventArgs e);
}
