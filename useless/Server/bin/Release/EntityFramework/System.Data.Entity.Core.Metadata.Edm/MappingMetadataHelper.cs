using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity.Core.Common.Utils;
using System.Data.Entity.Core.Mapping;
using System.Data.Entity.Resources;
using System.Linq;

namespace System.Data.Entity.Core.Metadata.Edm;

internal static class MappingMetadataHelper
{
	internal static IEnumerable<TypeMapping> GetMappingsForEntitySetAndType(StorageMappingItemCollection mappingCollection, EntityContainer container, EntitySetBase entitySet, EntityTypeBase entityType)
	{
		EntitySetBaseMapping setMapping = GetEntityContainerMap(mappingCollection, container).GetSetMapping(entitySet.Name);
		if (setMapping == null)
		{
			yield break;
		}
		foreach (TypeMapping item in setMapping.TypeMappings.Where((TypeMapping map) => map.Types.Union(map.IsOfTypes).Contains(entityType)))
		{
			yield return item;
		}
	}

	internal static IEnumerable<TypeMapping> GetMappingsForEntitySetAndSuperTypes(StorageMappingItemCollection mappingCollection, EntityContainer container, EntitySetBase entitySet, EntityTypeBase childEntityType)
	{
		return MetadataHelper.GetTypeAndParentTypesOf(childEntityType, includeAbstractTypes: true).SelectMany(delegate(EdmType edmType)
		{
			EntityTypeBase entityType = edmType as EntityTypeBase;
			return (!edmType.EdmEquals(childEntityType)) ? GetIsTypeOfMappingsForEntitySetAndType(mappingCollection, container, entitySet, entityType, childEntityType) : GetMappingsForEntitySetAndType(mappingCollection, container, entitySet, entityType);
		}).ToList();
	}

	private static IEnumerable<TypeMapping> GetIsTypeOfMappingsForEntitySetAndType(StorageMappingItemCollection mappingCollection, EntityContainer container, EntitySetBase entitySet, EntityTypeBase entityType, EntityTypeBase childEntityType)
	{
		foreach (TypeMapping item in GetMappingsForEntitySetAndType(mappingCollection, container, entitySet, entityType))
		{
			if (item.IsOfTypes.Any((EntityTypeBase parentType) => parentType.IsAssignableFrom(childEntityType)) || item.Types.Contains(childEntityType))
			{
				yield return item;
			}
		}
	}

	internal static IEnumerable<EntityTypeModificationFunctionMapping> GetModificationFunctionMappingsForEntitySetAndType(StorageMappingItemCollection mappingCollection, EntityContainer container, EntitySetBase entitySet, EntityTypeBase entityType)
	{
		if (!(GetEntityContainerMap(mappingCollection, container).GetSetMapping(entitySet.Name) is EntitySetMapping entitySetMapping) || entitySetMapping == null)
		{
			yield break;
		}
		foreach (EntityTypeModificationFunctionMapping item in entitySetMapping.ModificationFunctionMappings.Where((EntityTypeModificationFunctionMapping functionMap) => functionMap.EntityType.Equals(entityType)))
		{
			yield return item;
		}
	}

	internal static EntityContainerMapping GetEntityContainerMap(StorageMappingItemCollection mappingCollection, EntityContainer entityContainer)
	{
		ReadOnlyCollection<EntityContainerMapping> items = mappingCollection.GetItems<EntityContainerMapping>();
		EntityContainerMapping entityContainerMapping = null;
		foreach (EntityContainerMapping item in items)
		{
			if (entityContainer.Equals(item.EdmEntityContainer) || entityContainer.Equals(item.StorageEntityContainer))
			{
				entityContainerMapping = item;
				break;
			}
		}
		if (entityContainerMapping == null)
		{
			throw new MappingException(Strings.Mapping_NotFound_EntityContainer(entityContainer.Name));
		}
		return entityContainerMapping;
	}
}
