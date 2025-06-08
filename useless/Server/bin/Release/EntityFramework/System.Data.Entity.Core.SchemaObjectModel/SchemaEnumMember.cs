using System.Globalization;
using System.Xml;

namespace System.Data.Entity.Core.SchemaObjectModel;

internal class SchemaEnumMember : SchemaElement
{
	private long? _value;

	public long? Value
	{
		get
		{
			return _value;
		}
		set
		{
			_value = value;
		}
	}

	public SchemaEnumMember(SchemaElement parentElement)
		: base(parentElement)
	{
	}

	protected override bool HandleAttribute(XmlReader reader)
	{
		bool flag = base.HandleAttribute(reader);
		if (!flag && (flag = SchemaElement.CanHandleAttribute(reader, "Value")))
		{
			HandleValueAttribute(reader);
		}
		return flag;
	}

	private void HandleValueAttribute(XmlReader reader)
	{
		if (long.TryParse(reader.Value, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out var result))
		{
			_value = result;
		}
	}
}
