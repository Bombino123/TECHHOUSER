using System.Collections.Generic;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Utilities;
using System.Linq;
using System.Reflection;

namespace System.Data.Entity.ModelConfiguration.Edm;

internal static class EntityTypeExtensions
{
	private const string TableNameAnnotation = "TableName";

	public static void AddColumn(this EntityType table, EdmProperty column)
	{
		column.SetPreferredName(column.Name);
		column.Name = table.Properties.UniquifyName(column.Name);
		table.AddMember(column);
	}

	public static void SetConfiguration(this EntityType table, object configuration)
	{
		table.GetMetadataProperties().SetConfiguration(configuration);
	}

	public static DatabaseName GetTableName(this EntityType table)
	{
		return (DatabaseName)table.Annotations.GetAnnotation("TableName");
	}

	public static void SetTableName(this EntityType table, DatabaseName tableName)
	{
		table.GetMetadataProperties().SetAnnotation("TableName", tableName);
	}

	internal static IEnumerable<EntityType> ToHierarchy(this EntityType edmType)
	{
		return EdmType.SafeTraverseHierarchy(edmType);
	}

	public static IEnumerable<EdmProperty> GetValidKey(this EntityType entityType)
	{
		List<EdmProperty> list = null;
		List<EntityType> list2 = entityType.ToHierarchy().ToList();
		for (int num = list2.Count - 1; num >= 0; num--)
		{
			EntityType entityType2 = list2[num];
			if (entityType2.BaseType == null && entityType2.KeyProperties.Count > 0)
			{
				if (list != null)
				{
					return Enumerable.Empty<EdmProperty>();
				}
				list = new List<EdmProperty>();
				HashSet<EdmProperty> hashSet = new HashSet<EdmProperty>();
				HashSet<string> hashSet2 = new HashSet<string>();
				HashSet<EdmProperty> hashSet3 = new HashSet<EdmProperty>(entityType2.DeclaredProperties.Where((EdmProperty p) => p != null));
				for (int i = 0; i < entityType2.KeyProperties.Count; i++)
				{
					EdmProperty edmProperty = entityType2.KeyProperties[i];
					if (edmProperty != null && !hashSet.Contains(edmProperty) && hashSet3.Contains(edmProperty) && !string.IsNullOrEmpty(edmProperty.Name) && !string.IsNullOrWhiteSpace(edmProperty.Name) && !hashSet2.Contains(edmProperty.Name))
					{
						list.Add(edmProperty);
						hashSet.Add(edmProperty);
						hashSet2.Add(edmProperty.Name);
						continue;
					}
					return Enumerable.Empty<EdmProperty>();
				}
			}
		}
		IEnumerable<EdmProperty> enumerable = list;
		return enumerable ?? Enumerable.Empty<EdmProperty>();
	}

	public static List<EdmProperty> GetKeyProperties(this EntityType entityType)
	{
		HashSet<EntityType> visitedTypes = new HashSet<EntityType>();
		List<EdmProperty> list = new List<EdmProperty>();
		GetKeyProperties(visitedTypes, entityType, list);
		return list;
	}

	private static void GetKeyProperties(HashSet<EntityType> visitedTypes, EntityType visitingType, List<EdmProperty> keyProperties)
	{
		if (visitedTypes.Contains(visitingType))
		{
			return;
		}
		visitedTypes.Add(visitingType);
		if (visitingType.BaseType != null)
		{
			GetKeyProperties(visitedTypes, (EntityType)visitingType.BaseType, keyProperties);
			return;
		}
		ReadOnlyMetadataCollection<EdmProperty> keyProperties2 = visitingType.KeyProperties;
		if (keyProperties2.Count > 0)
		{
			keyProperties.AddRange(keyProperties2);
		}
	}

	public static EntityType GetRootType(this EntityType entityType)
	{
		EdmType edmType = entityType;
		while (edmType.BaseType != null)
		{
			edmType = edmType.BaseType;
		}
		return (EntityType)edmType;
	}

	public static bool IsAncestorOf(this EntityType ancestor, EntityType entityType)
	{
		while (entityType != null)
		{
			if (entityType.BaseType == ancestor)
			{
				return true;
			}
			entityType = (EntityType)entityType.BaseType;
		}
		return false;
	}

	public static IEnumerable<EdmProperty> KeyProperties(this EntityType entityType)
	{
		return entityType.GetRootType().KeyProperties;
	}

	public static object GetConfiguration(this EntityType entityType)
	{
		return entityType.Annotations.GetConfiguration();
	}

	public static Type GetClrType(this EntityType entityType)
	{
		return entityType.Annotations.GetClrType();
	}

	public static IEnumerable<EntityType> TypeHierarchyIterator(this EntityType entityType, EdmModel model)
	{
		yield return entityType;
		IEnumerable<EntityType> derivedTypes = model.GetDerivedTypes(entityType);
		if (derivedTypes == null)
		{
			yield break;
		}
		foreach (EntityType item in derivedTypes)
		{
			foreach (EntityType item2 in item.TypeHierarchyIterator(model))
			{
				yield return item2;
			}
		}
	}

	public static EdmProperty AddComplexProperty(this EntityType entityType, string name, ComplexType complexType)
	{
		EdmProperty edmProperty = EdmProperty.CreateComplex(name, complexType);
		entityType.AddMember(edmProperty);
		return edmProperty;
	}

	public static EdmProperty GetDeclaredPrimitiveProperty(this EntityType entityType, PropertyInfo propertyInfo)
	{
		return entityType.GetDeclaredPrimitiveProperties().SingleOrDefault((EdmProperty p) => p.GetClrPropertyInfo().IsSameAs(propertyInfo));
	}

	public static IEnumerable<EdmProperty> GetDeclaredPrimitiveProperties(this EntityType entityType)
	{
		return entityType.DeclaredProperties.Where((EdmProperty p) => p.IsUnderlyingPrimitiveType);
	}

	public static NavigationProperty AddNavigationProperty(this EntityType entityType, string name, AssociationType associationType)
	{
		EntityType entityType2 = associationType.TargetEnd.GetEntityType();
		EdmType edmType = (associationType.TargetEnd.RelationshipMultiplicity.IsMany() ? ((EdmType)entityType2.GetCollectionType()) : ((EdmType)entityType2));
		NavigationProperty navigationProperty = new NavigationProperty(name, TypeUsage.Create(edmType))
		{
			RelationshipType = associationType,
			FromEndMember = associationType.SourceEnd,
			ToEndMember = associationType.TargetEnd
		};
		entityType.AddMember(navigationProperty);
		return navigationProperty;
	}

	public static NavigationProperty GetNavigationProperty(this EntityType entityType, PropertyInfo propertyInfo)
	{
		return entityType.NavigationProperties.SingleOrDefault((NavigationProperty np) => np.GetClrPropertyInfo().IsSameAs(propertyInfo));
	}

	public static bool IsRootOfSet(this EntityType entityType, IEnumerable<EntityType> set)
	{
		return set.All((EntityType et) => et == entityType || entityType.IsAncestorOf(et) || et.GetRootType() != entityType.GetRootType());
	}
}
