using System.Collections.Generic;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Infrastructure.DependencyResolution;
using System.Data.Entity.Resources;
using System.Diagnostics;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;

namespace System.Data.Entity.Core.SchemaObjectModel;

[DebuggerDisplay("Name={Name}")]
internal abstract class SchemaElement
{
	internal const string XmlNamespaceNamespace = "http://www.w3.org/2000/xmlns/";

	private Schema _schema;

	private int _lineNumber;

	private int _linePosition;

	private string _name;

	private List<MetadataProperty> _otherContent;

	private readonly IDbDependencyResolver _resolver;

	protected const int MaxValueVersionComponent = 32767;

	internal int LineNumber => _lineNumber;

	internal int LinePosition => _linePosition;

	public virtual string Name
	{
		get
		{
			return _name;
		}
		set
		{
			_name = value;
		}
	}

	internal DocumentationElement Documentation { get; set; }

	internal SchemaElement ParentElement { get; private set; }

	internal Schema Schema
	{
		get
		{
			return _schema;
		}
		set
		{
			_schema = value;
		}
	}

	public virtual string FQName => Name;

	public virtual string Identity => Name;

	public List<MetadataProperty> OtherContent
	{
		get
		{
			if (_otherContent == null)
			{
				_otherContent = new List<MetadataProperty>();
			}
			return _otherContent;
		}
	}

	protected string SchemaLocation
	{
		get
		{
			if (Schema != null)
			{
				return Schema.Location;
			}
			return null;
		}
	}

	internal virtual void Validate()
	{
	}

	internal void AddError(ErrorCode errorCode, EdmSchemaErrorSeverity severity, int lineNumber, int linePosition, object message)
	{
		AddError(errorCode, severity, SchemaLocation, lineNumber, linePosition, message);
	}

	internal void AddError(ErrorCode errorCode, EdmSchemaErrorSeverity severity, XmlReader reader, object message)
	{
		GetPositionInfo(reader, out var lineNumber, out var linePosition);
		AddError(errorCode, severity, SchemaLocation, lineNumber, linePosition, message);
	}

	internal void AddError(ErrorCode errorCode, EdmSchemaErrorSeverity severity, object message)
	{
		AddError(errorCode, severity, SchemaLocation, LineNumber, LinePosition, message);
	}

	internal void AddError(ErrorCode errorCode, EdmSchemaErrorSeverity severity, SchemaElement element, object message)
	{
		AddError(errorCode, severity, element.Schema.Location, element.LineNumber, element.LinePosition, message);
	}

	internal void Parse(XmlReader reader)
	{
		GetPositionInfo(reader);
		bool flag = !reader.IsEmptyElement;
		bool flag2 = reader.MoveToFirstAttribute();
		while (flag2)
		{
			ParseAttribute(reader);
			flag2 = reader.MoveToNextAttribute();
		}
		HandleAttributesComplete();
		bool flag3 = !flag;
		bool flag4 = false;
		while (!flag3)
		{
			if (flag4)
			{
				flag4 = false;
				reader.Skip();
				if (reader.EOF)
				{
					break;
				}
			}
			else if (!reader.Read())
			{
				break;
			}
			switch (reader.NodeType)
			{
			case XmlNodeType.Element:
				flag4 = ParseElement(reader);
				break;
			case XmlNodeType.EndElement:
				flag3 = true;
				break;
			case XmlNodeType.Text:
			case XmlNodeType.CDATA:
			case XmlNodeType.SignificantWhitespace:
				ParseText(reader);
				break;
			case XmlNodeType.EntityReference:
			case XmlNodeType.DocumentType:
				flag4 = true;
				break;
			default:
				AddError(ErrorCode.UnexpectedXmlNodeType, EdmSchemaErrorSeverity.Error, reader, Strings.UnexpectedXmlNodeType(reader.NodeType));
				flag4 = true;
				break;
			case XmlNodeType.ProcessingInstruction:
			case XmlNodeType.Comment:
			case XmlNodeType.Notation:
			case XmlNodeType.Whitespace:
			case XmlNodeType.XmlDeclaration:
				break;
			}
		}
		HandleChildElementsComplete();
		if (reader.EOF && reader.Depth > 0)
		{
			AddError(ErrorCode.MalformedXml, EdmSchemaErrorSeverity.Error, 0, 0, Strings.MalformedXml(LineNumber, LinePosition));
		}
	}

	internal void GetPositionInfo(XmlReader reader)
	{
		GetPositionInfo(reader, out _lineNumber, out _linePosition);
	}

	internal static void GetPositionInfo(XmlReader reader, out int lineNumber, out int linePosition)
	{
		if (reader is IXmlLineInfo xmlLineInfo && xmlLineInfo.HasLineInfo())
		{
			lineNumber = xmlLineInfo.LineNumber;
			linePosition = xmlLineInfo.LinePosition;
		}
		else
		{
			lineNumber = 0;
			linePosition = 0;
		}
	}

	internal virtual void ResolveTopLevelNames()
	{
	}

	internal virtual void ResolveSecondLevelNames()
	{
	}

	internal SchemaElement(SchemaElement parentElement, IDbDependencyResolver resolver = null)
	{
		_resolver = resolver ?? DbConfiguration.DependencyResolver;
		if (parentElement == null)
		{
			return;
		}
		ParentElement = parentElement;
		for (SchemaElement schemaElement = parentElement; schemaElement != null; schemaElement = schemaElement.ParentElement)
		{
			if (schemaElement is Schema schema)
			{
				Schema = schema;
				break;
			}
		}
		if (Schema == null)
		{
			throw new InvalidOperationException(Strings.AllElementsMustBeInSchema);
		}
	}

	internal SchemaElement(SchemaElement parentElement, string name, IDbDependencyResolver resolver = null)
		: this(parentElement, resolver)
	{
		_name = name;
	}

	protected virtual void HandleAttributesComplete()
	{
	}

	protected virtual void HandleChildElementsComplete()
	{
	}

	protected string HandleUndottedNameAttribute(XmlReader reader, string field)
	{
		string name = field;
		Utils.GetUndottedName(Schema, reader, out name);
		return name;
	}

	protected ReturnValue<string> HandleDottedNameAttribute(XmlReader reader, string field)
	{
		ReturnValue<string> returnValue = new ReturnValue<string>();
		if (!Utils.GetDottedName(Schema, reader, out var name))
		{
			return returnValue;
		}
		returnValue.Value = name;
		return returnValue;
	}

	internal bool HandleIntAttribute(XmlReader reader, ref int field)
	{
		if (!Utils.GetInt(Schema, reader, out var value))
		{
			return false;
		}
		field = value;
		return true;
	}

	internal bool HandleByteAttribute(XmlReader reader, ref byte field)
	{
		if (!Utils.GetByte(Schema, reader, out var value))
		{
			return false;
		}
		field = value;
		return true;
	}

	internal bool HandleBoolAttribute(XmlReader reader, ref bool field)
	{
		if (!Utils.GetBool(Schema, reader, out var value))
		{
			return false;
		}
		field = value;
		return true;
	}

	protected virtual void SkipThroughElement(XmlReader reader)
	{
		Parse(reader);
	}

	protected virtual void SkipElement(XmlReader reader)
	{
		using XmlReader xmlReader = reader.ReadSubtree();
		while (xmlReader.Read())
		{
		}
	}

	protected virtual bool HandleText(XmlReader reader)
	{
		return false;
	}

	internal virtual SchemaElement Clone(SchemaElement parentElement)
	{
		throw Error.NotImplemented();
	}

	private void HandleDocumentationElement(XmlReader reader)
	{
		Documentation = new DocumentationElement(this);
		Documentation.Parse(reader);
	}

	protected virtual void HandleNameAttribute(XmlReader reader)
	{
		Name = HandleUndottedNameAttribute(reader, Name);
	}

	private void AddError(ErrorCode errorCode, EdmSchemaErrorSeverity severity, string sourceLocation, int lineNumber, int linePosition, object message)
	{
		EdmSchemaError edmSchemaError = null;
		edmSchemaError = ((!(message is string message2)) ? ((!(message is Exception ex)) ? new EdmSchemaError(message.ToString(), (int)errorCode, severity, sourceLocation, lineNumber, linePosition) : new EdmSchemaError(ex.Message, (int)errorCode, severity, sourceLocation, lineNumber, linePosition, ex)) : new EdmSchemaError(message2, (int)errorCode, severity, sourceLocation, lineNumber, linePosition));
		Schema.AddError(edmSchemaError);
	}

	private void ParseAttribute(XmlReader reader)
	{
		string namespaceURI = reader.NamespaceURI;
		if (!(namespaceURI == "http://schemas.microsoft.com/ado/2009/02/edm/annotation") || !(reader.LocalName == "UseStrongSpatialTypes") || ProhibitAttribute(namespaceURI, reader.LocalName) || !HandleAttribute(reader))
		{
			if (!Schema.IsParseableXmlNamespace(namespaceURI, isAttribute: true))
			{
				AddOtherContent(reader);
			}
			else if ((ProhibitAttribute(namespaceURI, reader.LocalName) || !HandleAttribute(reader)) && (reader.SchemaInfo == null || reader.SchemaInfo.Validity != XmlSchemaValidity.Invalid) && (string.IsNullOrEmpty(namespaceURI) || Schema.IsParseableXmlNamespace(namespaceURI, isAttribute: true)))
			{
				AddError(ErrorCode.UnexpectedXmlAttribute, EdmSchemaErrorSeverity.Error, reader, Strings.UnexpectedXmlAttribute(reader.Name));
			}
		}
	}

	protected virtual bool ProhibitAttribute(string namespaceUri, string localName)
	{
		return false;
	}

	internal static bool CanHandleAttribute(XmlReader reader, string localName)
	{
		if (reader.NamespaceURI.Length == 0)
		{
			return reader.LocalName == localName;
		}
		return false;
	}

	protected virtual bool HandleAttribute(XmlReader reader)
	{
		if (CanHandleAttribute(reader, "Name"))
		{
			HandleNameAttribute(reader);
			return true;
		}
		return false;
	}

	private bool AddOtherContent(XmlReader reader)
	{
		GetPositionInfo(reader, out var lineNumber, out var linePosition);
		MetadataProperty property;
		if (reader.NodeType == XmlNodeType.Element)
		{
			if (_schema.SchemaVersion == 1.0 || _schema.SchemaVersion == 1.1)
			{
				return true;
			}
			if (_schema.SchemaVersion >= 2.0 && reader.NamespaceURI == "http://schemas.microsoft.com/ado/2006/04/codegeneration")
			{
				AddError(ErrorCode.NoCodeGenNamespaceInStructuralAnnotation, EdmSchemaErrorSeverity.Error, lineNumber, linePosition, Strings.NoCodeGenNamespaceInStructuralAnnotation("http://schemas.microsoft.com/ado/2006/04/codegeneration"));
				return true;
			}
			using XmlReader xmlReader = reader.ReadSubtree();
			xmlReader.Read();
			using StringReader stringReader = new StringReader(xmlReader.ReadOuterXml());
			XElement val = XElement.Load((TextReader)stringReader);
			property = CreateMetadataPropertyFromXmlElement(val.Name.NamespaceName, val.Name.LocalName, val);
		}
		else
		{
			if (reader.NamespaceURI == "http://www.w3.org/2000/xmlns/")
			{
				return true;
			}
			property = CreateMetadataPropertyFromXmlAttribute(reader.NamespaceURI, reader.LocalName, reader.Value);
		}
		if (!OtherContent.Exists((MetadataProperty mp) => mp.Identity == property.Identity))
		{
			OtherContent.Add(property);
		}
		else
		{
			AddError(ErrorCode.AlreadyDefined, EdmSchemaErrorSeverity.Error, lineNumber, linePosition, Strings.DuplicateAnnotation(property.Identity, FQName));
		}
		return false;
	}

	internal static MetadataProperty CreateMetadataPropertyFromXmlElement(string xmlNamespaceUri, string elementName, XElement value)
	{
		return MetadataProperty.CreateAnnotation(xmlNamespaceUri + ":" + elementName, value);
	}

	internal MetadataProperty CreateMetadataPropertyFromXmlAttribute(string xmlNamespaceUri, string attributeName, string value)
	{
		Func<IMetadataAnnotationSerializer> service = _resolver.GetService<Func<IMetadataAnnotationSerializer>>(attributeName);
		object value2 = ((service == null) ? value : service().Deserialize(attributeName, value));
		return MetadataProperty.CreateAnnotation(xmlNamespaceUri + ":" + attributeName, value2);
	}

	private bool ParseElement(XmlReader reader)
	{
		string namespaceURI = reader.NamespaceURI;
		if (!Schema.IsParseableXmlNamespace(namespaceURI, isAttribute: true) && ParentElement != null)
		{
			return AddOtherContent(reader);
		}
		if (HandleElement(reader))
		{
			return false;
		}
		if (string.IsNullOrEmpty(namespaceURI) || Schema.IsParseableXmlNamespace(reader.NamespaceURI, isAttribute: false))
		{
			AddError(ErrorCode.UnexpectedXmlElement, EdmSchemaErrorSeverity.Error, reader, Strings.UnexpectedXmlElement(reader.Name));
		}
		return true;
	}

	protected bool CanHandleElement(XmlReader reader, string localName)
	{
		if (reader.NamespaceURI == Schema.SchemaXmlNamespace)
		{
			return reader.LocalName == localName;
		}
		return false;
	}

	protected virtual bool HandleElement(XmlReader reader)
	{
		if (CanHandleElement(reader, "Documentation"))
		{
			HandleDocumentationElement(reader);
			return true;
		}
		return false;
	}

	private void ParseText(XmlReader reader)
	{
		if (!HandleText(reader) && (reader.Value == null || reader.Value.Trim().Length != 0))
		{
			AddError(ErrorCode.TextNotAllowed, EdmSchemaErrorSeverity.Error, reader, Strings.TextNotAllowed(reader.Value));
		}
	}

	[Conditional("DEBUG")]
	internal static void AssertReaderConsidersSchemaInvalid(XmlReader reader)
	{
	}
}
