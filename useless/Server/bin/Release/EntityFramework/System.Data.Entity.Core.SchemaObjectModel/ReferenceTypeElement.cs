using System.Collections.Generic;
using System.Data.Entity.Core.Metadata.Edm;
using System.Text;
using System.Xml;

namespace System.Data.Entity.Core.SchemaObjectModel;

internal class ReferenceTypeElement : ModelFunctionTypeElement
{
	internal ReferenceTypeElement(SchemaElement parentElement)
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
			HandleTypeElementAttribute(reader);
			return true;
		}
		return false;
	}

	protected void HandleTypeElementAttribute(XmlReader reader)
	{
		if (Utils.GetString(base.Schema, reader, out var value) && Utils.ValidateDottedName(base.Schema, reader, value))
		{
			_unresolvedType = value;
		}
	}

	internal override void WriteIdentity(StringBuilder builder)
	{
		builder.Append("Ref(" + base.UnresolvedType + ")");
	}

	internal override TypeUsage GetTypeUsage()
	{
		return _typeUsage;
	}

	internal override bool ResolveNameAndSetTypeUsage(Converter.ConversionCache convertedItemCache, Dictionary<SchemaElement, GlobalItem> newGlobalItems)
	{
		if (_typeUsage == null)
		{
			RefType refType = new RefType(((EdmType)Converter.LoadSchemaElement(_type, _type.Schema.ProviderManifest, convertedItemCache, newGlobalItems)) as EntityType);
			refType.AddMetadataProperties(base.OtherContent);
			_typeUsage = TypeUsage.Create(refType);
		}
		return true;
	}

	internal override void Validate()
	{
		base.Validate();
		ValidationHelper.ValidateRefType(this, _type);
	}
}
