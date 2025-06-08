using System.Collections.Generic;
using System.Data.Entity.Core.Common.Utils;
using System.Data.Entity.Utilities;
using System.Reflection;

namespace System.Data.Entity.Core.Metadata.Edm;

internal sealed class MetadataPropertyCollection : MetadataCollection<MetadataProperty>
{
	private class ItemTypeInformation
	{
		private readonly List<ItemPropertyInfo> _itemProperties;

		internal ItemTypeInformation(Type clrType)
		{
			_itemProperties = GetItemProperties(clrType);
		}

		internal IEnumerable<MetadataProperty> GetItemAttributes(MetadataItem item)
		{
			foreach (ItemPropertyInfo itemProperty in _itemProperties)
			{
				yield return itemProperty.GetMetadataProperty(item);
			}
		}

		private static List<ItemPropertyInfo> GetItemProperties(Type clrType)
		{
			List<ItemPropertyInfo> list = new List<ItemPropertyInfo>();
			foreach (PropertyInfo instanceProperty in clrType.GetInstanceProperties())
			{
				foreach (MetadataPropertyAttribute customAttribute in instanceProperty.GetCustomAttributes<MetadataPropertyAttribute>(inherit: false))
				{
					list.Add(new ItemPropertyInfo(instanceProperty, customAttribute));
				}
			}
			return list;
		}
	}

	private class ItemPropertyInfo
	{
		private readonly MetadataPropertyAttribute _attribute;

		private readonly PropertyInfo _propertyInfo;

		internal ItemPropertyInfo(PropertyInfo propertyInfo, MetadataPropertyAttribute attribute)
		{
			_propertyInfo = propertyInfo;
			_attribute = attribute;
		}

		internal MetadataProperty GetMetadataProperty(MetadataItem item)
		{
			return new MetadataProperty(_propertyInfo.Name, _attribute.Type, _attribute.IsCollectionType, new MetadataPropertyValue(_propertyInfo, item));
		}
	}

	private static readonly Memoizer<Type, ItemTypeInformation> _itemTypeMemoizer = new Memoizer<Type, ItemTypeInformation>((Type clrType) => new ItemTypeInformation(clrType), null);

	internal MetadataPropertyCollection(MetadataItem item)
		: base(GetSystemMetadataProperties(item))
	{
	}

	private static IEnumerable<MetadataProperty> GetSystemMetadataProperties(MetadataItem item)
	{
		return GetItemTypeInformation(item.GetType()).GetItemAttributes(item);
	}

	private static ItemTypeInformation GetItemTypeInformation(Type clrType)
	{
		return _itemTypeMemoizer.Evaluate(clrType);
	}
}
