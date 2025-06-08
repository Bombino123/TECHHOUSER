using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Resources;
using System.Xml;

namespace System.Data.Entity.Core.SchemaObjectModel;

internal sealed class SchemaComplexType : StructuredType
{
	internal SchemaComplexType(Schema parentElement)
		: base(parentElement)
	{
		if (base.Schema.DataModel == SchemaDataModelOption.EntityDataModel)
		{
			base.OtherContent.Add(base.Schema.SchemaSource);
		}
	}

	internal override void ResolveTopLevelNames()
	{
		base.ResolveTopLevelNames();
		if (base.BaseType != null && !(base.BaseType is SchemaComplexType))
		{
			AddError(ErrorCode.InvalidBaseType, EdmSchemaErrorSeverity.Error, Strings.InvalidBaseTypeForNestedType(base.BaseType.FQName, FQName));
		}
	}

	protected override bool HandleElement(XmlReader reader)
	{
		if (base.HandleElement(reader))
		{
			return true;
		}
		if (CanHandleElement(reader, "ValueAnnotation"))
		{
			SkipElement(reader);
			return true;
		}
		if (CanHandleElement(reader, "TypeAnnotation"))
		{
			SkipElement(reader);
			return true;
		}
		return false;
	}
}
