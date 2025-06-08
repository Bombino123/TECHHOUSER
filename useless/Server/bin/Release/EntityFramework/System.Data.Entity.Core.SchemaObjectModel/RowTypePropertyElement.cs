using System.Collections.Generic;
using System.Data.Entity.Core.Metadata.Edm;
using System.Text;
using System.Xml;

namespace System.Data.Entity.Core.SchemaObjectModel;

internal class RowTypePropertyElement : ModelFunctionTypeElement
{
	private ModelFunctionTypeElement _typeSubElement;

	private bool _isRefType;

	private CollectionKind _collectionKind;

	internal RowTypePropertyElement(SchemaElement parentElement)
		: base(parentElement)
	{
		_typeUsageBuilder = new TypeUsageBuilder(this);
	}

	internal override void ResolveTopLevelNames()
	{
		if (_unresolvedType != null)
		{
			base.ResolveTopLevelNames();
		}
		if (_typeSubElement != null)
		{
			_typeSubElement.ResolveTopLevelNames();
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
		return false;
	}

	protected void HandleTypeAttribute(XmlReader reader)
	{
		if (Utils.GetString(base.Schema, reader, out var value))
		{
			Function.RemoveTypeModifier(ref value, out var typeModifier, out _isRefType);
			if (typeModifier == TypeModifier.Array)
			{
				_collectionKind = CollectionKind.Bag;
			}
			if (Utils.ValidateDottedName(base.Schema, reader, value))
			{
				_unresolvedType = value;
			}
		}
	}

	protected override bool HandleElement(XmlReader reader)
	{
		if (CanHandleElement(reader, "CollectionType"))
		{
			HandleCollectionTypeElement(reader);
			return true;
		}
		if (CanHandleElement(reader, "ReferenceType"))
		{
			HandleReferenceTypeElement(reader);
			return true;
		}
		if (CanHandleElement(reader, "TypeRef"))
		{
			HandleTypeRefElement(reader);
			return true;
		}
		if (CanHandleElement(reader, "RowType"))
		{
			HandleRowTypeElement(reader);
			return true;
		}
		return false;
	}

	protected void HandleCollectionTypeElement(XmlReader reader)
	{
		CollectionTypeElement collectionTypeElement = new CollectionTypeElement(this);
		collectionTypeElement.Parse(reader);
		_typeSubElement = collectionTypeElement;
	}

	protected void HandleReferenceTypeElement(XmlReader reader)
	{
		ReferenceTypeElement referenceTypeElement = new ReferenceTypeElement(this);
		referenceTypeElement.Parse(reader);
		_typeSubElement = referenceTypeElement;
	}

	protected void HandleTypeRefElement(XmlReader reader)
	{
		TypeRefElement typeRefElement = new TypeRefElement(this);
		typeRefElement.Parse(reader);
		_typeSubElement = typeRefElement;
	}

	protected void HandleRowTypeElement(XmlReader reader)
	{
		RowTypeElement rowTypeElement = new RowTypeElement(this);
		rowTypeElement.Parse(reader);
		_typeSubElement = rowTypeElement;
	}

	internal override void WriteIdentity(StringBuilder builder)
	{
		builder.Append("Property(");
		if (!string.IsNullOrWhiteSpace(base.UnresolvedType))
		{
			if (_collectionKind != 0)
			{
				builder.Append("Collection(" + base.UnresolvedType + ")");
			}
			else if (_isRefType)
			{
				builder.Append("Ref(" + base.UnresolvedType + ")");
			}
			else
			{
				builder.Append(base.UnresolvedType);
			}
		}
		else
		{
			_typeSubElement.WriteIdentity(builder);
		}
		builder.Append(")");
	}

	internal override TypeUsage GetTypeUsage()
	{
		if (_typeUsage != null)
		{
			return _typeUsage;
		}
		if (_typeSubElement != null)
		{
			_typeUsage = _typeSubElement.GetTypeUsage();
		}
		return _typeUsage;
	}

	internal override bool ResolveNameAndSetTypeUsage(Converter.ConversionCache convertedItemCache, Dictionary<SchemaElement, GlobalItem> newGlobalItems)
	{
		if (_typeUsage == null)
		{
			if (_typeSubElement != null)
			{
				return _typeSubElement.ResolveNameAndSetTypeUsage(convertedItemCache, newGlobalItems);
			}
			if (_type is ScalarType)
			{
				_typeUsageBuilder.ValidateAndSetTypeUsage(_type as ScalarType, complainOnMissingFacet: false);
				_typeUsage = _typeUsageBuilder.TypeUsage;
			}
			else
			{
				EdmType edmType = (EdmType)Converter.LoadSchemaElement(_type, _type.Schema.ProviderManifest, convertedItemCache, newGlobalItems);
				if (edmType != null)
				{
					if (_isRefType)
					{
						EntityType entityType = edmType as EntityType;
						_typeUsage = TypeUsage.Create(new RefType(entityType));
					}
					else
					{
						_typeUsageBuilder.ValidateAndSetTypeUsage(edmType, complainOnMissingFacet: false);
						_typeUsage = _typeUsageBuilder.TypeUsage;
					}
				}
			}
			if (_collectionKind != 0)
			{
				_typeUsage = TypeUsage.Create(new CollectionType(_typeUsage));
			}
			return _typeUsage != null;
		}
		return true;
	}

	internal bool ValidateIsScalar()
	{
		if (_type != null)
		{
			if (!(_type is ScalarType) || _isRefType || _collectionKind != 0)
			{
				return false;
			}
		}
		else if (_typeSubElement != null && !(_typeSubElement.Type is ScalarType))
		{
			return false;
		}
		return true;
	}

	internal override void Validate()
	{
		base.Validate();
		ValidationHelper.ValidateFacets(this, _type, _typeUsageBuilder);
		ValidationHelper.ValidateTypeDeclaration(this, _type, _typeSubElement);
		if (_isRefType)
		{
			ValidationHelper.ValidateRefType(this, _type);
		}
		if (_typeSubElement != null)
		{
			_typeSubElement.Validate();
		}
	}
}
