using System.Data.Entity.Utilities;

namespace System.Data.Entity.Core.Metadata.Edm;

public class MetadataProperty : MetadataItem
{
	private readonly string _name;

	private readonly PropertyKind _propertyKind;

	private object _value;

	private readonly TypeUsage _typeUsage;

	public override BuiltInTypeKind BuiltInTypeKind => BuiltInTypeKind.MetadataProperty;

	internal override string Identity => Name;

	[MetadataProperty(PrimitiveTypeKind.String, false)]
	public virtual string Name => _name;

	[MetadataProperty(typeof(object), false)]
	public virtual object Value
	{
		get
		{
			if (_value is MetadataPropertyValue metadataPropertyValue)
			{
				return metadataPropertyValue.GetValue();
			}
			return _value;
		}
		set
		{
			Check.NotNull(value, "value");
			Util.ThrowIfReadOnly(this);
			_value = value;
		}
	}

	[MetadataProperty(BuiltInTypeKind.TypeUsage, false)]
	public TypeUsage TypeUsage => _typeUsage;

	public virtual PropertyKind PropertyKind => _propertyKind;

	public bool IsAnnotation
	{
		get
		{
			if (PropertyKind == PropertyKind.Extended)
			{
				return TypeUsage == null;
			}
			return false;
		}
	}

	internal MetadataProperty()
	{
	}

	internal MetadataProperty(string name, TypeUsage typeUsage, object value)
	{
		Check.NotNull(typeUsage, "typeUsage");
		_name = name;
		_value = value;
		_typeUsage = typeUsage;
		_propertyKind = PropertyKind.Extended;
	}

	internal MetadataProperty(string name, EdmType edmType, bool isCollectionType, object value)
	{
		_name = name;
		_value = value;
		if (isCollectionType)
		{
			_typeUsage = TypeUsage.Create(edmType.GetCollectionType());
		}
		else
		{
			_typeUsage = TypeUsage.Create(edmType);
		}
		_propertyKind = PropertyKind.System;
	}

	private MetadataProperty(string name, object value)
	{
		_name = name;
		_value = value;
		_propertyKind = PropertyKind.Extended;
	}

	internal override void SetReadOnly()
	{
		if (!base.IsReadOnly)
		{
			base.SetReadOnly();
		}
	}

	public static MetadataProperty Create(string name, TypeUsage typeUsage, object value)
	{
		Check.NotEmpty(name, "name");
		Check.NotNull(typeUsage, "typeUsage");
		MetadataProperty metadataProperty = new MetadataProperty(name, typeUsage, value);
		metadataProperty.SetReadOnly();
		return metadataProperty;
	}

	public static MetadataProperty CreateAnnotation(string name, object value)
	{
		Check.NotEmpty(name, "name");
		return new MetadataProperty(name, value);
	}
}
