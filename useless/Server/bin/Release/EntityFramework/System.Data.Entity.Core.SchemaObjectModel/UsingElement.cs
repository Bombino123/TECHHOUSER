using System.Xml;

namespace System.Data.Entity.Core.SchemaObjectModel;

internal class UsingElement : SchemaElement
{
	public virtual string Alias { get; private set; }

	public virtual string NamespaceName { get; private set; }

	public override string FQName => null;

	internal UsingElement(Schema parentElement)
		: base(parentElement)
	{
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
		if (SchemaElement.CanHandleAttribute(reader, "Namespace"))
		{
			HandleNamespaceAttribute(reader);
			return true;
		}
		if (SchemaElement.CanHandleAttribute(reader, "Alias"))
		{
			HandleAliasAttribute(reader);
			return true;
		}
		return false;
	}

	private void HandleNamespaceAttribute(XmlReader reader)
	{
		ReturnValue<string> returnValue = HandleDottedNameAttribute(reader, NamespaceName);
		if (returnValue.Succeeded)
		{
			NamespaceName = returnValue.Value;
		}
	}

	private void HandleAliasAttribute(XmlReader reader)
	{
		Alias = HandleUndottedNameAttribute(reader, Alias);
	}
}
