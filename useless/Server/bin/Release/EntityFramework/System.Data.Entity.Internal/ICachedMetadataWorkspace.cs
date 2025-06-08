using System.Collections.Generic;
using System.Data.Common;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Infrastructure;
using System.Reflection;

namespace System.Data.Entity.Internal;

internal interface ICachedMetadataWorkspace
{
	IEnumerable<Assembly> Assemblies { get; }

	string DefaultContainerName { get; }

	DbProviderInfo ProviderInfo { get; }

	MetadataWorkspace GetMetadataWorkspace(DbConnection storeConnection);
}
