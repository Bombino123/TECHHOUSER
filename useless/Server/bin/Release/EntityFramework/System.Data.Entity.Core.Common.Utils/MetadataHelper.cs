using System.Collections.Generic;
using System.Data.Entity.Core.Mapping;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Resources;
using System.Linq;
using System.Security.Cryptography;

namespace System.Data.Entity.Core.Common.Utils;

internal static class MetadataHelper
{
	internal static bool TryGetFunctionImportReturnType<T>(EdmFunction functionImport, int resultSetIndex, out T returnType) where T : EdmType
	{
		if (TryGetWrappedReturnEdmTypeFromFunctionImport<T>(functionImport, resultSetIndex, out var resultType) && ((typeof(EntityType).Equals(typeof(T)) && resultType is EntityType) || (typeof(ComplexType).Equals(typeof(T)) && resultType is ComplexType) || (typeof(StructuralType).Equals(typeof(T)) && resultType is StructuralType) || (typeof(EdmType).Equals(typeof(T)) && resultType != null)))
		{
			returnType = resultType;
			return true;
		}
		returnType = null;
		return false;
	}

	private static bool TryGetWrappedReturnEdmTypeFromFunctionImport<T>(EdmFunction functionImport, int resultSetIndex, out T resultType) where T : EdmType
	{
		resultType = null;
		if (TryGetFunctionImportReturnCollectionType(functionImport, resultSetIndex, out var collectionType))
		{
			resultType = collectionType.TypeUsage.EdmType as T;
			return true;
		}
		return false;
	}

	internal static bool TryGetFunctionImportReturnCollectionType(EdmFunction functionImport, int resultSetIndex, out CollectionType collectionType)
	{
		FunctionParameter returnParameter = GetReturnParameter(functionImport, resultSetIndex);
		if (returnParameter != null && returnParameter.TypeUsage.EdmType.BuiltInTypeKind == BuiltInTypeKind.CollectionType)
		{
			collectionType = (CollectionType)returnParameter.TypeUsage.EdmType;
			return true;
		}
		collectionType = null;
		return false;
	}

	internal static FunctionParameter GetReturnParameter(EdmFunction functionImport, int resultSetIndex)
	{
		if (functionImport.ReturnParameters.Count <= resultSetIndex)
		{
			return null;
		}
		return functionImport.ReturnParameters[resultSetIndex];
	}

	internal static EdmFunction GetFunctionImport(string functionName, string defaultContainerName, MetadataWorkspace workspace, out string containerName, out string functionImportName)
	{
		CommandHelper.ParseFunctionImportCommandText(functionName, defaultContainerName, out containerName, out functionImportName);
		return CommandHelper.FindFunctionImport(workspace, containerName, functionImportName);
	}

	internal static EdmType GetAndCheckFunctionImportReturnType<TElement>(EdmFunction functionImport, int resultSetIndex, MetadataWorkspace workspace)
	{
		if (!TryGetFunctionImportReturnType<EdmType>(functionImport, resultSetIndex, out var returnType))
		{
			throw EntityUtil.ExecuteFunctionCalledWithNonReaderFunction(functionImport);
		}
		CheckFunctionImportReturnType<TElement>(returnType, workspace);
		return returnType;
	}

	internal static void CheckFunctionImportReturnType<TElement>(EdmType expectedEdmType, MetadataWorkspace workspace)
	{
		EdmType item = expectedEdmType;
		if (Helper.IsSpatialType(expectedEdmType, out var isGeographic))
		{
			item = PrimitiveType.GetEdmPrimitiveType(isGeographic ? PrimitiveTypeKind.Geography : PrimitiveTypeKind.Geometry);
		}
		if (!workspace.TryDetermineCSpaceModelType<TElement>(out var modelEdmType) || !modelEdmType.EdmEquals(item))
		{
			throw new InvalidOperationException(Strings.ObjectContext_ExecuteFunctionTypeMismatch(typeof(TElement).FullName, expectedEdmType.FullName));
		}
	}

	internal static ParameterDirection ParameterModeToParameterDirection(ParameterMode mode)
	{
		return mode switch
		{
			ParameterMode.In => ParameterDirection.Input, 
			ParameterMode.InOut => ParameterDirection.InputOutput, 
			ParameterMode.Out => ParameterDirection.Output, 
			ParameterMode.ReturnValue => ParameterDirection.ReturnValue, 
			_ => (ParameterDirection)0, 
		};
	}

	internal static bool DoesMemberExist(StructuralType type, EdmMember member)
	{
		foreach (EdmMember member2 in type.Members)
		{
			if (member2.Equals(member))
			{
				return true;
			}
		}
		return false;
	}

	internal static bool IsNonRefSimpleMember(EdmMember member)
	{
		if (member.TypeUsage.EdmType.BuiltInTypeKind != BuiltInTypeKind.PrimitiveType)
		{
			return member.TypeUsage.EdmType.BuiltInTypeKind == BuiltInTypeKind.EnumType;
		}
		return true;
	}

	internal static bool HasDiscreteDomain(EdmType edmType)
	{
		if (edmType is PrimitiveType primitiveType)
		{
			return primitiveType.PrimitiveTypeKind == PrimitiveTypeKind.Boolean;
		}
		return false;
	}

	internal static EntityType GetEntityTypeForEnd(AssociationEndMember end)
	{
		return (EntityType)((RefType)end.TypeUsage.EdmType).ElementType;
	}

	internal static EntitySet GetEntitySetAtEnd(AssociationSet associationSet, AssociationEndMember endMember)
	{
		return associationSet.AssociationSetEnds[endMember.Name].EntitySet;
	}

	internal static AssociationEndMember GetOtherAssociationEnd(AssociationEndMember endMember)
	{
		ReadOnlyMetadataCollection<EdmMember> members = endMember.DeclaringType.Members;
		EdmMember edmMember = members[0];
		if (endMember != edmMember)
		{
			return (AssociationEndMember)edmMember;
		}
		return (AssociationEndMember)members[1];
	}

	internal static bool IsEveryOtherEndAtLeastOne(AssociationSet associationSet, AssociationEndMember member)
	{
		foreach (AssociationSetEnd associationSetEnd in associationSet.AssociationSetEnds)
		{
			AssociationEndMember correspondingAssociationEndMember = associationSetEnd.CorrespondingAssociationEndMember;
			if (!correspondingAssociationEndMember.Equals(member) && GetLowerBoundOfMultiplicity(correspondingAssociationEndMember.RelationshipMultiplicity) == 0)
			{
				return false;
			}
		}
		return true;
	}

	internal static bool IsAssociationValidForEntityType(AssociationSetEnd toEnd, EntityType type)
	{
		return GetEntityTypeForEnd(GetOppositeEnd(toEnd).CorrespondingAssociationEndMember).IsAssignableFrom(type);
	}

	internal static AssociationSetEnd GetOppositeEnd(AssociationSetEnd end)
	{
		return end.ParentAssociationSet.AssociationSetEnds.Where((AssociationSetEnd e) => !e.EdmEquals(end)).Single();
	}

	internal static bool IsComposable(EdmFunction function)
	{
		if (function.MetadataProperties.TryGetValue("IsComposableAttribute", ignoreCase: false, out var item))
		{
			return (bool)item.Value;
		}
		return !function.IsFunctionImport;
	}

	internal static bool IsMemberNullable(EdmMember member)
	{
		if (Helper.IsEdmProperty(member))
		{
			return ((EdmProperty)member).Nullable;
		}
		return false;
	}

	internal static IEnumerable<EntitySet> GetInfluencingEntitySetsForTable(EntitySet table, MetadataWorkspace workspace)
	{
		ItemCollection collection = null;
		workspace.TryGetItemCollection(DataSpace.CSSpace, out collection);
		return (from map in MappingMetadataHelper.GetEntityContainerMap((StorageMappingItemCollection)collection, table.EntityContainer).EntitySetMaps
			where map.TypeMappings.Any((TypeMapping typeMap) => typeMap.MappingFragments.Any((MappingFragment mappingFrag) => mappingFrag.TableSet.EdmEquals(table)))
			select map into m
			select m.Set).Cast<EntitySet>().Distinct();
	}

	internal static IEnumerable<EdmType> GetTypeAndSubtypesOf(EdmType type, MetadataWorkspace workspace, bool includeAbstractTypes)
	{
		return GetTypeAndSubtypesOf(type, workspace.GetItemCollection(DataSpace.CSpace), includeAbstractTypes);
	}

	internal static IEnumerable<EdmType> GetTypeAndSubtypesOf(EdmType type, ItemCollection itemCollection, bool includeAbstractTypes)
	{
		if (Helper.IsRefType(type))
		{
			type = ((RefType)type).ElementType;
		}
		if (includeAbstractTypes || !type.Abstract)
		{
			yield return type;
		}
		foreach (EdmType item in GetTypeAndSubtypesOf<EntityType>(type, itemCollection, includeAbstractTypes))
		{
			yield return item;
		}
		foreach (EdmType item2 in GetTypeAndSubtypesOf<ComplexType>(type, itemCollection, includeAbstractTypes))
		{
			yield return item2;
		}
	}

	private static IEnumerable<EdmType> GetTypeAndSubtypesOf<T_EdmType>(EdmType type, ItemCollection itemCollection, bool includeAbstractTypes) where T_EdmType : EdmType
	{
		if (!(type is T_EdmType specificType))
		{
			yield break;
		}
		IEnumerable<T_EdmType> items = itemCollection.GetItems<T_EdmType>();
		foreach (T_EdmType item in items)
		{
			if (!specificType.Equals(item) && Helper.IsSubtypeOf(item, specificType) && (includeAbstractTypes || !item.Abstract))
			{
				yield return item;
			}
		}
	}

	internal static IEnumerable<EdmType> GetTypeAndParentTypesOf(EdmType type, bool includeAbstractTypes)
	{
		if (Helper.IsRefType(type))
		{
			type = ((RefType)type).ElementType;
		}
		for (EdmType specificType = type; specificType != null; specificType = specificType.BaseType as EntityType)
		{
			if (includeAbstractTypes || !specificType.Abstract)
			{
				yield return specificType;
			}
		}
	}

	internal static Dictionary<EntityType, Set<EntityType>> BuildUndirectedGraphOfTypes(EdmItemCollection edmItemCollection)
	{
		Dictionary<EntityType, Set<EntityType>> dictionary = new Dictionary<EntityType, Set<EntityType>>();
		foreach (EntityType item in (IEnumerable<EntityType>)edmItemCollection.GetItems<EntityType>())
		{
			if (item.BaseType != null)
			{
				EntityType entityType = item.BaseType as EntityType;
				AddDirectedEdgeBetweenEntityTypes(dictionary, item, entityType);
				AddDirectedEdgeBetweenEntityTypes(dictionary, entityType, item);
			}
		}
		return dictionary;
	}

	internal static bool IsParentOf(EntityType a, EntityType b)
	{
		for (EntityType entityType = b.BaseType as EntityType; entityType != null; entityType = entityType.BaseType as EntityType)
		{
			if (entityType.EdmEquals(a))
			{
				return true;
			}
		}
		return false;
	}

	private static void AddDirectedEdgeBetweenEntityTypes(Dictionary<EntityType, Set<EntityType>> graph, EntityType a, EntityType b)
	{
		Set<EntityType> set;
		if (graph.ContainsKey(a))
		{
			set = graph[a];
		}
		else
		{
			set = new Set<EntityType>();
			graph.Add(a, set);
		}
		set.Add(b);
	}

	internal static bool DoesEndKeySubsumeAssociationSetKey(AssociationSet assocSet, AssociationEndMember thisEnd, HashSet<Pair<EdmMember, EntityType>> associationkeys)
	{
		AssociationType elementType = assocSet.ElementType;
		EntityType thisEndsEntityType = (EntityType)((RefType)thisEnd.TypeUsage.EdmType).ElementType;
		HashSet<Pair<EdmMember, EntityType>> other = new HashSet<Pair<EdmMember, EntityType>>(thisEndsEntityType.KeyMembers.Select((EdmMember edmMember) => new Pair<EdmMember, EntityType>(edmMember, thisEndsEntityType)));
		foreach (ReferentialConstraint referentialConstraint in elementType.ReferentialConstraints)
		{
			IEnumerable<EdmMember> enumerable;
			EntityType second;
			if (thisEnd.Equals(referentialConstraint.ToRole))
			{
				enumerable = Helpers.AsSuperTypeList<EdmProperty, EdmMember>(referentialConstraint.FromProperties);
				second = (EntityType)((RefType)referentialConstraint.FromRole.TypeUsage.EdmType).ElementType;
			}
			else
			{
				if (!thisEnd.Equals(referentialConstraint.FromRole))
				{
					continue;
				}
				enumerable = Helpers.AsSuperTypeList<EdmProperty, EdmMember>(referentialConstraint.ToProperties);
				second = (EntityType)((RefType)referentialConstraint.ToRole.TypeUsage.EdmType).ElementType;
			}
			foreach (EdmMember item in enumerable)
			{
				associationkeys.Remove(new Pair<EdmMember, EntityType>(item, second));
			}
		}
		return associationkeys.IsSubsetOf(other);
	}

	internal static bool DoesEndFormKey(AssociationSet associationSet, AssociationEndMember end)
	{
		foreach (AssociationEndMember member in associationSet.ElementType.Members)
		{
			if (!member.Equals(end) && member.RelationshipMultiplicity == RelationshipMultiplicity.Many)
			{
				return false;
			}
		}
		return true;
	}

	internal static bool IsExtentAtSomeRelationshipEnd(AssociationSet relationshipSet, EntitySetBase extent)
	{
		if (Helper.IsEntitySet(extent))
		{
			return GetSomeEndForEntitySet(relationshipSet, extent) != null;
		}
		return false;
	}

	internal static AssociationEndMember GetSomeEndForEntitySet(AssociationSet associationSet, EntitySetBase entitySet)
	{
		foreach (AssociationSetEnd associationSetEnd in associationSet.AssociationSetEnds)
		{
			if (associationSetEnd.EntitySet.Equals(entitySet))
			{
				return associationSetEnd.CorrespondingAssociationEndMember;
			}
		}
		return null;
	}

	internal static List<AssociationSet> GetAssociationsForEntitySets(EntitySet entitySet1, EntitySet entitySet2)
	{
		List<AssociationSet> list = new List<AssociationSet>();
		foreach (EntitySetBase baseEntitySet in entitySet1.EntityContainer.BaseEntitySets)
		{
			if (Helper.IsRelationshipSet(baseEntitySet))
			{
				AssociationSet associationSet = (AssociationSet)baseEntitySet;
				if (IsExtentAtSomeRelationshipEnd(associationSet, entitySet1) && IsExtentAtSomeRelationshipEnd(associationSet, entitySet2))
				{
					list.Add(associationSet);
				}
			}
		}
		return list;
	}

	internal static List<AssociationSet> GetAssociationsForEntitySet(EntitySetBase entitySet)
	{
		List<AssociationSet> list = new List<AssociationSet>();
		foreach (EntitySetBase baseEntitySet in entitySet.EntityContainer.BaseEntitySets)
		{
			if (Helper.IsRelationshipSet(baseEntitySet))
			{
				AssociationSet associationSet = (AssociationSet)baseEntitySet;
				if (IsExtentAtSomeRelationshipEnd(associationSet, entitySet))
				{
					list.Add(associationSet);
				}
			}
		}
		return list;
	}

	internal static bool IsSuperTypeOf(EdmType superType, EdmType subType)
	{
		for (EdmType edmType = subType; edmType != null; edmType = edmType.BaseType)
		{
			if (edmType.Equals(superType))
			{
				return true;
			}
		}
		return false;
	}

	internal static bool IsPartOfEntityTypeKey(EdmMember member)
	{
		if (Helper.IsEntityType(member.DeclaringType) && Helper.IsEdmProperty(member))
		{
			return ((EntityType)member.DeclaringType).KeyMembers.Contains(member);
		}
		return false;
	}

	internal static TypeUsage GetElementType(TypeUsage typeUsage)
	{
		if (BuiltInTypeKind.CollectionType == typeUsage.EdmType.BuiltInTypeKind)
		{
			return GetElementType(((CollectionType)typeUsage.EdmType).TypeUsage);
		}
		return typeUsage;
	}

	internal static int GetLowerBoundOfMultiplicity(RelationshipMultiplicity multiplicity)
	{
		if (multiplicity == RelationshipMultiplicity.Many || multiplicity == RelationshipMultiplicity.ZeroOrOne)
		{
			return 0;
		}
		return 1;
	}

	internal static int? GetUpperBoundOfMultiplicity(RelationshipMultiplicity multiplicity)
	{
		if (multiplicity == RelationshipMultiplicity.One || multiplicity == RelationshipMultiplicity.ZeroOrOne)
		{
			return 1;
		}
		return null;
	}

	internal static Set<EdmMember> GetConcurrencyMembersForTypeHierarchy(EntityTypeBase superType, EdmItemCollection edmItemCollection)
	{
		Set<EdmMember> set = new Set<EdmMember>();
		foreach (StructuralType item in GetTypeAndSubtypesOf(superType, edmItemCollection, includeAbstractTypes: true))
		{
			foreach (EdmMember member in item.Members)
			{
				if (GetConcurrencyMode(member) == ConcurrencyMode.Fixed)
				{
					set.Add(member);
				}
			}
		}
		return set;
	}

	internal static ConcurrencyMode GetConcurrencyMode(EdmMember member)
	{
		return GetConcurrencyMode(member.TypeUsage);
	}

	internal static ConcurrencyMode GetConcurrencyMode(TypeUsage typeUsage)
	{
		if (typeUsage.Facets.TryGetValue("ConcurrencyMode", ignoreCase: false, out var item) && item.Value != null)
		{
			return (ConcurrencyMode)item.Value;
		}
		return ConcurrencyMode.None;
	}

	internal static StoreGeneratedPattern GetStoreGeneratedPattern(EdmMember member)
	{
		if (member.TypeUsage.Facets.TryGetValue("StoreGeneratedPattern", ignoreCase: false, out var item) && item.Value != null)
		{
			return (StoreGeneratedPattern)item.Value;
		}
		return StoreGeneratedPattern.None;
	}

	internal static bool CheckIfAllErrorsAreWarnings(IList<EdmSchemaError> schemaErrors)
	{
		int count = schemaErrors.Count;
		for (int i = 0; i < count; i++)
		{
			if (schemaErrors[i].Severity != 0)
			{
				return false;
			}
		}
		return true;
	}

	internal static HashAlgorithm CreateMetadataHashAlgorithm(double schemaVersion)
	{
		if (schemaVersion < 2.0)
		{
			return new MD5CryptoServiceProvider();
		}
		return CreateSHA256HashAlgorithm();
	}

	internal static SHA256 CreateSHA256HashAlgorithm()
	{
		try
		{
			return new SHA256CryptoServiceProvider();
		}
		catch (PlatformNotSupportedException)
		{
			return new SHA256Managed();
		}
	}

	internal static TypeUsage ConvertStoreTypeUsageToEdmTypeUsage(TypeUsage storeTypeUsage)
	{
		return storeTypeUsage.ModelTypeUsage.ShallowCopy(FacetValues.NullFacetValues);
	}

	internal static byte GetPrecision(this TypeUsage type)
	{
		return type.GetFacetValue<byte>("Precision");
	}

	internal static byte GetScale(this TypeUsage type)
	{
		return type.GetFacetValue<byte>("Scale");
	}

	internal static int GetMaxLength(this TypeUsage type)
	{
		return type.GetFacetValue<int>("MaxLength");
	}

	internal static T GetFacetValue<T>(this TypeUsage type, string facetName)
	{
		return (T)type.Facets[facetName].Value;
	}

	internal static NavigationPropertyAccessor GetNavigationPropertyAccessor(EntityType sourceEntityType, AssociationEndMember sourceMember, AssociationEndMember targetMember)
	{
		return GetNavigationPropertyAccessor(sourceEntityType, sourceMember.DeclaringType.FullName, sourceMember.Name, targetMember.Name);
	}

	internal static NavigationPropertyAccessor GetNavigationPropertyAccessor(EntityType entityType, string relationshipType, string fromName, string toName)
	{
		if (entityType.TryGetNavigationProperty(relationshipType, fromName, toName, out var navigationProperty))
		{
			return navigationProperty.Accessor;
		}
		return NavigationPropertyAccessor.NoNavigationProperty;
	}
}
