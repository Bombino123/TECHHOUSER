using System.Collections.Generic;
using System.Data.Entity.Core.Common;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.ModelConfiguration.Edm.Services;
using System.Data.Entity.Utilities;
using System.Linq;
using System.Reflection;
using System.Xml;

namespace System.Data.Entity.ModelConfiguration.Edm;

internal static class EdmModelExtensions
{
	public const string DefaultSchema = "dbo";

	public const string DefaultModelNamespace = "CodeFirstNamespace";

	public const string DefaultStoreNamespace = "CodeFirstDatabaseSchema";

	public static EntityType AddTable(this EdmModel database, string name)
	{
		string text = database.EntityTypes.UniquifyName(name);
		EntityType entityType = new EntityType(text, "CodeFirstDatabaseSchema", DataSpace.SSpace);
		database.AddItem(entityType);
		database.AddEntitySet(entityType.Name, entityType, text);
		return entityType;
	}

	public static EntityType AddTable(this EdmModel database, string name, EntityType pkSource)
	{
		EntityType entityType = database.AddTable(name);
		foreach (EdmProperty keyProperty in pkSource.KeyProperties)
		{
			entityType.AddKeyMember(keyProperty.Clone());
		}
		return entityType;
	}

	public static EdmFunction AddFunction(this EdmModel database, string name, EdmFunctionPayload functionPayload)
	{
		EdmFunction edmFunction = new EdmFunction(database.Functions.UniquifyName(name), "CodeFirstDatabaseSchema", DataSpace.SSpace, functionPayload);
		database.AddItem(edmFunction);
		return edmFunction;
	}

	public static EntityType FindTableByName(this EdmModel database, DatabaseName tableName)
	{
		IList<EntityType> list = (database.EntityTypes as IList<EntityType>) ?? database.EntityTypes.ToList();
		for (int i = 0; i < list.Count; i++)
		{
			EntityType entityType = list[i];
			DatabaseName tableName2 = entityType.GetTableName();
			bool num;
			if (tableName2 == null)
			{
				if (!string.Equals(entityType.Name, tableName.Name, StringComparison.Ordinal))
				{
					continue;
				}
				num = tableName.Schema == null;
			}
			else
			{
				num = tableName2.Equals(tableName);
			}
			if (num)
			{
				return entityType;
			}
		}
		return null;
	}

	public static bool HasCascadeDeletePath(this EdmModel model, EntityType sourceEntityType, EntityType targetEntityType)
	{
		return (from a in model.AssociationTypes
			from ae in a.Members.Cast<AssociationEndMember>()
			where ae.GetEntityType() == sourceEntityType && ae.DeleteBehavior == OperationAction.Cascade
			select a.GetOtherEnd(ae).GetEntityType()).Any((EntityType et) => et == targetEntityType || model.HasCascadeDeletePath(et, targetEntityType));
	}

	public static IEnumerable<Type> GetClrTypes(this EdmModel model)
	{
		return model.EntityTypes.Select((EntityType e) => e.GetClrType()).Union(model.ComplexTypes.Select((ComplexType ct) => ct.GetClrType()));
	}

	public static NavigationProperty GetNavigationProperty(this EdmModel model, PropertyInfo propertyInfo)
	{
		IList<EntityType> list = (model.EntityTypes as IList<EntityType>) ?? model.EntityTypes.ToList();
		for (int i = 0; i < list.Count; i++)
		{
			NavigationProperty navigationProperty = list[i].GetNavigationProperty(propertyInfo);
			if (navigationProperty != null)
			{
				return navigationProperty;
			}
		}
		return null;
	}

	public static void ValidateAndSerializeCsdl(this EdmModel model, XmlWriter writer)
	{
		List<DataModelErrorEventArgs> list = model.SerializeAndGetCsdlErrors(writer);
		if (list.Count > 0)
		{
			throw new ModelValidationException(list);
		}
	}

	private static List<DataModelErrorEventArgs> SerializeAndGetCsdlErrors(this EdmModel model, XmlWriter writer)
	{
		List<DataModelErrorEventArgs> validationErrors = new List<DataModelErrorEventArgs>();
		CsdlSerializer csdlSerializer = new CsdlSerializer();
		csdlSerializer.OnError += delegate(object s, DataModelErrorEventArgs e)
		{
			validationErrors.Add(e);
		};
		csdlSerializer.Serialize(model, writer);
		return validationErrors;
	}

	public static DbDatabaseMapping GenerateDatabaseMapping(this EdmModel model, DbProviderInfo providerInfo, DbProviderManifest providerManifest)
	{
		return new DatabaseMappingGenerator(providerInfo, providerManifest).Generate(model);
	}

	public static EdmType GetStructuralOrEnumType(this EdmModel model, string name)
	{
		return model.GetStructuralType(name) ?? model.GetEnumType(name);
	}

	public static EdmType GetStructuralType(this EdmModel model, string name)
	{
		return (EdmType)(((object)model.GetEntityType(name)) ?? ((object)model.GetComplexType(name)));
	}

	public static EntityType GetEntityType(this EdmModel model, string name)
	{
		return model.EntityTypes.SingleOrDefault((EntityType e) => e.Name == name);
	}

	public static EntityType GetEntityType(this EdmModel model, Type clrType)
	{
		IList<EntityType> list = (model.EntityTypes as IList<EntityType>) ?? model.EntityTypes.ToList();
		for (int i = 0; i < list.Count; i++)
		{
			EntityType entityType = list[i];
			if (entityType.GetClrType() == clrType)
			{
				return entityType;
			}
		}
		return null;
	}

	public static ComplexType GetComplexType(this EdmModel model, string name)
	{
		return model.ComplexTypes.SingleOrDefault((ComplexType e) => e.Name == name);
	}

	public static ComplexType GetComplexType(this EdmModel model, Type clrType)
	{
		return model.ComplexTypes.SingleOrDefault((ComplexType e) => e.GetClrType() == clrType);
	}

	public static EnumType GetEnumType(this EdmModel model, string name)
	{
		return model.EnumTypes.SingleOrDefault((EnumType e) => e.Name == name);
	}

	public static EntityType AddEntityType(this EdmModel model, string name, string modelNamespace = null)
	{
		EntityType entityType = new EntityType(name, modelNamespace ?? "CodeFirstNamespace", DataSpace.CSpace);
		model.AddItem(entityType);
		return entityType;
	}

	public static EntitySet GetEntitySet(this EdmModel model, EntityType entityType)
	{
		return model.GetEntitySets().SingleOrDefault((EntitySet e) => e.ElementType == entityType.GetRootType());
	}

	public static AssociationSet GetAssociationSet(this EdmModel model, AssociationType associationType)
	{
		return model.Containers.Single().AssociationSets.SingleOrDefault((AssociationSet a) => a.ElementType == associationType);
	}

	public static IEnumerable<EntitySet> GetEntitySets(this EdmModel model)
	{
		return model.Containers.Single().EntitySets;
	}

	public static EntitySet AddEntitySet(this EdmModel model, string name, EntityType elementType, string table = null)
	{
		EntitySet entitySet = new EntitySet(name, null, table, null, elementType);
		model.Containers.Single().AddEntitySetBase(entitySet);
		return entitySet;
	}

	public static ComplexType AddComplexType(this EdmModel model, string name, string modelNamespace = null)
	{
		ComplexType complexType = new ComplexType(name, modelNamespace ?? "CodeFirstNamespace", DataSpace.CSpace);
		model.AddItem(complexType);
		return complexType;
	}

	public static EnumType AddEnumType(this EdmModel model, string name, string modelNamespace = null)
	{
		EnumType enumType = new EnumType(name, modelNamespace ?? "CodeFirstNamespace", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.Int32), isFlags: false, DataSpace.CSpace);
		model.AddItem(enumType);
		return enumType;
	}

	public static AssociationType GetAssociationType(this EdmModel model, string name)
	{
		return model.AssociationTypes.SingleOrDefault((AssociationType a) => a.Name == name);
	}

	public static IEnumerable<AssociationType> GetAssociationTypesBetween(this EdmModel model, EntityType first, EntityType second)
	{
		return model.AssociationTypes.Where((AssociationType a) => (a.SourceEnd.GetEntityType() == first && a.TargetEnd.GetEntityType() == second) || (a.SourceEnd.GetEntityType() == second && a.TargetEnd.GetEntityType() == first));
	}

	public static AssociationType AddAssociationType(this EdmModel model, string name, EntityType sourceEntityType, RelationshipMultiplicity sourceAssociationEndKind, EntityType targetEntityType, RelationshipMultiplicity targetAssociationEndKind, string modelNamespace = null)
	{
		AssociationType associationType = new AssociationType(name, modelNamespace ?? "CodeFirstNamespace", foreignKey: false, DataSpace.CSpace)
		{
			SourceEnd = new AssociationEndMember(name + "_Source", sourceEntityType.GetReferenceType(), sourceAssociationEndKind),
			TargetEnd = new AssociationEndMember(name + "_Target", targetEntityType.GetReferenceType(), targetAssociationEndKind)
		};
		model.AddAssociationType(associationType);
		return associationType;
	}

	public static void AddAssociationType(this EdmModel model, AssociationType associationType)
	{
		model.AddItem(associationType);
	}

	public static void AddAssociationSet(this EdmModel model, AssociationSet associationSet)
	{
		model.Containers.Single().AddEntitySetBase(associationSet);
	}

	public static void RemoveEntityType(this EdmModel model, EntityType entityType)
	{
		model.RemoveItem(entityType);
		EntityContainer entityContainer = model.Containers.Single();
		EntitySet entitySet = entityContainer.EntitySets.SingleOrDefault((EntitySet a) => a.ElementType == entityType);
		if (entitySet != null)
		{
			entityContainer.RemoveEntitySetBase(entitySet);
		}
	}

	public static void ReplaceEntitySet(this EdmModel model, EntityType entityType, EntitySet newSet)
	{
		EntityContainer entityContainer = model.Containers.Single();
		EntitySet entitySet = entityContainer.EntitySets.SingleOrDefault((EntitySet a) => a.ElementType == entityType);
		if (entitySet == null)
		{
			return;
		}
		entityContainer.RemoveEntitySetBase(entitySet);
		if (newSet == null)
		{
			return;
		}
		foreach (AssociationSet associationSet in model.Containers.Single().AssociationSets)
		{
			if (associationSet.SourceSet == entitySet)
			{
				associationSet.SourceSet = newSet;
			}
			if (associationSet.TargetSet == entitySet)
			{
				associationSet.TargetSet = newSet;
			}
		}
	}

	public static void RemoveAssociationType(this EdmModel model, AssociationType associationType)
	{
		model.RemoveItem(associationType);
		EntityContainer entityContainer = model.Containers.Single();
		AssociationSet associationSet = entityContainer.AssociationSets.SingleOrDefault((AssociationSet a) => a.ElementType == associationType);
		if (associationSet != null)
		{
			entityContainer.RemoveEntitySetBase(associationSet);
		}
	}

	public static AssociationSet AddAssociationSet(this EdmModel model, string name, AssociationType associationType)
	{
		AssociationSet associationSet = new AssociationSet(name, associationType)
		{
			SourceSet = model.GetEntitySet(associationType.SourceEnd.GetEntityType()),
			TargetSet = model.GetEntitySet(associationType.TargetEnd.GetEntityType())
		};
		model.Containers.Single().AddEntitySetBase(associationSet);
		return associationSet;
	}

	public static IEnumerable<EntityType> GetDerivedTypes(this EdmModel model, EntityType entityType)
	{
		return model.EntityTypes.Where((EntityType et) => et.BaseType == entityType);
	}

	public static IEnumerable<EntityType> GetSelfAndAllDerivedTypes(this EdmModel model, EntityType entityType)
	{
		List<EntityType> list = new List<EntityType>();
		AddSelfAndAllDerivedTypes(model, entityType, list);
		return list;
	}

	private static void AddSelfAndAllDerivedTypes(EdmModel model, EntityType entityType, List<EntityType> entityTypes)
	{
		entityTypes.Add(entityType);
		foreach (EntityType item in model.EntityTypes.Where((EntityType et) => et.BaseType == entityType))
		{
			AddSelfAndAllDerivedTypes(model, item, entityTypes);
		}
	}
}
