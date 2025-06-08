using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Resources;
using System.Xml;

namespace System.Data.Entity.Core.SchemaObjectModel;

internal sealed class FunctionCommandText : SchemaElement
{
	private string _commandText;

	public string CommandText => _commandText;

	public FunctionCommandText(Function parentElement)
		: base(parentElement)
	{
	}

	protected override bool HandleText(XmlReader reader)
	{
		_commandText = reader.Value;
		return true;
	}

	internal override void Validate()
	{
		base.Validate();
		if (string.IsNullOrEmpty(_commandText))
		{
			AddError(ErrorCode.EmptyCommandText, EdmSchemaErrorSeverity.Error, Strings.EmptyCommandText);
		}
	}
}
