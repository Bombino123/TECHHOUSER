using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Common;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Resources;
using System.Data.Entity.Utilities;

namespace System.Data.Entity.Core.Objects;

internal sealed class FieldDescriptor : PropertyDescriptor
{
	private readonly EdmProperty _property;

	private readonly Type _fieldType;

	private readonly Type _itemType;

	private readonly bool _isReadOnly;

	internal EdmProperty EdmProperty => _property;

	public override Type ComponentType => _itemType;

	public override bool IsReadOnly => _isReadOnly;

	public override Type PropertyType => _fieldType;

	public override bool IsBrowsable => true;

	internal FieldDescriptor(string propertyName)
		: base(propertyName, null)
	{
	}

	internal FieldDescriptor(Type itemType, bool isReadOnly, EdmProperty property)
		: base(property.Name, null)
	{
		_itemType = itemType;
		_property = property;
		_isReadOnly = isReadOnly;
		_fieldType = DetermineClrType(_property.TypeUsage);
	}

	private Type DetermineClrType(TypeUsage typeUsage)
	{
		Type type = null;
		EdmType edmType = typeUsage.EdmType;
		switch (edmType.BuiltInTypeKind)
		{
		case BuiltInTypeKind.ComplexType:
		case BuiltInTypeKind.EntityType:
			type = edmType.ClrType;
			break;
		case BuiltInTypeKind.RefType:
			type = typeof(EntityKey);
			break;
		case BuiltInTypeKind.CollectionType:
		{
			TypeUsage typeUsage2 = ((CollectionType)edmType).TypeUsage;
			type = DetermineClrType(typeUsage2);
			type = typeof(IEnumerable<>).MakeGenericType(type);
			break;
		}
		case BuiltInTypeKind.EnumType:
		case BuiltInTypeKind.PrimitiveType:
		{
			type = edmType.ClrType;
			if (type.IsValueType() && typeUsage.Facets.TryGetValue("Nullable", ignoreCase: false, out var item) && (bool)item.Value)
			{
				type = typeof(Nullable<>).MakeGenericType(type);
			}
			break;
		}
		case BuiltInTypeKind.RowType:
			type = typeof(IDataRecord);
			break;
		}
		return type;
	}

	public override bool CanResetValue(object item)
	{
		return false;
	}

	public override object GetValue(object item)
	{
		Check.NotNull(item, "item");
		if (!_itemType.IsAssignableFrom(item.GetType()))
		{
			throw new ArgumentException(Strings.ObjectView_IncompatibleArgument);
		}
		if (item is DbDataRecord dbDataRecord)
		{
			return dbDataRecord.GetValue(dbDataRecord.GetOrdinal(_property.Name));
		}
		return DelegateFactory.GetValue(_property, item);
	}

	public override void ResetValue(object item)
	{
		throw new NotSupportedException();
	}

	public override void SetValue(object item, object value)
	{
		Check.NotNull(item, "item");
		if (!_itemType.IsAssignableFrom(item.GetType()))
		{
			throw new ArgumentException(Strings.ObjectView_IncompatibleArgument);
		}
		if (!_isReadOnly)
		{
			DelegateFactory.SetValue(_property, item, value);
			return;
		}
		throw new InvalidOperationException(Strings.ObjectView_WriteOperationNotAllowedOnReadOnlyBindingList);
	}

	public override bool ShouldSerializeValue(object item)
	{
		return false;
	}
}
