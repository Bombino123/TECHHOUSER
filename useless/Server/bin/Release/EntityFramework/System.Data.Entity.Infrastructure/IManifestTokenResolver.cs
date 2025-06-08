using System.Data.Common;

namespace System.Data.Entity.Infrastructure;

public interface IManifestTokenResolver
{
	string ResolveManifestToken(DbConnection connection);
}
