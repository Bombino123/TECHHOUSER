using System.Data.Entity.Core.Metadata.Edm;
using System.Globalization;
using System.Xml;

namespace System.Data.Entity.ModelConfiguration.Edm.Serialization;

internal sealed class EdmxSerializer
{
	private class EndElement : IDisposable
	{
		private readonly XmlWriter _xmlWriter;

		public EndElement(XmlWriter xmlWriter)
		{
			_xmlWriter = xmlWriter;
		}

		public void Dispose()
		{
			_xmlWriter.WriteEndElement();
		}
	}

	private const string EdmXmlNamespaceV1 = "http://schemas.microsoft.com/ado/2007/06/edmx";

	private const string EdmXmlNamespaceV2 = "http://schemas.microsoft.com/ado/2008/10/edmx";

	private const string EdmXmlNamespaceV3 = "http://schemas.microsoft.com/ado/2009/11/edmx";

	private DbDatabaseMapping _databaseMapping;

	private double _version;

	private XmlWriter _xmlWriter;

	private string _namespace;

	public void Serialize(DbDatabaseMapping databaseMapping, XmlWriter xmlWriter)
	{
		_xmlWriter = xmlWriter;
		_databaseMapping = databaseMapping;
		_version = databaseMapping.Model.SchemaVersion;
		_namespace = (object.Equals(_version, 3.0) ? "http://schemas.microsoft.com/ado/2009/11/edmx" : (object.Equals(_version, 2.0) ? "http://schemas.microsoft.com/ado/2008/10/edmx" : "http://schemas.microsoft.com/ado/2007/06/edmx"));
		_xmlWriter.WriteStartDocument();
		using (Element("Edmx", "Version", string.Format(CultureInfo.InvariantCulture, "{0:F1}", new object[1] { _version })))
		{
			WriteEdmxRuntime();
			WriteEdmxDesigner();
		}
		_xmlWriter.WriteEndDocument();
		_xmlWriter.Flush();
	}

	private void WriteEdmxRuntime()
	{
		using (Element("Runtime"))
		{
			using (Element("ConceptualModels"))
			{
				_databaseMapping.Model.ValidateAndSerializeCsdl(_xmlWriter);
			}
			using (Element("Mappings"))
			{
				new MslSerializer().Serialize(_databaseMapping, _xmlWriter);
			}
			using (Element("StorageModels"))
			{
				new SsdlSerializer().Serialize(_databaseMapping.Database, _databaseMapping.ProviderInfo.ProviderInvariantName, _databaseMapping.ProviderInfo.ProviderManifestToken, _xmlWriter);
			}
		}
	}

	private void WriteEdmxDesigner()
	{
		using (Element("Designer"))
		{
			WriteEdmxConnection();
			WriteEdmxOptions();
			WriteEdmxDiagrams();
		}
	}

	private void WriteEdmxConnection()
	{
		using (Element("Connection"))
		{
			using (Element("DesignerInfoPropertySet"))
			{
				WriteDesignerPropertyElement("MetadataArtifactProcessing", "EmbedInOutputAssembly");
			}
		}
	}

	private void WriteEdmxOptions()
	{
		using (Element("Options"))
		{
			using (Element("DesignerInfoPropertySet"))
			{
				WriteDesignerPropertyElement("ValidateOnBuild", "False");
				WriteDesignerPropertyElement("CodeGenerationStrategy", "None");
				WriteDesignerPropertyElement("ProcessDependentTemplatesOnSave", "False");
				WriteDesignerPropertyElement("UseLegacyProvider", "False");
			}
		}
	}

	private void WriteDesignerPropertyElement(string name, string value)
	{
		using (Element("DesignerProperty", "Name", name, "Value", value))
		{
		}
	}

	private void WriteEdmxDiagrams()
	{
		using (Element("Diagrams"))
		{
		}
	}

	private IDisposable Element(string elementName, params string[] attributes)
	{
		_xmlWriter.WriteStartElement(elementName, _namespace);
		for (int i = 0; i < attributes.Length - 1; i += 2)
		{
			_xmlWriter.WriteAttributeString(attributes[i], attributes[i + 1]);
		}
		return new EndElement(_xmlWriter);
	}
}
