using System.Data.Entity.Core.Common;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Resources;
using System.Linq;

namespace System.Data.Entity.Utilities;

internal static class DbProviderManifestExtensions
{
	public static PrimitiveType GetStoreTypeFromName(this DbProviderManifest providerManifest, string name)
	{
		return providerManifest.GetStoreTypes().SingleOrDefault((PrimitiveType p) => string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase)) ?? throw Error.StoreTypeNotFound(name, providerManifest.NamespaceName);
	}
}
