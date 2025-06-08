using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Common;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Core.Objects.DataClasses;
using System.Data.Entity.Utilities;

namespace System.Data.Entity.Core.Objects;

internal static class ObjectViewFactory
{
	private static readonly Type _genericObjectViewType = typeof(ObjectView<>);

	private static readonly Type _genericObjectViewDataInterfaceType = typeof(IObjectViewData<>);

	private static readonly Type _genericObjectViewQueryResultDataType = typeof(ObjectViewQueryResultData<>);

	private static readonly Type _genericObjectViewEntityCollectionDataType = typeof(ObjectViewEntityCollectionData<, >);

	internal static IBindingList CreateViewForQuery<TElement>(TypeUsage elementEdmTypeUsage, IEnumerable<TElement> queryResults, ObjectContext objectContext, bool forceReadOnly, EntitySet singleEntitySet)
	{
		Type type = null;
		TypeUsage oSpaceTypeUsage = GetOSpaceTypeUsage(elementEdmTypeUsage, objectContext);
		if (oSpaceTypeUsage == null)
		{
			type = typeof(TElement);
		}
		type = GetClrType<TElement>(oSpaceTypeUsage.EdmType);
		object objectStateManager = objectContext.ObjectStateManager;
		if (type == typeof(TElement))
		{
			return new ObjectView<TElement>(new ObjectViewQueryResultData<TElement>(queryResults, objectContext, forceReadOnly, singleEntitySet), objectStateManager);
		}
		if (type == null)
		{
			return new DataRecordObjectView(new ObjectViewQueryResultData<DbDataRecord>(queryResults, objectContext, forceReadOnlyList: true, null), objectStateManager, (RowType)oSpaceTypeUsage.EdmType, typeof(TElement));
		}
		if (!typeof(TElement).IsAssignableFrom(type))
		{
			throw EntityUtil.ValueInvalidCast(type, typeof(TElement));
		}
		Type type2 = _genericObjectViewQueryResultDataType.MakeGenericType(type);
		object viewData = type2.GetDeclaredConstructor(typeof(IEnumerable), typeof(ObjectContext), typeof(bool), typeof(EntitySet)).Invoke(new object[4] { queryResults, objectContext, forceReadOnly, singleEntitySet });
		return CreateObjectView(type, type2, viewData, objectStateManager);
	}

	internal static IBindingList CreateViewForEntityCollection<TElement>(EntityType entityType, EntityCollection<TElement> entityCollection) where TElement : class
	{
		Type type = null;
		TypeUsage oSpaceTypeUsage = GetOSpaceTypeUsage((entityType == null) ? null : TypeUsage.Create(entityType), entityCollection.ObjectContext);
		if (oSpaceTypeUsage == null)
		{
			type = typeof(TElement);
		}
		else
		{
			type = GetClrType<TElement>(oSpaceTypeUsage.EdmType);
			if (type == null)
			{
				type = typeof(TElement);
			}
		}
		if (type == typeof(TElement))
		{
			return new ObjectView<TElement>(new ObjectViewEntityCollectionData<TElement, TElement>(entityCollection), entityCollection);
		}
		if (!typeof(TElement).IsAssignableFrom(type))
		{
			throw EntityUtil.ValueInvalidCast(type, typeof(TElement));
		}
		Type type2 = _genericObjectViewEntityCollectionDataType.MakeGenericType(type, typeof(TElement));
		object viewData = type2.GetDeclaredConstructor(typeof(EntityCollection<TElement>)).Invoke(new object[1] { entityCollection });
		return CreateObjectView(type, type2, viewData, entityCollection);
	}

	private static IBindingList CreateObjectView(Type clrElementType, Type objectViewDataType, object viewData, object eventDataSource)
	{
		Type type2 = _genericObjectViewType.MakeGenericType(clrElementType);
		Type[] array = objectViewDataType.FindInterfaces((Type type, object unusedFilter) => type.Name == _genericObjectViewDataInterfaceType.Name, null);
		return (IBindingList)type2.GetDeclaredConstructor(array[0], typeof(object)).Invoke(new object[2] { viewData, eventDataSource });
	}

	private static TypeUsage GetOSpaceTypeUsage(TypeUsage typeUsage, ObjectContext objectContext)
	{
		if (typeUsage == null || typeUsage.EdmType == null)
		{
			return null;
		}
		if (typeUsage.EdmType.DataSpace == DataSpace.OSpace)
		{
			return typeUsage;
		}
		return objectContext?.Perspective.MetadataWorkspace.GetOSpaceTypeUsage(typeUsage);
	}

	private static Type GetClrType<TElement>(EdmType ospaceEdmType)
	{
		Type type;
		if (ospaceEdmType.BuiltInTypeKind == BuiltInTypeKind.RowType)
		{
			RowType rowType = (RowType)ospaceEdmType;
			if (rowType.InitializerMetadata != null && rowType.InitializerMetadata.ClrType != null)
			{
				type = rowType.InitializerMetadata.ClrType;
			}
			else
			{
				Type typeFromHandle = typeof(TElement);
				type = ((!typeof(IDataRecord).IsAssignableFrom(typeFromHandle) && !(typeFromHandle == typeof(object))) ? typeof(TElement) : null);
			}
		}
		else
		{
			type = ospaceEdmType.ClrType;
			if (type == null)
			{
				type = typeof(TElement);
			}
		}
		return type;
	}
}
