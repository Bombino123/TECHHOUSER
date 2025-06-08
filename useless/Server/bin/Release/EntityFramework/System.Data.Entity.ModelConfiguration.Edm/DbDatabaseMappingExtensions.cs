using System.Collections.Generic;
using System.Data.Entity.Core.Mapping;
using System.Data.Entity.Core.Metadata.Edm;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace System.Data.Entity.ModelConfiguration.Edm;

internal static class DbDatabaseMappingExtensions
{
	public static DbDatabaseMapping Initialize(this DbDatabaseMapping databaseMapping, EdmModel model, EdmModel database)
	{
		databaseMapping.Model = model;
		databaseMapping.Database = database;
		databaseMapping.AddEntityContainerMapping(new EntityContainerMapping(model.Containers.Single()));
		return databaseMapping;
	}

	public static MetadataWorkspace ToMetadataWorkspace(this DbDatabaseMapping databaseMapping)
	{
		EdmItemCollection itemCollection = new EdmItemCollection(databaseMapping.Model);
		StoreItemCollection storeItemCollection = new StoreItemCollection(databaseMapping.Database);
		StorageMappingItemCollection storageMappingItemCollection = databaseMapping.ToStorageMappingItemCollection(itemCollection, storeItemCollection);
		MetadataWorkspace metadataWorkspace = new MetadataWorkspace(() => itemCollection, () => storeItemCollection, () => storageMappingItemCollection);
		new CodeFirstOSpaceLoader().LoadTypes(itemCollection, (ObjectItemCollection)metadataWorkspace.GetItemCollection(DataSpace.OSpace));
		return metadataWorkspace;
	}

	public static StorageMappingItemCollection ToStorageMappingItemCollection(this DbDatabaseMapping databaseMapping, EdmItemCollection itemCollection, StoreItemCollection storeItemCollection)
	{
		StringBuilder stringBuilder = new StringBuilder();
		using (XmlWriter xmlWriter = XmlWriter.Create(stringBuilder, new XmlWriterSettings
		{
			Indent = true
		}))
		{
			new MslSerializer().Serialize(databaseMapping, xmlWriter);
		}
		using XmlReader xmlReader = XmlReader.Create(new StringReader(stringBuilder.ToString()));
		return new StorageMappingItemCollection(itemCollection, storeItemCollection, new XmlReader[1] { xmlReader });
	}

	public static EntityTypeMapping GetEntityTypeMapping(this DbDatabaseMapping databaseMapping, EntityType entityType)
	{
		IList<EntityTypeMapping> entityTypeMappings = databaseMapping.GetEntityTypeMappings(entityType);
		if (entityTypeMappings.Count <= 1)
		{
			return entityTypeMappings.FirstOrDefault();
		}
		return entityTypeMappings.SingleOrDefault((EntityTypeMapping m) => m.IsHierarchyMapping);
	}

	public static IList<EntityTypeMapping> GetEntityTypeMappings(this DbDatabaseMapping databaseMapping, EntityType entityType)
	{
		List<EntityTypeMapping> list = new List<EntityTypeMapping>();
		foreach (EntitySetMapping entitySetMapping in databaseMapping.EntityContainerMappings.Single().EntitySetMappings)
		{
			foreach (EntityTypeMapping entityTypeMapping in entitySetMapping.EntityTypeMappings)
			{
				if (entityTypeMapping.EntityType == entityType)
				{
					list.Add(entityTypeMapping);
				}
			}
		}
		return list;
	}

	public static EntityTypeMapping GetEntityTypeMapping(this DbDatabaseMapping databaseMapping, Type clrType)
	{
		List<EntityTypeMapping> list = new List<EntityTypeMapping>();
		foreach (EntitySetMapping entitySetMapping in databaseMapping.EntityContainerMappings.Single().EntitySetMappings)
		{
			foreach (EntityTypeMapping entityTypeMapping in entitySetMapping.EntityTypeMappings)
			{
				if (entityTypeMapping.GetClrType() == clrType)
				{
					list.Add(entityTypeMapping);
				}
			}
		}
		if (list.Count <= 1)
		{
			return list.FirstOrDefault();
		}
		return list.SingleOrDefault((EntityTypeMapping m) => m.IsHierarchyMapping);
	}

	public static IEnumerable<Tuple<ColumnMappingBuilder, EntityType>> GetComplexPropertyMappings(this DbDatabaseMapping databaseMapping, Type complexType)
	{
		return from esm in databaseMapping.EntityContainerMappings.Single().EntitySetMappings
			from etm in esm.EntityTypeMappings
			from etmf in etm.MappingFragments
			from epm in etmf.ColumnMappings
			where epm.PropertyPath.Any((EdmProperty p) => p.IsComplexType && p.ComplexType.GetClrType() == complexType)
			select Tuple.Create(epm, etmf.Table);
	}

	public static IEnumerable<ModificationFunctionParameterBinding> GetComplexParameterBindings(this DbDatabaseMapping databaseMapping, Type complexType)
	{
		return from esm in databaseMapping.GetEntitySetMappings()
			from mfm in esm.ModificationFunctionMappings
			from pb in mfm.PrimaryParameterBindings
			where pb.MemberPath.Members.OfType<EdmProperty>().Any((EdmProperty p) => p.IsComplexType && p.ComplexType.GetClrType() == complexType)
			select pb;
	}

	public static EntitySetMapping GetEntitySetMapping(this DbDatabaseMapping databaseMapping, EntitySet entitySet)
	{
		return databaseMapping.EntityContainerMappings.Single().EntitySetMappings.SingleOrDefault((EntitySetMapping e) => e.EntitySet == entitySet);
	}

	public static IEnumerable<EntitySetMapping> GetEntitySetMappings(this DbDatabaseMapping databaseMapping)
	{
		return databaseMapping.EntityContainerMappings.Single().EntitySetMappings;
	}

	public static IEnumerable<AssociationSetMapping> GetAssociationSetMappings(this DbDatabaseMapping databaseMapping)
	{
		return databaseMapping.EntityContainerMappings.Single().AssociationSetMappings;
	}

	public static EntitySetMapping AddEntitySetMapping(this DbDatabaseMapping databaseMapping, EntitySet entitySet)
	{
		EntitySetMapping entitySetMapping = new EntitySetMapping(entitySet, null);
		databaseMapping.EntityContainerMappings.Single().AddSetMapping(entitySetMapping);
		return entitySetMapping;
	}

	public static AssociationSetMapping AddAssociationSetMapping(this DbDatabaseMapping databaseMapping, AssociationSet associationSet, EntitySet entitySet)
	{
		EntityContainerMapping entityContainerMapping = databaseMapping.EntityContainerMappings.Single();
		AssociationSetMapping associationSetMapping = new AssociationSetMapping(associationSet, entitySet, entityContainerMapping).Initialize();
		entityContainerMapping.AddSetMapping(associationSetMapping);
		return associationSetMapping;
	}
}
