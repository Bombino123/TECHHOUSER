using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Resources;
using System.Xml;

namespace System.Data.Entity.Core.SchemaObjectModel;

internal sealed class OnOperation : SchemaElement
{
	public Operation Operation { get; private set; }

	public Action Action { get; private set; }

	private new RelationshipEnd ParentElement => (RelationshipEnd)base.ParentElement;

	public OnOperation(RelationshipEnd parentElement, Operation operation)
		: base(parentElement)
	{
		Operation = operation;
	}

	protected override bool ProhibitAttribute(string namespaceUri, string localName)
	{
		if (base.ProhibitAttribute(namespaceUri, localName))
		{
			return true;
		}
		if (namespaceUri == null)
		{
			_ = localName == "Name";
			return false;
		}
		return false;
	}

	protected override bool HandleAttribute(XmlReader reader)
	{
		if (base.HandleAttribute(reader))
		{
			return true;
		}
		if (SchemaElement.CanHandleAttribute(reader, "Action"))
		{
			HandleActionAttribute(reader);
			return true;
		}
		return false;
	}

	private void HandleActionAttribute(XmlReader reader)
	{
		switch (reader.Value.Trim())
		{
		case "None":
			Action = Action.None;
			break;
		case "Cascade":
			Action = Action.Cascade;
			break;
		default:
			AddError(ErrorCode.InvalidAction, EdmSchemaErrorSeverity.Error, reader, Strings.InvalidAction(reader.Value, ParentElement.FQName));
			break;
		}
	}
}
