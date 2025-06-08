using System.Data.Entity.Core.Common;
using System.Data.Entity.Core.Mapping;
using System.Data.Entity.Core.Metadata.Edm;
using System.Linq;

namespace System.Data.Entity.ModelConfiguration.Edm.Services;

internal abstract class StructuralTypeMappingGenerator
{
	protected readonly DbProviderManifest _providerManifest;

	protected StructuralTypeMappingGenerator(DbProviderManifest providerManifest)
	{
		_providerManifest = providerManifest;
	}

	protected EdmProperty MapTableColumn(EdmProperty property, string columnName, bool isInstancePropertyOnDerivedType)
	{
		TypeUsage edmType = TypeUsage.Create(property.UnderlyingPrimitiveType, property.TypeUsage.Facets);
		TypeUsage storeType = _providerManifest.GetStoreType(edmType);
		EdmProperty edmProperty = new EdmProperty(columnName, storeType)
		{
			Nullable = (isInstancePropertyOnDerivedType || property.Nullable)
		};
		if (edmProperty.IsPrimaryKeyColumn)
		{
			edmProperty.Nullable = false;
		}
		StoreGeneratedPattern? storeGeneratedPattern = property.GetStoreGeneratedPattern();
		if (storeGeneratedPattern.HasValue)
		{
			edmProperty.StoreGeneratedPattern = storeGeneratedPattern.Value;
		}
		MapPrimitivePropertyFacets(property, edmProperty, storeType);
		return edmProperty;
	}

	internal static void MapPrimitivePropertyFacets(EdmProperty property, EdmProperty column, TypeUsage typeUsage)
	{
		if (IsValidFacet(typeUsage, "FixedLength") && property.IsFixedLength.HasValue)
		{
			column.IsFixedLength = property.IsFixedLength;
		}
		if (IsValidFacet(typeUsage, "MaxLength"))
		{
			column.IsMaxLength = property.IsMaxLength;
			if (!column.IsMaxLength || property.MaxLength.HasValue)
			{
				column.MaxLength = property.MaxLength;
			}
		}
		if (IsValidFacet(typeUsage, "Unicode") && property.IsUnicode.HasValue)
		{
			column.IsUnicode = property.IsUnicode;
		}
		if (IsValidFacet(typeUsage, "Precision") && property.Precision.HasValue)
		{
			column.Precision = property.Precision;
		}
		if (IsValidFacet(typeUsage, "Scale") && property.Scale.HasValue)
		{
			column.Scale = property.Scale;
		}
	}

	private static bool IsValidFacet(TypeUsage typeUsage, string name)
	{
		if (typeUsage.Facets.TryGetValue(name, ignoreCase: false, out var item))
		{
			return !item.Description.IsConstant;
		}
		return false;
	}

	protected static EntityTypeMapping GetEntityTypeMappingInHierarchy(DbDatabaseMapping databaseMapping, EntityType entityType)
	{
		EntityTypeMapping entityTypeMapping = databaseMapping.GetEntityTypeMapping(entityType);
		if (entityTypeMapping == null)
		{
			EntitySetMapping entitySetMapping = databaseMapping.GetEntitySetMapping(databaseMapping.Model.GetEntitySet(entityType));
			if (entitySetMapping != null)
			{
				entityTypeMapping = entitySetMapping.EntityTypeMappings.First((EntityTypeMapping etm) => entityType.DeclaredProperties.All((EdmProperty dp) => etm.MappingFragments.First().ColumnMappings.Select((ColumnMappingBuilder pm) => pm.PropertyPath.First()).Contains(dp)));
			}
		}
		return entityTypeMapping;
	}
}
