using System.Collections;
using System.ComponentModel;
using System.Data.Common;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Utilities;
using System.Linq;
using System.Reflection;

namespace System.Data.Entity.Core.Objects;

internal sealed class DataRecordObjectView : ObjectView<DbDataRecord>, ITypedList
{
	private readonly PropertyDescriptorCollection _propertyDescriptorsCache;

	private readonly RowType _rowType;

	internal DataRecordObjectView(IObjectViewData<DbDataRecord> viewData, object eventDataSource, RowType rowType, Type propertyComponentType)
		: base(viewData, eventDataSource)
	{
		if (!typeof(IDataRecord).IsAssignableFrom(propertyComponentType))
		{
			propertyComponentType = typeof(IDataRecord);
		}
		_rowType = rowType;
		_propertyDescriptorsCache = MaterializedDataRecord.CreatePropertyDescriptorCollection(_rowType, propertyComponentType, isReadOnly: true);
	}

	private static PropertyInfo GetTypedIndexer(Type type)
	{
		PropertyInfo propertyInfo = null;
		if (typeof(IList).IsAssignableFrom(type) || typeof(ITypedList).IsAssignableFrom(type) || typeof(IListSource).IsAssignableFrom(type))
		{
			foreach (PropertyInfo item in from p in type.GetInstanceProperties()
				where p.IsPublic()
				select p)
			{
				if (item.GetIndexParameters().Length != 0 && item.PropertyType != typeof(object))
				{
					propertyInfo = item;
					if (propertyInfo.Name == "Item")
					{
						break;
					}
				}
			}
		}
		return propertyInfo;
	}

	private static Type GetListItemType(Type type)
	{
		if (typeof(Array).IsAssignableFrom(type))
		{
			return type.GetElementType();
		}
		PropertyInfo typedIndexer = GetTypedIndexer(type);
		if (typedIndexer != null)
		{
			return typedIndexer.PropertyType;
		}
		return type;
	}

	PropertyDescriptorCollection ITypedList.GetItemProperties(PropertyDescriptor[] listAccessors)
	{
		if (listAccessors == null || listAccessors.Length == 0)
		{
			return _propertyDescriptorsCache;
		}
		PropertyDescriptor propertyDescriptor = listAccessors[^1];
		if (propertyDescriptor is FieldDescriptor { EdmProperty: not null } fieldDescriptor && fieldDescriptor.EdmProperty.TypeUsage.EdmType.BuiltInTypeKind == BuiltInTypeKind.RowType)
		{
			return MaterializedDataRecord.CreatePropertyDescriptorCollection((RowType)fieldDescriptor.EdmProperty.TypeUsage.EdmType, typeof(IDataRecord), isReadOnly: true);
		}
		return TypeDescriptor.GetProperties(GetListItemType(propertyDescriptor.PropertyType));
	}

	string ITypedList.GetListName(PropertyDescriptor[] listAccessors)
	{
		return _rowType.Name;
	}
}
