using System.Xml;

namespace System.Data.Entity.Core.SchemaObjectModel;

internal abstract class Property : SchemaElement
{
	public abstract SchemaType Type { get; }

	internal Property(StructuredType parentElement)
		: base(parentElement)
	{
	}

	protected override bool HandleElement(XmlReader reader)
	{
		if (base.HandleElement(reader))
		{
			return true;
		}
		if (base.Schema.DataModel == SchemaDataModelOption.EntityDataModel)
		{
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
		}
		return false;
	}
}
