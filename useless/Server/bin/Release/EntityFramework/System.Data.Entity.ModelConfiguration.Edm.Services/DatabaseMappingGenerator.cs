using System.Data.Entity.Core.Common;
using System.Data.Entity.Core.Mapping;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Resources;
using System.Linq;

namespace System.Data.Entity.ModelConfiguration.Edm.Services;

internal class DatabaseMappingGenerator
{
	private const string DiscriminatorColumnName = "Discriminator";

	public const int DiscriminatorMaxLength = 128;

	public static TypeUsage DiscriminatorTypeUsage = TypeUsage.CreateStringTypeUsage(PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String), isUnicode: true, isFixedLength: false, 128);

	private readonly DbProviderInfo _providerInfo;

	private readonly DbProviderManifest _providerManifest;

	public DatabaseMappingGenerator(DbProviderInfo providerInfo, DbProviderManifest providerManifest)
	{
		_providerInfo = providerInfo;
		_providerManifest = providerManifest;
	}

	public DbDatabaseMapping Generate(EdmModel conceptualModel)
	{
		DbDatabaseMapping dbDatabaseMapping = InitializeDatabaseMapping(conceptualModel);
		GenerateEntityTypes(dbDatabaseMapping);
		GenerateDiscriminators(dbDatabaseMapping);
		GenerateAssociationTypes(dbDatabaseMapping);
		return dbDatabaseMapping;
	}

	private DbDatabaseMapping InitializeDatabaseMapping(EdmModel conceptualModel)
	{
		EdmModel database = EdmModel.CreateStoreModel(_providerInfo, _providerManifest, conceptualModel.SchemaVersion);
		return new DbDatabaseMapping().Initialize(conceptualModel, database);
	}

	private static void GenerateEntityTypes(DbDatabaseMapping databaseMapping)
	{
		foreach (EntityType entityType in databaseMapping.Model.EntityTypes)
		{
			if (entityType.Abstract && databaseMapping.Model.EntityTypes.All((EntityType e) => e.BaseType != entityType))
			{
				throw new InvalidOperationException(Strings.UnmappedAbstractType(entityType.GetClrType()));
			}
			new TableMappingGenerator(databaseMapping.ProviderManifest).Generate(entityType, databaseMapping);
		}
	}

	private static void GenerateDiscriminators(DbDatabaseMapping databaseMapping)
	{
		foreach (EntitySetMapping entitySetMapping in databaseMapping.GetEntitySetMappings())
		{
			if (entitySetMapping.EntityTypeMappings.Count() <= 1)
			{
				continue;
			}
			TypeUsage storeType = databaseMapping.ProviderManifest.GetStoreType(DiscriminatorTypeUsage);
			EdmProperty edmProperty = new EdmProperty("Discriminator", storeType)
			{
				Nullable = false,
				DefaultValue = "(Undefined)"
			};
			entitySetMapping.EntityTypeMappings.First().MappingFragments.Single().Table.AddColumn(edmProperty);
			foreach (EntityTypeMapping entityTypeMapping in entitySetMapping.EntityTypeMappings)
			{
				if (!entityTypeMapping.EntityType.Abstract)
				{
					MappingFragment mappingFragment = entityTypeMapping.MappingFragments.Single();
					mappingFragment.SetDefaultDiscriminator(edmProperty);
					mappingFragment.AddDiscriminatorCondition(edmProperty, entityTypeMapping.EntityType.Name);
				}
			}
		}
	}

	private static void GenerateAssociationTypes(DbDatabaseMapping databaseMapping)
	{
		foreach (AssociationType associationType in databaseMapping.Model.AssociationTypes)
		{
			new AssociationTypeMappingGenerator(databaseMapping.ProviderManifest).Generate(associationType, databaseMapping);
		}
	}
}
