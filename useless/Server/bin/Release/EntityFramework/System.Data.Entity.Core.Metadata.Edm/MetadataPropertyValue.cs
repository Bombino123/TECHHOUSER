using System.Reflection;

namespace System.Data.Entity.Core.Metadata.Edm;

internal sealed class MetadataPropertyValue
{
	private readonly PropertyInfo _propertyInfo;

	private readonly MetadataItem _item;

	internal MetadataPropertyValue(PropertyInfo propertyInfo, MetadataItem item)
	{
		_propertyInfo = propertyInfo;
		_item = item;
	}

	internal object GetValue()
	{
		return _propertyInfo.GetValue(_item, new object[0]);
	}
}
