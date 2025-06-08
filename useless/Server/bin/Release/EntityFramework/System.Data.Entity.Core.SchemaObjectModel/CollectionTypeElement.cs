using System.Collections.Generic;
using System.Data.Entity.Core.Metadata.Edm;
using System.Text;
using System.Xml;

namespace System.Data.Entity.Core.SchemaObjectModel;

internal class CollectionTypeElement : ModelFunctionTypeElement
{
	private ModelFunctionTypeElement _typeSubElement;

	internal ModelFunctionTypeElement SubElement => _typeSubElement;

	internal CollectionTypeElement(SchemaElement parentElement)
		: base(parentElement)
	{
	}

	protected override bool HandleAttribute(XmlReader reader)
	{
		if (base.HandleAttribute(reader))
		{
			return true;
		}
		if (SchemaElement.CanHandleAttribute(reader, "ElementType"))
		{
			HandleElementTypeAttribute(reader);
			return true;
		}
		return false;
	}

	protected void HandleElementTypeAttribute(XmlReader reader)
	{
		if (Utils.GetString(base.Schema, reader, out var value) && Utils.ValidateDottedName(base.Schema, reader, value))
		{
			_unresolvedType = value;
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

	internal override void ResolveTopLevelNames()
	{
		if (_typeSubElement != null)
		{
			_typeSubElement.ResolveTopLevelNames();
		}
		if (_unresolvedType != null)
		{
			base.ResolveTopLevelNames();
		}
	}

	internal override void WriteIdentity(StringBuilder builder)
	{
		if (!string.IsNullOrWhiteSpace(base.UnresolvedType))
		{
			builder.Append("Collection(" + base.UnresolvedType + ")");
			return;
		}
		builder.Append("Collection(");
		_typeSubElement.WriteIdentity(builder);
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
			CollectionType collectionType = new CollectionType(_typeSubElement.GetTypeUsage());
			collectionType.AddMetadataProperties(base.OtherContent);
			_typeUsage = TypeUsage.Create(collectionType);
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
				_typeUsage = TypeUsage.Create(new CollectionType(_typeUsageBuilder.TypeUsage));
				return true;
			}
			EdmType edmType = (EdmType)Converter.LoadSchemaElement(_type, _type.Schema.ProviderManifest, convertedItemCache, newGlobalItems);
			if (edmType != null)
			{
				_typeUsageBuilder.ValidateAndSetTypeUsage(edmType, complainOnMissingFacet: false);
				_typeUsage = TypeUsage.Create(new CollectionType(_typeUsageBuilder.TypeUsage));
			}
			return _typeUsage != null;
		}
		return true;
	}

	internal override void Validate()
	{
		base.Validate();
		ValidationHelper.ValidateFacets(this, _type, _typeUsageBuilder);
		ValidationHelper.ValidateTypeDeclaration(this, _type, _typeSubElement);
		if (_typeSubElement != null)
		{
			_typeSubElement.Validate();
		}
	}
}
