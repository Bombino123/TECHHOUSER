using System.Data.Entity.Core.Mapping;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Migrations.Edm;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace System.Data.Entity.Utilities;

internal static class XDocumentExtensions
{
	public static StorageMappingItemCollection GetStorageMappingItemCollection(this XDocument model, out DbProviderInfo providerInfo)
	{
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		EdmItemCollection edmCollection = new EdmItemCollection(new XmlReader[1] { ((XNode)((XContainer)(object)model).Descendants(EdmXNames.Csdl.SchemaNames).Single()).CreateReader() });
		XElement val = ((XContainer)(object)model).Descendants(EdmXNames.Ssdl.SchemaNames).Single();
		providerInfo = new DbProviderInfo(val.ProviderAttribute(), val.ProviderManifestTokenAttribute());
		StoreItemCollection storeCollection = new StoreItemCollection(new XmlReader[1] { ((XNode)val).CreateReader() });
		return new StorageMappingItemCollection(edmCollection, storeCollection, new XmlReader[1] { ((XNode)new XElement(((XContainer)(object)model).Descendants(EdmXNames.Msl.MappingNames).Single())).CreateReader() });
	}
}
