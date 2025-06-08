using System.Collections.Generic;
using System.Data.Entity.Core.Common;
using System.Data.Entity.Core.Common.Utils;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Core.Metadata.Edm.Provider;
using System.Data.Entity.Resources;
using System.Diagnostics;
using System.Linq;
using System.Xml;

namespace System.Data.Entity.Core.SchemaObjectModel;

[DebuggerDisplay("DataModel={DataModel}")]
internal class SchemaManager
{
	private readonly HashSet<string> _namespaceLookUpTable = new HashSet<string>(StringComparer.Ordinal);

	private readonly SchemaElementLookUpTable<SchemaType> _schemaTypes = new SchemaElementLookUpTable<SchemaType>();

	private const int MaxErrorCount = 100;

	private DbProviderManifest _providerManifest;

	private PrimitiveSchema _primitiveSchema;

	private double effectiveSchemaVersion;

	private readonly SchemaDataModelOption _dataModel;

	private readonly ProviderManifestNeeded _providerManifestNeeded;

	private readonly AttributeValueNotification _providerNotification;

	private readonly AttributeValueNotification _providerManifestTokenNotification;

	public double SchemaVersion => effectiveSchemaVersion;

	internal SchemaElementLookUpTable<SchemaType> SchemaTypes => _schemaTypes;

	internal SchemaDataModelOption DataModel => _dataModel;

	internal PrimitiveSchema PrimitiveSchema => _primitiveSchema;

	internal AttributeValueNotification ProviderNotification => _providerNotification;

	internal AttributeValueNotification ProviderManifestTokenNotification => _providerManifestTokenNotification;

	private SchemaManager(SchemaDataModelOption dataModel, AttributeValueNotification providerNotification, AttributeValueNotification providerManifestTokenNotification, ProviderManifestNeeded providerManifestNeeded)
	{
		_dataModel = dataModel;
		_providerNotification = providerNotification;
		_providerManifestTokenNotification = providerManifestTokenNotification;
		_providerManifestNeeded = providerManifestNeeded;
	}

	public static IList<EdmSchemaError> LoadProviderManifest(XmlReader xmlReader, string location, bool checkForSystemNamespace, out Schema schema)
	{
		IList<Schema> schemaCollection = new List<Schema>(1);
		DbProviderManifest providerManifest = (checkForSystemNamespace ? EdmProviderManifest.Instance : null);
		IList<EdmSchemaError> result = ParseAndValidate(new XmlReader[1] { xmlReader }, new string[1] { location }, SchemaDataModelOption.ProviderManifestModel, providerManifest, out schemaCollection);
		if (schemaCollection.Count != 0)
		{
			schema = schemaCollection[0];
			return result;
		}
		schema = null;
		return result;
	}

	public static void NoOpAttributeValueNotification(string attributeValue, Action<string, ErrorCode, EdmSchemaErrorSeverity> addError)
	{
	}

	public static IList<EdmSchemaError> ParseAndValidate(IEnumerable<XmlReader> xmlReaders, IEnumerable<string> sourceFilePaths, SchemaDataModelOption dataModel, DbProviderManifest providerManifest, out IList<Schema> schemaCollection)
	{
		return ParseAndValidate(xmlReaders, sourceFilePaths, dataModel, NoOpAttributeValueNotification, NoOpAttributeValueNotification, (Action<string, ErrorCode, EdmSchemaErrorSeverity> error) => providerManifest ?? MetadataItem.EdmProviderManifest, out schemaCollection);
	}

	public static IList<EdmSchemaError> ParseAndValidate(IEnumerable<XmlReader> xmlReaders, IEnumerable<string> sourceFilePaths, SchemaDataModelOption dataModel, AttributeValueNotification providerNotification, AttributeValueNotification providerManifestTokenNotification, ProviderManifestNeeded providerManifestNeeded, out IList<Schema> schemaCollection)
	{
		SchemaManager schemaManager = new SchemaManager(dataModel, providerNotification, providerManifestTokenNotification, providerManifestNeeded);
		List<EdmSchemaError> list = new List<EdmSchemaError>();
		schemaCollection = new List<Schema>();
		bool errorEncountered = false;
		List<string> list2 = ((sourceFilePaths == null) ? new List<string>() : new List<string>(sourceFilePaths));
		int num = 0;
		foreach (XmlReader xmlReader in xmlReaders)
		{
			string location = null;
			if (list2.Count <= num)
			{
				TryGetBaseUri(xmlReader, out location);
			}
			else
			{
				location = list2[num];
			}
			Schema schema = new Schema(schemaManager);
			IList<EdmSchemaError> newErrors = schema.Parse(xmlReader, location);
			CheckIsSameVersion(schema, schemaCollection, list);
			if (UpdateErrorCollectionAndCheckForMaxErrors(list, newErrors, ref errorEncountered))
			{
				return list;
			}
			if (!errorEncountered)
			{
				schemaCollection.Add(schema);
				schemaManager.AddSchema(schema);
			}
			num++;
		}
		if (!errorEncountered)
		{
			foreach (Schema item in schemaCollection)
			{
				if (UpdateErrorCollectionAndCheckForMaxErrors(list, item.Resolve(), ref errorEncountered))
				{
					return list;
				}
			}
			if (!errorEncountered)
			{
				foreach (Schema item2 in schemaCollection)
				{
					if (UpdateErrorCollectionAndCheckForMaxErrors(list, item2.ValidateSchema(), ref errorEncountered))
					{
						return list;
					}
				}
			}
		}
		return list;
	}

	internal static bool TryGetSchemaVersion(XmlReader reader, out double version, out DataSpace dataSpace)
	{
		if (!reader.EOF && reader.NodeType != XmlNodeType.Element)
		{
			while (reader.Read() && reader.NodeType != XmlNodeType.Element)
			{
			}
		}
		if (!reader.EOF && (reader.LocalName == "Schema" || reader.LocalName == "Mapping"))
		{
			return TryGetSchemaVersion(reader.NamespaceURI, out version, out dataSpace);
		}
		version = 0.0;
		dataSpace = DataSpace.OSpace;
		return false;
	}

	internal static bool TryGetSchemaVersion(string xmlNamespaceName, out double version, out DataSpace dataSpace)
	{
		switch (xmlNamespaceName)
		{
		case "http://schemas.microsoft.com/ado/2006/04/edm":
			version = 1.0;
			dataSpace = DataSpace.CSpace;
			return true;
		case "http://schemas.microsoft.com/ado/2007/05/edm":
			version = 1.1;
			dataSpace = DataSpace.CSpace;
			return true;
		case "http://schemas.microsoft.com/ado/2008/09/edm":
			version = 2.0;
			dataSpace = DataSpace.CSpace;
			return true;
		case "http://schemas.microsoft.com/ado/2009/11/edm":
			version = 3.0;
			dataSpace = DataSpace.CSpace;
			return true;
		case "http://schemas.microsoft.com/ado/2006/04/edm/ssdl":
			version = 1.0;
			dataSpace = DataSpace.SSpace;
			return true;
		case "http://schemas.microsoft.com/ado/2009/02/edm/ssdl":
			version = 2.0;
			dataSpace = DataSpace.SSpace;
			return true;
		case "http://schemas.microsoft.com/ado/2009/11/edm/ssdl":
			version = 3.0;
			dataSpace = DataSpace.SSpace;
			return true;
		case "urn:schemas-microsoft-com:windows:storage:mapping:CS":
			version = 1.0;
			dataSpace = DataSpace.CSSpace;
			return true;
		case "http://schemas.microsoft.com/ado/2008/09/mapping/cs":
			version = 2.0;
			dataSpace = DataSpace.CSSpace;
			return true;
		case "http://schemas.microsoft.com/ado/2009/11/mapping/cs":
			version = 3.0;
			dataSpace = DataSpace.CSSpace;
			return true;
		default:
			version = 0.0;
			dataSpace = DataSpace.OSpace;
			return false;
		}
	}

	private static bool CheckIsSameVersion(Schema schemaToBeAdded, IEnumerable<Schema> schemaCollection, List<EdmSchemaError> errorCollection)
	{
		if (schemaToBeAdded.SchemaVersion != 0.0 && schemaCollection.Count() > 0 && schemaCollection.Any((Schema s) => s.SchemaVersion != 0.0 && s.SchemaVersion != schemaToBeAdded.SchemaVersion))
		{
			errorCollection.Add(new EdmSchemaError(Strings.CannotLoadDifferentVersionOfSchemaInTheSameItemCollection, 194, EdmSchemaErrorSeverity.Error));
		}
		return true;
	}

	public void AddSchema(Schema schema)
	{
		if (_namespaceLookUpTable.Count == 0 && schema.DataModel != SchemaDataModelOption.ProviderManifestModel && PrimitiveSchema.Namespace != null)
		{
			_namespaceLookUpTable.Add(PrimitiveSchema.Namespace);
		}
		_namespaceLookUpTable.Add(schema.Namespace);
	}

	public bool TryResolveType(string namespaceName, string typeName, out SchemaType schemaType)
	{
		string key = (string.IsNullOrEmpty(namespaceName) ? typeName : (namespaceName + "." + typeName));
		schemaType = SchemaTypes.LookUpEquivalentKey(key);
		if (schemaType != null)
		{
			return true;
		}
		return false;
	}

	public bool IsValidNamespaceName(string namespaceName)
	{
		return _namespaceLookUpTable.Contains(namespaceName);
	}

	internal static bool TryGetBaseUri(XmlReader xmlReader, out string location)
	{
		string baseURI = xmlReader.BaseURI;
		Uri result = null;
		if (!string.IsNullOrEmpty(baseURI) && Uri.TryCreate(baseURI, UriKind.Absolute, out result) && result.Scheme == "file")
		{
			location = Helper.GetFileNameFromUri(result);
			return true;
		}
		location = null;
		return false;
	}

	private static bool UpdateErrorCollectionAndCheckForMaxErrors(List<EdmSchemaError> errorCollection, IList<EdmSchemaError> newErrors, ref bool errorEncountered)
	{
		if (!errorEncountered && !MetadataHelper.CheckIfAllErrorsAreWarnings(newErrors))
		{
			errorEncountered = true;
		}
		errorCollection.AddRange(newErrors);
		if (errorEncountered && errorCollection.Where((EdmSchemaError e) => e.Severity == EdmSchemaErrorSeverity.Error).Count() > 100)
		{
			return true;
		}
		return false;
	}

	internal DbProviderManifest GetProviderManifest(Action<string, ErrorCode, EdmSchemaErrorSeverity> addError)
	{
		if (_providerManifest == null)
		{
			_providerManifest = _providerManifestNeeded(addError);
		}
		return _providerManifest;
	}

	internal void EnsurePrimitiveSchemaIsLoaded(double forSchemaVersion)
	{
		if (_primitiveSchema == null)
		{
			effectiveSchemaVersion = forSchemaVersion;
			_primitiveSchema = new PrimitiveSchema(this);
		}
	}
}
