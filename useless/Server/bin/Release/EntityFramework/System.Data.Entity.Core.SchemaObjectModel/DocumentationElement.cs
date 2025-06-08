using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Resources;
using System.Xml;

namespace System.Data.Entity.Core.SchemaObjectModel;

internal sealed class DocumentationElement : SchemaElement
{
	private readonly Documentation _metdataDocumentation = new Documentation();

	public Documentation MetadataDocumentation
	{
		get
		{
			_metdataDocumentation.SetReadOnly();
			return _metdataDocumentation;
		}
	}

	public DocumentationElement(SchemaElement parentElement)
		: base(parentElement)
	{
	}

	protected override bool HandleElement(XmlReader reader)
	{
		if (base.HandleElement(reader))
		{
			return true;
		}
		if (CanHandleElement(reader, "Summary"))
		{
			HandleSummaryElement(reader);
			return true;
		}
		if (CanHandleElement(reader, "LongDescription"))
		{
			HandleLongDescriptionElement(reader);
			return true;
		}
		return false;
	}

	protected override bool HandleText(XmlReader reader)
	{
		if (!string.IsNullOrWhiteSpace(reader.Value))
		{
			AddError(ErrorCode.UnexpectedXmlElement, EdmSchemaErrorSeverity.Error, Strings.InvalidDocumentationBothTextAndStructure);
		}
		return true;
	}

	private void HandleSummaryElement(XmlReader reader)
	{
		TextElement textElement = new TextElement(this);
		textElement.Parse(reader);
		_metdataDocumentation.Summary = textElement.Value;
	}

	private void HandleLongDescriptionElement(XmlReader reader)
	{
		TextElement textElement = new TextElement(this);
		textElement.Parse(reader);
		_metdataDocumentation.LongDescription = textElement.Value;
	}
}
