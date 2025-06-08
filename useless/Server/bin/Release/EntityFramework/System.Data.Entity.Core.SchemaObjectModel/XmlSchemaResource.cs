using System.Collections.Generic;

namespace System.Data.Entity.Core.SchemaObjectModel;

internal struct XmlSchemaResource
{
	private static readonly XmlSchemaResource[] _emptyImportList = new XmlSchemaResource[0];

	internal string NamespaceUri;

	internal string ResourceName;

	internal XmlSchemaResource[] ImportedSchemas;

	public XmlSchemaResource(string namespaceUri, string resourceName, XmlSchemaResource[] importedSchemas)
	{
		NamespaceUri = namespaceUri;
		ResourceName = resourceName;
		ImportedSchemas = importedSchemas;
	}

	public XmlSchemaResource(string namespaceUri, string resourceName)
	{
		NamespaceUri = namespaceUri;
		ResourceName = resourceName;
		ImportedSchemas = _emptyImportList;
	}

	internal static Dictionary<string, XmlSchemaResource> GetMetadataSchemaResourceMap(double schemaVersion)
	{
		Dictionary<string, XmlSchemaResource> dictionary = new Dictionary<string, XmlSchemaResource>(StringComparer.Ordinal);
		AddEdmSchemaResourceMapEntries(dictionary, schemaVersion);
		AddStoreSchemaResourceMapEntries(dictionary, schemaVersion);
		return dictionary;
	}

	internal static void AddStoreSchemaResourceMapEntries(Dictionary<string, XmlSchemaResource> schemaResourceMap, double schemaVersion)
	{
		XmlSchemaResource[] importedSchemas = new XmlSchemaResource[1]
		{
			new XmlSchemaResource("http://schemas.microsoft.com/ado/2007/12/edm/EntityStoreSchemaGenerator", "System.Data.Resources.EntityStoreSchemaGenerator.xsd")
		};
		XmlSchemaResource value = new XmlSchemaResource("http://schemas.microsoft.com/ado/2006/04/edm/ssdl", "System.Data.Resources.SSDLSchema.xsd", importedSchemas);
		schemaResourceMap.Add(value.NamespaceUri, value);
		if (schemaVersion >= 2.0)
		{
			XmlSchemaResource value2 = new XmlSchemaResource("http://schemas.microsoft.com/ado/2009/02/edm/ssdl", "System.Data.Resources.SSDLSchema_2.xsd", importedSchemas);
			schemaResourceMap.Add(value2.NamespaceUri, value2);
		}
		if (schemaVersion >= 3.0)
		{
			XmlSchemaResource value3 = new XmlSchemaResource("http://schemas.microsoft.com/ado/2009/11/edm/ssdl", "System.Data.Resources.SSDLSchema_3.xsd", importedSchemas);
			schemaResourceMap.Add(value3.NamespaceUri, value3);
		}
		XmlSchemaResource value4 = new XmlSchemaResource("http://schemas.microsoft.com/ado/2006/04/edm/providermanifest", "System.Data.Resources.ProviderServices.ProviderManifest.xsd");
		schemaResourceMap.Add(value4.NamespaceUri, value4);
	}

	internal static void AddMappingSchemaResourceMapEntries(Dictionary<string, XmlSchemaResource> schemaResourceMap, double schemaVersion)
	{
		XmlSchemaResource value = new XmlSchemaResource("urn:schemas-microsoft-com:windows:storage:mapping:CS", "System.Data.Resources.CSMSL_1.xsd");
		schemaResourceMap.Add(value.NamespaceUri, value);
		if (schemaVersion >= 2.0)
		{
			XmlSchemaResource value2 = new XmlSchemaResource("http://schemas.microsoft.com/ado/2008/09/mapping/cs", "System.Data.Resources.CSMSL_2.xsd");
			schemaResourceMap.Add(value2.NamespaceUri, value2);
		}
		if (schemaVersion >= 3.0)
		{
			XmlSchemaResource value3 = new XmlSchemaResource("http://schemas.microsoft.com/ado/2009/11/mapping/cs", "System.Data.Resources.CSMSL_3.xsd");
			schemaResourceMap.Add(value3.NamespaceUri, value3);
		}
	}

	internal static void AddEdmSchemaResourceMapEntries(Dictionary<string, XmlSchemaResource> schemaResourceMap, double schemaVersion)
	{
		XmlSchemaResource[] importedSchemas = new XmlSchemaResource[1]
		{
			new XmlSchemaResource("http://schemas.microsoft.com/ado/2006/04/codegeneration", "System.Data.Resources.CodeGenerationSchema.xsd")
		};
		XmlSchemaResource[] importedSchemas2 = new XmlSchemaResource[2]
		{
			new XmlSchemaResource("http://schemas.microsoft.com/ado/2006/04/codegeneration", "System.Data.Resources.CodeGenerationSchema.xsd"),
			new XmlSchemaResource("http://schemas.microsoft.com/ado/2009/02/edm/annotation", "System.Data.Resources.AnnotationSchema.xsd")
		};
		XmlSchemaResource[] importedSchemas3 = new XmlSchemaResource[2]
		{
			new XmlSchemaResource("http://schemas.microsoft.com/ado/2006/04/codegeneration", "System.Data.Resources.CodeGenerationSchema.xsd"),
			new XmlSchemaResource("http://schemas.microsoft.com/ado/2009/02/edm/annotation", "System.Data.Resources.AnnotationSchema.xsd")
		};
		XmlSchemaResource value = new XmlSchemaResource("http://schemas.microsoft.com/ado/2006/04/edm", "System.Data.Resources.CSDLSchema_1.xsd", importedSchemas);
		schemaResourceMap.Add(value.NamespaceUri, value);
		XmlSchemaResource value2 = new XmlSchemaResource("http://schemas.microsoft.com/ado/2007/05/edm", "System.Data.Resources.CSDLSchema_1_1.xsd", importedSchemas);
		schemaResourceMap.Add(value2.NamespaceUri, value2);
		if (schemaVersion >= 2.0)
		{
			XmlSchemaResource value3 = new XmlSchemaResource("http://schemas.microsoft.com/ado/2008/09/edm", "System.Data.Resources.CSDLSchema_2.xsd", importedSchemas2);
			schemaResourceMap.Add(value3.NamespaceUri, value3);
		}
		if (schemaVersion >= 3.0)
		{
			XmlSchemaResource value4 = new XmlSchemaResource("http://schemas.microsoft.com/ado/2009/11/edm", "System.Data.Resources.CSDLSchema_3.xsd", importedSchemas3);
			schemaResourceMap.Add(value4.NamespaceUri, value4);
		}
	}
}
