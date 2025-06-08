using System.Data.Entity.Core.Common;
using System.Data.Entity.Core.Mapping;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Internal;
using System.Data.Entity.ModelConfiguration.Edm;
using System.Linq;

namespace System.Data.Entity.Infrastructure;

public class DbModel : IEdmModelAdapter
{
	private readonly DbDatabaseMapping _databaseMapping;

	private readonly DbModelBuilder _cachedModelBuilder;

	public DbProviderInfo ProviderInfo => StoreModel.ProviderInfo;

	public DbProviderManifest ProviderManifest => StoreModel.ProviderManifest;

	public EdmModel ConceptualModel => _databaseMapping.Model;

	public EdmModel StoreModel => _databaseMapping.Database;

	public EntityContainerMapping ConceptualToStoreMapping => _databaseMapping.EntityContainerMappings.SingleOrDefault();

	internal DbModelBuilder CachedModelBuilder => _cachedModelBuilder;

	internal DbDatabaseMapping DatabaseMapping => _databaseMapping;

	internal DbModel(DbDatabaseMapping databaseMapping, DbModelBuilder modelBuilder)
	{
		_databaseMapping = databaseMapping;
		_cachedModelBuilder = modelBuilder;
	}

	internal DbModel(DbProviderInfo providerInfo, DbProviderManifest providerManifest)
	{
		_databaseMapping = new DbDatabaseMapping().Initialize(EdmModel.CreateConceptualModel(), EdmModel.CreateStoreModel(providerInfo, providerManifest));
	}

	internal DbModel(EdmModel conceptualModel, EdmModel storeModel)
	{
		_databaseMapping = new DbDatabaseMapping
		{
			Model = conceptualModel,
			Database = storeModel
		};
	}

	public DbCompiledModel Compile()
	{
		return new DbCompiledModel(CodeFirstCachedMetadataWorkspace.Create(DatabaseMapping), CachedModelBuilder);
	}
}
