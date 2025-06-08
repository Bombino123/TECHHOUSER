using System.Collections.Generic;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Resources;
using System.Xml;

namespace System.Data.Entity.Core.SchemaObjectModel;

internal class StructuredProperty : Property
{
	private SchemaType _type;

	private readonly TypeUsageBuilder _typeUsageBuilder;

	private CollectionKind _collectionKind;

	public override SchemaType Type => _type;

	public TypeUsage TypeUsage => _typeUsageBuilder.TypeUsage;

	public bool Nullable => _typeUsageBuilder.Nullable;

	public string Default => _typeUsageBuilder.Default;

	public object DefaultAsObject => _typeUsageBuilder.DefaultAsObject;

	public CollectionKind CollectionKind => _collectionKind;

	internal string UnresolvedType { get; set; }

	internal StructuredProperty(StructuredType parentElement)
		: base(parentElement)
	{
		_typeUsageBuilder = new TypeUsageBuilder(this);
	}

	internal override void ResolveTopLevelNames()
	{
		base.ResolveTopLevelNames();
		if (_type == null)
		{
			_type = ResolveType(UnresolvedType);
			_typeUsageBuilder.ValidateDefaultValue(_type);
			if (_type is ScalarType scalar)
			{
				_typeUsageBuilder.ValidateAndSetTypeUsage(scalar, complainOnMissingFacet: true);
			}
		}
	}

	internal void EnsureEnumTypeFacets(Converter.ConversionCache convertedItemCache, Dictionary<SchemaElement, GlobalItem> newGlobalItems)
	{
		EdmType edmType = (EdmType)Converter.LoadSchemaElement(Type, Type.Schema.ProviderManifest, convertedItemCache, newGlobalItems);
		_typeUsageBuilder.ValidateAndSetTypeUsage(edmType, complainOnMissingFacet: false);
	}

	protected virtual SchemaType ResolveType(string typeName)
	{
		if (!base.Schema.ResolveTypeName(this, typeName, out var type))
		{
			return null;
		}
		if (!(type is SchemaComplexType) && !(type is ScalarType) && !(type is SchemaEnumType))
		{
			AddError(ErrorCode.InvalidPropertyType, EdmSchemaErrorSeverity.Error, Strings.InvalidPropertyType(UnresolvedType));
			return null;
		}
		return type;
	}

	internal override void Validate()
	{
		base.Validate();
		if (_collectionKind != CollectionKind.Bag)
		{
			_ = _collectionKind;
			_ = 2;
		}
		if (_type is SchemaEnumType schemaEnumType)
		{
			_typeUsageBuilder.ValidateEnumFacets(schemaEnumType);
		}
		else if (Nullable && base.Schema.SchemaVersion != 1.1 && _type is SchemaComplexType)
		{
			AddError(ErrorCode.NullableComplexType, EdmSchemaErrorSeverity.Error, Strings.ComplexObject_NullableComplexTypesNotSupported(FQName));
		}
	}

	protected override bool HandleAttribute(XmlReader reader)
	{
		if (base.HandleAttribute(reader))
		{
			return true;
		}
		if (SchemaElement.CanHandleAttribute(reader, "Type"))
		{
			HandleTypeAttribute(reader);
			return true;
		}
		if (SchemaElement.CanHandleAttribute(reader, "CollectionKind"))
		{
			HandleCollectionKindAttribute(reader);
			return true;
		}
		if (_typeUsageBuilder.HandleAttribute(reader))
		{
			return true;
		}
		return false;
	}

	private void HandleTypeAttribute(XmlReader reader)
	{
		string name;
		if (UnresolvedType != null)
		{
			AddError(ErrorCode.AlreadyDefined, EdmSchemaErrorSeverity.Error, reader, Strings.PropertyTypeAlreadyDefined(reader.Name));
		}
		else if (Utils.GetDottedName(base.Schema, reader, out name))
		{
			UnresolvedType = name;
		}
	}

	private void HandleCollectionKindAttribute(XmlReader reader)
	{
		switch (reader.Value)
		{
		case "None":
			_collectionKind = CollectionKind.None;
			break;
		case "List":
			_collectionKind = CollectionKind.List;
			break;
		case "Bag":
			_collectionKind = CollectionKind.Bag;
			break;
		}
	}
}
