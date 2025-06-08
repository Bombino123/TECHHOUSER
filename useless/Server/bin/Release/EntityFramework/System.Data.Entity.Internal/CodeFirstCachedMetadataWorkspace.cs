using System.Collections.Generic;
using System.Data.Common;
using System.Data.Entity.Core.Mapping;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.ModelConfiguration.Edm;
using System.Data.Entity.Resources;
using System.Data.Entity.Utilities;
using System.Linq;
using System.Reflection;

namespace System.Data.Entity.Internal;

internal class CodeFirstCachedMetadataWorkspace : ICachedMetadataWorkspace
{
	private readonly MetadataWorkspace _metadataWorkspace;

	private readonly IEnumerable<Assembly> _assemblies;

	private readonly DbProviderInfo _providerInfo;

	private readonly string _defaultContainerName;

	public string DefaultContainerName => _defaultContainerName;

	public IEnumerable<Assembly> Assemblies => _assemblies;

	public DbProviderInfo ProviderInfo => _providerInfo;

	private CodeFirstCachedMetadataWorkspace(MetadataWorkspace metadataWorkspace, IEnumerable<Assembly> assemblies, DbProviderInfo providerInfo, string defaultContainerName)
	{
		_metadataWorkspace = metadataWorkspace;
		_assemblies = assemblies;
		_providerInfo = providerInfo;
		_defaultContainerName = defaultContainerName;
	}

	public MetadataWorkspace GetMetadataWorkspace(DbConnection connection)
	{
		string providerInvariantName = connection.GetProviderInvariantName();
		if (!string.Equals(_providerInfo.ProviderInvariantName, providerInvariantName, StringComparison.Ordinal))
		{
			throw Error.CodeFirstCachedMetadataWorkspace_SameModelDifferentProvidersNotSupported();
		}
		return _metadataWorkspace;
	}

	public static CodeFirstCachedMetadataWorkspace Create(DbDatabaseMapping databaseMapping)
	{
		EdmModel model = databaseMapping.Model;
		return new CodeFirstCachedMetadataWorkspace(databaseMapping.ToMetadataWorkspace(), (from t in model.GetClrTypes()
			select t.Assembly()).Distinct().ToArray(), databaseMapping.ProviderInfo, model.Container.Name);
	}

	public static CodeFirstCachedMetadataWorkspace Create(StorageMappingItemCollection mappingItemCollection, DbProviderInfo providerInfo)
	{
		EdmItemCollection edmItemCollection = mappingItemCollection.EdmItemCollection;
		IEnumerable<Type> first = from et in edmItemCollection.GetItems<EntityType>()
			select et.GetClrType();
		IEnumerable<Type> second = from ct in edmItemCollection.GetItems<ComplexType>()
			select ct.GetClrType();
		return new CodeFirstCachedMetadataWorkspace(mappingItemCollection.Workspace, (from t in first.Union(second)
			select t.Assembly()).Distinct().ToArray(), providerInfo, edmItemCollection.GetItems<EntityContainer>().Single().Name);
	}
}
