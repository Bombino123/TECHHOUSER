using System.Collections.Generic;
using System.Data.Entity.Core.Metadata.Edm;
using System.Text;
using System.Xml;

namespace System.Data.Entity.Core.SchemaObjectModel;

internal class TypeRefElement : ModelFunctionTypeElement
{
	internal TypeRefElement(SchemaElement parentElement)
		: base(parentElement)
	{
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
		if (Utils.GetString(base.Schema, reader, out var value) && Utils.ValidateDottedName(base.Schema, reader, value))
		{
			_unresolvedType = value;
		}
	}

	internal override bool ResolveNameAndSetTypeUsage(Converter.ConversionCache convertedItemCache, Dictionary<SchemaElement, GlobalItem> newGlobalItems)
	{
		if (_type is ScalarType)
		{
			_typeUsageBuilder.ValidateAndSetTypeUsage(_type as ScalarType, complainOnMissingFacet: false);
			_typeUsage = _typeUsageBuilder.TypeUsage;
			return true;
		}
		EdmType edmType = (EdmType)Converter.LoadSchemaElement(_type, _type.Schema.ProviderManifest, convertedItemCache, newGlobalItems);
		if (edmType != null)
		{
			_typeUsageBuilder.ValidateAndSetTypeUsage(edmType, complainOnMissingFacet: false);
			_typeUsage = _typeUsageBuilder.TypeUsage;
		}
		return _typeUsage != null;
	}

	internal override void WriteIdentity(StringBuilder builder)
	{
		builder.Append(base.UnresolvedType);
	}

	internal override TypeUsage GetTypeUsage()
	{
		return _typeUsage;
	}

	internal override void Validate()
	{
		base.Validate();
		ValidationHelper.ValidateFacets(this, _type, _typeUsageBuilder);
	}
}
