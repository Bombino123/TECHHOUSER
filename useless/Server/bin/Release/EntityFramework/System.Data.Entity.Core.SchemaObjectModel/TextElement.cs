using System.Xml;

namespace System.Data.Entity.Core.SchemaObjectModel;

internal sealed class TextElement : SchemaElement
{
	public string Value { get; private set; }

	public TextElement(SchemaElement parentElement)
		: base(parentElement)
	{
	}

	protected override bool HandleText(XmlReader reader)
	{
		TextElementTextHandler(reader);
		return true;
	}

	private void TextElementTextHandler(XmlReader reader)
	{
		string value = reader.Value;
		if (!string.IsNullOrEmpty(value))
		{
			if (string.IsNullOrEmpty(Value))
			{
				Value = value;
			}
			else
			{
				Value += value;
			}
		}
	}
}
