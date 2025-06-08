using System.Collections.Generic;
using System.Data.Entity.Core.Common;
using System.Data.Entity.Core.Common.Utils;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Core.Metadata.Edm.Provider;
using System.Data.Entity.Core.Objects.DataClasses;
using System.Data.Entity.Resources;
using System.Data.Entity.Utilities;
using System.Diagnostics;
using System.IO;
using System.Security;
using System.Xml;
using System.Xml.Schema;

namespace System.Data.Entity.Core.SchemaObjectModel;

[DebuggerDisplay("Namespace={Namespace}, PublicKeyToken={PublicKeyToken}, Version={Version}")]
internal class Schema : SchemaElement
{
	private static class SomSchemaSetHelper
	{
		private static readonly Memoizer<SchemaDataModelOption, XmlSchemaSet> _cachedSchemaSets = new Memoizer<SchemaDataModelOption, XmlSchemaSet>(ComputeSchemaSet, EqualityComparer<SchemaDataModelOption>.Default);

		internal static List<string> GetPrimarySchemaNamespaces(SchemaDataModelOption dataModel)
		{
			List<string> list = new List<string>();
			switch (dataModel)
			{
			case SchemaDataModelOption.EntityDataModel:
				list.Add("http://schemas.microsoft.com/ado/2006/04/edm");
				list.Add("http://schemas.microsoft.com/ado/2007/05/edm");
				list.Add("http://schemas.microsoft.com/ado/2008/09/edm");
				list.Add("http://schemas.microsoft.com/ado/2009/11/edm");
				break;
			case SchemaDataModelOption.ProviderDataModel:
				list.Add("http://schemas.microsoft.com/ado/2006/04/edm/ssdl");
				list.Add("http://schemas.microsoft.com/ado/2009/02/edm/ssdl");
				list.Add("http://schemas.microsoft.com/ado/2009/11/edm/ssdl");
				break;
			default:
				list.Add("http://schemas.microsoft.com/ado/2006/04/edm/providermanifest");
				break;
			}
			return list;
		}

		internal static XmlSchemaSet GetSchemaSet(SchemaDataModelOption dataModel)
		{
			return _cachedSchemaSets.Evaluate(dataModel);
		}

		private static XmlSchemaSet ComputeSchemaSet(SchemaDataModelOption dataModel)
		{
			List<string> primarySchemaNamespaces = GetPrimarySchemaNamespaces(dataModel);
			XmlSchemaSet xmlSchemaSet = new XmlSchemaSet
			{
				XmlResolver = null
			};
			Dictionary<string, XmlSchemaResource> metadataSchemaResourceMap = XmlSchemaResource.GetMetadataSchemaResourceMap(3.0);
			HashSet<string> schemasAlreadyAdded = new HashSet<string>();
			foreach (string item in primarySchemaNamespaces)
			{
				XmlSchemaResource schemaResource = metadataSchemaResourceMap[item];
				AddXmlSchemaToSet(xmlSchemaSet, schemaResource, schemasAlreadyAdded);
			}
			xmlSchemaSet.Compile();
			return xmlSchemaSet;
		}

		private static void AddXmlSchemaToSet(XmlSchemaSet schemaSet, XmlSchemaResource schemaResource, HashSet<string> schemasAlreadyAdded)
		{
			XmlSchemaResource[] importedSchemas = schemaResource.ImportedSchemas;
			foreach (XmlSchemaResource schemaResource2 in importedSchemas)
			{
				AddXmlSchemaToSet(schemaSet, schemaResource2, schemasAlreadyAdded);
			}
			if (!schemasAlreadyAdded.Contains(schemaResource.NamespaceUri))
			{
				XmlSchema schema = XmlSchema.Read(GetResourceStream(schemaResource.ResourceName), null);
				schemaSet.Add(schema);
				schemasAlreadyAdded.Add(schemaResource.NamespaceUri);
			}
		}

		private static Stream GetResourceStream(string resourceName)
		{
			return typeof(Schema).Assembly().GetManifestResourceStream(resourceName);
		}
	}

	private const int RootDepth = 2;

	private List<EdmSchemaError> _errors = new List<EdmSchemaError>();

	private List<Function> _functions;

	private AliasResolver _aliasResolver;

	private string _location;

	protected string _namespaceName;

	private List<SchemaType> _schemaTypes;

	private int _depth;

	private double _schemaVersion;

	private readonly SchemaManager _schemaManager;

	private bool? _useStrongSpatialTypes;

	private HashSet<string> _validatableXmlNamespaces;

	private HashSet<string> _parseableXmlNamespaces;

	private MetadataProperty _schemaSourceProperty;

	internal string SchemaXmlNamespace { get; private set; }

	internal DbProviderManifest ProviderManifest => _schemaManager.GetProviderManifest(delegate(string message, ErrorCode code, EdmSchemaErrorSeverity severity)
	{
		AddError(code, severity, message);
	});

	internal double SchemaVersion
	{
		get
		{
			return _schemaVersion;
		}
		set
		{
			_schemaVersion = value;
		}
	}

	internal virtual string Alias { get; private set; }

	internal virtual string Namespace
	{
		get
		{
			return _namespaceName;
		}
		private set
		{
			_namespaceName = value;
		}
	}

	internal string Location
	{
		get
		{
			return _location;
		}
		private set
		{
			_location = value;
		}
	}

	internal MetadataProperty SchemaSource
	{
		get
		{
			if (_schemaSourceProperty == null)
			{
				_schemaSourceProperty = new MetadataProperty("SchemaSource", EdmProviderManifest.Instance.GetPrimitiveType(PrimitiveTypeKind.String), isCollectionType: false, (_location != null) ? _location : string.Empty);
			}
			return _schemaSourceProperty;
		}
	}

	internal List<SchemaType> SchemaTypes
	{
		get
		{
			if (_schemaTypes == null)
			{
				_schemaTypes = new List<SchemaType>();
			}
			return _schemaTypes;
		}
	}

	public override string FQName => Namespace;

	private List<Function> Functions
	{
		get
		{
			if (_functions == null)
			{
				_functions = new List<Function>();
			}
			return _functions;
		}
	}

	internal AliasResolver AliasResolver
	{
		get
		{
			if (_aliasResolver == null)
			{
				_aliasResolver = new AliasResolver(this);
			}
			return _aliasResolver;
		}
	}

	internal SchemaDataModelOption DataModel => SchemaManager.DataModel;

	internal SchemaManager SchemaManager => _schemaManager;

	internal bool UseStrongSpatialTypes => _useStrongSpatialTypes ?? true;

	public Schema(SchemaManager schemaManager)
		: base(null)
	{
		_schemaManager = schemaManager;
		_errors = new List<EdmSchemaError>();
	}

	internal IList<EdmSchemaError> Resolve()
	{
		ResolveTopLevelNames();
		if (_errors.Count != 0)
		{
			return ResetErrors();
		}
		ResolveSecondLevelNames();
		return ResetErrors();
	}

	internal IList<EdmSchemaError> ValidateSchema()
	{
		Validate();
		return ResetErrors();
	}

	internal void AddError(EdmSchemaError error)
	{
		_errors.Add(error);
	}

	internal IList<EdmSchemaError> Parse(XmlReader sourceReader, string sourceLocation)
	{
		try
		{
			XmlReaderSettings settings = CreateXmlReaderSettings();
			XmlReader sourceReader2 = XmlReader.Create(sourceReader, settings);
			return InternalParse(sourceReader2, sourceLocation);
		}
		catch (IOException message)
		{
			AddError(ErrorCode.IOException, EdmSchemaErrorSeverity.Error, sourceReader, message);
		}
		return ResetErrors();
	}

	private IList<EdmSchemaError> InternalParse(XmlReader sourceReader, string sourceLocation)
	{
		base.Schema = this;
		Location = sourceLocation;
		try
		{
			if (sourceReader.NodeType != XmlNodeType.Element)
			{
				while (sourceReader.Read() && sourceReader.NodeType != XmlNodeType.Element)
				{
				}
			}
			GetPositionInfo(sourceReader);
			List<string> primarySchemaNamespaces = SomSchemaSetHelper.GetPrimarySchemaNamespaces(DataModel);
			if (sourceReader.EOF)
			{
				if (sourceLocation != null)
				{
					AddError(ErrorCode.EmptyFile, EdmSchemaErrorSeverity.Error, Strings.EmptyFile(sourceLocation));
				}
				else
				{
					AddError(ErrorCode.EmptyFile, EdmSchemaErrorSeverity.Error, Strings.EmptySchemaTextReader);
				}
			}
			else if (!primarySchemaNamespaces.Contains(sourceReader.NamespaceURI))
			{
				Func<object, object, object, string> func = Strings.UnexpectedRootElement;
				if (string.IsNullOrEmpty(sourceReader.NamespaceURI))
				{
					func = Strings.UnexpectedRootElementNoNamespace;
				}
				string commaDelimitedString = Helper.GetCommaDelimitedString(primarySchemaNamespaces);
				AddError(ErrorCode.UnexpectedXmlElement, EdmSchemaErrorSeverity.Error, func(sourceReader.NamespaceURI, sourceReader.LocalName, commaDelimitedString));
			}
			else
			{
				SchemaXmlNamespace = sourceReader.NamespaceURI;
				if (DataModel == SchemaDataModelOption.EntityDataModel)
				{
					if (SchemaXmlNamespace == "http://schemas.microsoft.com/ado/2006/04/edm")
					{
						SchemaVersion = 1.0;
					}
					else if (SchemaXmlNamespace == "http://schemas.microsoft.com/ado/2007/05/edm")
					{
						SchemaVersion = 1.1;
					}
					else if (SchemaXmlNamespace == "http://schemas.microsoft.com/ado/2008/09/edm")
					{
						SchemaVersion = 2.0;
					}
					else
					{
						SchemaVersion = 3.0;
					}
				}
				else if (DataModel == SchemaDataModelOption.ProviderDataModel)
				{
					if (SchemaXmlNamespace == "http://schemas.microsoft.com/ado/2006/04/edm/ssdl")
					{
						SchemaVersion = 1.0;
					}
					else if (SchemaXmlNamespace == "http://schemas.microsoft.com/ado/2009/02/edm/ssdl")
					{
						SchemaVersion = 2.0;
					}
					else
					{
						SchemaVersion = 3.0;
					}
				}
				switch (sourceReader.LocalName)
				{
				case "Schema":
				case "ProviderManifest":
					HandleTopLevelSchemaElement(sourceReader);
					sourceReader.Read();
					break;
				default:
					AddError(ErrorCode.UnexpectedXmlElement, EdmSchemaErrorSeverity.Error, Strings.UnexpectedRootElement(sourceReader.NamespaceURI, sourceReader.LocalName, SchemaXmlNamespace));
					break;
				}
			}
		}
		catch (InvalidOperationException ex)
		{
			AddError(ErrorCode.InternalError, EdmSchemaErrorSeverity.Error, ex.Message);
		}
		catch (UnauthorizedAccessException message)
		{
			AddError(ErrorCode.UnauthorizedAccessException, EdmSchemaErrorSeverity.Error, sourceReader, message);
		}
		catch (IOException message2)
		{
			AddError(ErrorCode.IOException, EdmSchemaErrorSeverity.Error, sourceReader, message2);
		}
		catch (SecurityException message3)
		{
			AddError(ErrorCode.SecurityError, EdmSchemaErrorSeverity.Error, sourceReader, message3);
		}
		catch (XmlException message4)
		{
			AddError(ErrorCode.XmlError, EdmSchemaErrorSeverity.Error, sourceReader, message4);
		}
		return ResetErrors();
	}

	internal static XmlReaderSettings CreateEdmStandardXmlReaderSettings()
	{
		XmlReaderSettings obj = new XmlReaderSettings
		{
			CheckCharacters = true,
			CloseInput = false,
			IgnoreWhitespace = true,
			ConformanceLevel = ConformanceLevel.Auto,
			IgnoreComments = true,
			IgnoreProcessingInstructions = true,
			DtdProcessing = DtdProcessing.Prohibit
		};
		obj.ValidationFlags &= ~XmlSchemaValidationFlags.ProcessIdentityConstraints;
		obj.ValidationFlags &= ~XmlSchemaValidationFlags.ProcessSchemaLocation;
		obj.ValidationFlags &= ~XmlSchemaValidationFlags.ProcessInlineSchema;
		return obj;
	}

	private XmlReaderSettings CreateXmlReaderSettings()
	{
		XmlReaderSettings xmlReaderSettings = CreateEdmStandardXmlReaderSettings();
		xmlReaderSettings.ValidationFlags |= XmlSchemaValidationFlags.ReportValidationWarnings;
		xmlReaderSettings.ValidationEventHandler += OnSchemaValidationEvent;
		xmlReaderSettings.ValidationType = ValidationType.Schema;
		XmlSchemaSet schemaSet = SomSchemaSetHelper.GetSchemaSet(DataModel);
		xmlReaderSettings.Schemas = schemaSet;
		return xmlReaderSettings;
	}

	internal void OnSchemaValidationEvent(object sender, ValidationEventArgs e)
	{
		XmlReader xmlReader = sender as XmlReader;
		if ((xmlReader == null || IsValidateableXmlNamespace(xmlReader.NamespaceURI, xmlReader.NodeType == XmlNodeType.Attribute) || (SchemaVersion != 1.0 && SchemaVersion != 1.1 && xmlReader.NodeType != XmlNodeType.Attribute && e.Severity != XmlSeverityType.Warning)) && (!(SchemaVersion >= 2.0) || xmlReader.NodeType != XmlNodeType.Attribute || e.Severity != XmlSeverityType.Warning))
		{
			EdmSchemaErrorSeverity severity = EdmSchemaErrorSeverity.Error;
			if (e.Severity == XmlSeverityType.Warning)
			{
				severity = EdmSchemaErrorSeverity.Warning;
			}
			AddError(ErrorCode.XmlError, severity, e.Exception.LineNumber, e.Exception.LinePosition, e.Message);
		}
	}

	public bool IsParseableXmlNamespace(string xmlNamespaceUri, bool isAttribute)
	{
		if (string.IsNullOrEmpty(xmlNamespaceUri) && isAttribute)
		{
			return true;
		}
		if (_parseableXmlNamespaces == null)
		{
			_parseableXmlNamespaces = new HashSet<string>();
			foreach (XmlSchemaResource value in XmlSchemaResource.GetMetadataSchemaResourceMap(SchemaVersion).Values)
			{
				_parseableXmlNamespaces.Add(value.NamespaceUri);
			}
		}
		return _parseableXmlNamespaces.Contains(xmlNamespaceUri);
	}

	public bool IsValidateableXmlNamespace(string xmlNamespaceUri, bool isAttribute)
	{
		if (string.IsNullOrEmpty(xmlNamespaceUri) && isAttribute)
		{
			return true;
		}
		if (_validatableXmlNamespaces == null)
		{
			HashSet<string> hashSet = new HashSet<string>();
			foreach (XmlSchemaResource value in XmlSchemaResource.GetMetadataSchemaResourceMap((SchemaVersion == 0.0) ? 3.0 : SchemaVersion).Values)
			{
				AddAllSchemaResourceNamespaceNames(hashSet, value);
			}
			if (SchemaVersion == 0.0)
			{
				return hashSet.Contains(xmlNamespaceUri);
			}
			_validatableXmlNamespaces = hashSet;
		}
		return _validatableXmlNamespaces.Contains(xmlNamespaceUri);
	}

	private static void AddAllSchemaResourceNamespaceNames(HashSet<string> hashSet, XmlSchemaResource schemaResource)
	{
		hashSet.Add(schemaResource.NamespaceUri);
		XmlSchemaResource[] importedSchemas = schemaResource.ImportedSchemas;
		foreach (XmlSchemaResource schemaResource2 in importedSchemas)
		{
			AddAllSchemaResourceNamespaceNames(hashSet, schemaResource2);
		}
	}

	internal override void ResolveTopLevelNames()
	{
		base.ResolveTopLevelNames();
		AliasResolver.ResolveNamespaces();
		foreach (SchemaType schemaType in SchemaTypes)
		{
			schemaType.ResolveTopLevelNames();
		}
		foreach (Function function in Functions)
		{
			function.ResolveTopLevelNames();
		}
	}

	internal override void ResolveSecondLevelNames()
	{
		base.ResolveSecondLevelNames();
		foreach (SchemaType schemaType in SchemaTypes)
		{
			schemaType.ResolveSecondLevelNames();
		}
		foreach (Function function in Functions)
		{
			function.ResolveSecondLevelNames();
		}
	}

	internal override void Validate()
	{
		if (string.IsNullOrEmpty(Namespace))
		{
			AddError(ErrorCode.MissingNamespaceAttribute, EdmSchemaErrorSeverity.Error, Strings.MissingNamespaceAttribute);
			return;
		}
		if (!string.IsNullOrEmpty(Alias) && EdmItemCollection.IsSystemNamespace(ProviderManifest, Alias))
		{
			AddError(ErrorCode.CannotUseSystemNamespaceAsAlias, EdmSchemaErrorSeverity.Error, Strings.CannotUseSystemNamespaceAsAlias(Alias));
		}
		if (ProviderManifest != null && EdmItemCollection.IsSystemNamespace(ProviderManifest, Namespace))
		{
			AddError(ErrorCode.SystemNamespace, EdmSchemaErrorSeverity.Error, Strings.SystemNamespaceEncountered(Namespace));
		}
		foreach (SchemaType schemaType in SchemaTypes)
		{
			schemaType.Validate();
		}
		foreach (Function function in Functions)
		{
			AddFunctionType(function);
			function.Validate();
		}
	}

	protected override bool HandleElement(XmlReader reader)
	{
		if (base.HandleElement(reader))
		{
			return true;
		}
		if (CanHandleElement(reader, "EntityType"))
		{
			HandleEntityTypeElement(reader);
			return true;
		}
		if (CanHandleElement(reader, "ComplexType"))
		{
			HandleInlineTypeElement(reader);
			return true;
		}
		if (CanHandleElement(reader, "Association"))
		{
			HandleAssociationElement(reader);
			return true;
		}
		if (DataModel == SchemaDataModelOption.EntityDataModel)
		{
			if (CanHandleElement(reader, "Using"))
			{
				HandleUsingElement(reader);
				return true;
			}
			if (CanHandleElement(reader, "Function"))
			{
				HandleModelFunctionElement(reader);
				return true;
			}
			if (CanHandleElement(reader, "EnumType"))
			{
				HandleEnumTypeElement(reader);
				return true;
			}
			if (CanHandleElement(reader, "ValueTerm"))
			{
				SkipElement(reader);
				return true;
			}
			if (CanHandleElement(reader, "Annotations"))
			{
				SkipElement(reader);
				return true;
			}
		}
		if (DataModel == SchemaDataModelOption.EntityDataModel || DataModel == SchemaDataModelOption.ProviderDataModel)
		{
			if (CanHandleElement(reader, "EntityContainer"))
			{
				HandleEntityContainerTypeElement(reader);
				return true;
			}
			if (DataModel == SchemaDataModelOption.ProviderDataModel && CanHandleElement(reader, "Function"))
			{
				HandleFunctionElement(reader);
				return true;
			}
		}
		else
		{
			if (CanHandleElement(reader, "Types"))
			{
				SkipThroughElement(reader);
				return true;
			}
			if (CanHandleElement(reader, "Functions"))
			{
				SkipThroughElement(reader);
				return true;
			}
			if (CanHandleElement(reader, "Function"))
			{
				HandleFunctionElement(reader);
				return true;
			}
			if (CanHandleElement(reader, "Type"))
			{
				HandleTypeInformationElement(reader);
				return true;
			}
		}
		return false;
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
		if (_depth == 1)
		{
			return false;
		}
		if (base.HandleAttribute(reader))
		{
			return true;
		}
		if (SchemaElement.CanHandleAttribute(reader, "Alias"))
		{
			HandleAliasAttribute(reader);
			return true;
		}
		if (SchemaElement.CanHandleAttribute(reader, "Namespace"))
		{
			HandleNamespaceAttribute(reader);
			return true;
		}
		if (SchemaElement.CanHandleAttribute(reader, "Provider"))
		{
			HandleProviderAttribute(reader);
			return true;
		}
		if (SchemaElement.CanHandleAttribute(reader, "ProviderManifestToken"))
		{
			HandleProviderManifestTokenAttribute(reader);
			return true;
		}
		if (reader.NamespaceURI == "http://schemas.microsoft.com/ado/2009/02/edm/annotation" && reader.LocalName == "UseStrongSpatialTypes")
		{
			HandleUseStrongSpatialTypesAnnotation(reader);
			return true;
		}
		return false;
	}

	protected override void HandleAttributesComplete()
	{
		if (_depth >= 2)
		{
			if (_depth == 2)
			{
				_schemaManager.EnsurePrimitiveSchemaIsLoaded(SchemaVersion);
			}
			base.HandleAttributesComplete();
		}
	}

	protected override void SkipThroughElement(XmlReader reader)
	{
		try
		{
			_depth++;
			base.SkipThroughElement(reader);
		}
		finally
		{
			_depth--;
		}
	}

	internal bool ResolveTypeName(SchemaElement usingElement, string typeName, out SchemaType type)
	{
		type = null;
		Utils.ExtractNamespaceAndName(typeName, out var namespaceName, out var name);
		string text = namespaceName;
		if (text == null)
		{
			text = ((ProviderManifest == null) ? _namespaceName : ProviderManifest.NamespaceName);
		}
		if (namespaceName == null || !AliasResolver.TryResolveAlias(text, out var namespaceName2))
		{
			namespaceName2 = text;
		}
		if (!SchemaManager.TryResolveType(namespaceName2, name, out type))
		{
			if (namespaceName == null)
			{
				usingElement.AddError(ErrorCode.NotInNamespace, EdmSchemaErrorSeverity.Error, Strings.NotNamespaceQualified(typeName));
			}
			else if (!SchemaManager.IsValidNamespaceName(namespaceName2))
			{
				usingElement.AddError(ErrorCode.BadNamespace, EdmSchemaErrorSeverity.Error, Strings.BadNamespaceOrAlias(namespaceName));
			}
			else if (namespaceName2 != text)
			{
				usingElement.AddError(ErrorCode.NotInNamespace, EdmSchemaErrorSeverity.Error, Strings.NotInNamespaceAlias(name, namespaceName2, text));
			}
			else
			{
				usingElement.AddError(ErrorCode.NotInNamespace, EdmSchemaErrorSeverity.Error, Strings.NotInNamespaceNoAlias(name, namespaceName2));
			}
			return false;
		}
		if (DataModel != 0 && type.Schema != this && type.Schema != SchemaManager.PrimitiveSchema)
		{
			usingElement.AddError(ErrorCode.InvalidNamespaceOrAliasSpecified, EdmSchemaErrorSeverity.Error, Strings.InvalidNamespaceOrAliasSpecified(namespaceName));
			return false;
		}
		return true;
	}

	private void HandleNamespaceAttribute(XmlReader reader)
	{
		ReturnValue<string> returnValue = HandleDottedNameAttribute(reader, Namespace);
		if (returnValue.Succeeded)
		{
			Namespace = returnValue.Value;
		}
	}

	private void HandleAliasAttribute(XmlReader reader)
	{
		Alias = HandleUndottedNameAttribute(reader, Alias);
	}

	private void HandleProviderAttribute(XmlReader reader)
	{
		string value = reader.Value;
		_schemaManager.ProviderNotification(value, delegate(string message, ErrorCode code, EdmSchemaErrorSeverity severity)
		{
			AddError(code, severity, reader, message);
		});
	}

	private void HandleProviderManifestTokenAttribute(XmlReader reader)
	{
		string value = reader.Value;
		_schemaManager.ProviderManifestTokenNotification(value, delegate(string message, ErrorCode code, EdmSchemaErrorSeverity severity)
		{
			AddError(code, severity, reader, message);
		});
	}

	private void HandleUseStrongSpatialTypesAnnotation(XmlReader reader)
	{
		bool field = false;
		if (HandleBoolAttribute(reader, ref field))
		{
			_useStrongSpatialTypes = field;
		}
	}

	private void HandleUsingElement(XmlReader reader)
	{
		UsingElement usingElement = new UsingElement(this);
		usingElement.Parse(reader);
		AliasResolver.Add(usingElement);
	}

	private void HandleEnumTypeElement(XmlReader reader)
	{
		SchemaEnumType schemaEnumType = new SchemaEnumType(this);
		schemaEnumType.Parse(reader);
		TryAddType(schemaEnumType, doNotAddErrorForEmptyName: true);
	}

	private void HandleTopLevelSchemaElement(XmlReader reader)
	{
		try
		{
			_depth += 2;
			Parse(reader);
		}
		finally
		{
			_depth -= 2;
		}
	}

	private void HandleEntityTypeElement(XmlReader reader)
	{
		SchemaEntityType schemaEntityType = new SchemaEntityType(this);
		schemaEntityType.Parse(reader);
		TryAddType(schemaEntityType, doNotAddErrorForEmptyName: true);
	}

	private void HandleTypeInformationElement(XmlReader reader)
	{
		TypeElement typeElement = new TypeElement(this);
		typeElement.Parse(reader);
		TryAddType(typeElement, doNotAddErrorForEmptyName: true);
	}

	private void HandleFunctionElement(XmlReader reader)
	{
		Function function = new Function(this);
		function.Parse(reader);
		Functions.Add(function);
	}

	private void HandleModelFunctionElement(XmlReader reader)
	{
		ModelFunction modelFunction = new ModelFunction(this);
		modelFunction.Parse(reader);
		Functions.Add(modelFunction);
	}

	private void HandleAssociationElement(XmlReader reader)
	{
		Relationship relationship = new Relationship(this, RelationshipKind.Association);
		relationship.Parse(reader);
		TryAddType(relationship, doNotAddErrorForEmptyName: true);
	}

	private void HandleInlineTypeElement(XmlReader reader)
	{
		SchemaComplexType schemaComplexType = new SchemaComplexType(this);
		schemaComplexType.Parse(reader);
		TryAddType(schemaComplexType, doNotAddErrorForEmptyName: true);
	}

	private void HandleEntityContainerTypeElement(XmlReader reader)
	{
		EntityContainer entityContainer = new EntityContainer(this);
		entityContainer.Parse(reader);
		TryAddContainer(entityContainer, doNotAddErrorForEmptyName: true);
	}

	private List<EdmSchemaError> ResetErrors()
	{
		List<EdmSchemaError> errors = _errors;
		_errors = new List<EdmSchemaError>();
		return errors;
	}

	protected void TryAddType(SchemaType schemaType, bool doNotAddErrorForEmptyName)
	{
		SchemaManager.SchemaTypes.Add(schemaType, doNotAddErrorForEmptyName, Strings.TypeNameAlreadyDefinedDuplicate);
		SchemaTypes.Add(schemaType);
	}

	protected void TryAddContainer(SchemaType schemaType, bool doNotAddErrorForEmptyName)
	{
		SchemaManager.SchemaTypes.Add(schemaType, doNotAddErrorForEmptyName, Strings.EntityContainerAlreadyExists);
		SchemaTypes.Add(schemaType);
	}

	protected void AddFunctionType(Function function)
	{
		string p = ((DataModel == SchemaDataModelOption.EntityDataModel) ? "Conceptual" : "Storage");
		if (SchemaVersion >= 2.0 && SchemaManager.SchemaTypes.ContainsKey(function.FQName))
		{
			function.AddError(ErrorCode.AlreadyDefined, EdmSchemaErrorSeverity.Error, Strings.AmbiguousFunctionAndType(function.FQName, p));
		}
		else if (SchemaManager.SchemaTypes.TryAdd(function) != 0)
		{
			function.AddError(ErrorCode.AlreadyDefined, EdmSchemaErrorSeverity.Error, Strings.AmbiguousFunctionOverload(function.FQName, p));
		}
		else
		{
			SchemaTypes.Add(function);
		}
	}
}
