using System.Data.Entity.Internal;
using System.Data.Entity.Utilities;
using System.Xml;
using System.Xml.Linq;

namespace System.Data.Entity.Infrastructure;

public static class EdmxReader
{
	public static DbCompiledModel Read(XmlReader reader, string defaultSchema)
	{
		Check.NotNull(reader, "reader");
		DbProviderInfo providerInfo;
		return new DbCompiledModel(CodeFirstCachedMetadataWorkspace.Create(XDocument.Load(reader).GetStorageMappingItemCollection(out providerInfo), providerInfo), defaultSchema);
	}
}
