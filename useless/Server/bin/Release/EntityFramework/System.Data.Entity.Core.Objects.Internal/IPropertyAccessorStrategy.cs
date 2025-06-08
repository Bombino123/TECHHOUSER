using System.Data.Entity.Core.Objects.DataClasses;

namespace System.Data.Entity.Core.Objects.Internal;

internal interface IPropertyAccessorStrategy
{
	object GetNavigationPropertyValue(RelatedEnd relatedEnd);

	void SetNavigationPropertyValue(RelatedEnd relatedEnd, object value);

	void CollectionAdd(RelatedEnd relatedEnd, object value);

	bool CollectionRemove(RelatedEnd relatedEnd, object value);

	object CollectionCreate(RelatedEnd relatedEnd);
}
