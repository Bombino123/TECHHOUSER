using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.Entity.Core.Common.Utils;
using System.Data.Entity.Core.Mapping;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Core.Objects.Internal;
using System.Data.Entity.Resources;
using System.Data.Entity.Utilities;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;

namespace System.Data.Entity.Core.Objects.DataClasses;

[Serializable]
public class RelationshipManager
{
	private IEntityWithRelationships _owner;

	private List<RelatedEnd> _relationships;

	[NonSerialized]
	private bool _nodeVisited;

	[NonSerialized]
	private IEntityWrapper _wrappedOwner;

	[NonSerialized]
	private EntityWrapperFactory _entityWrapperFactory;

	[NonSerialized]
	private ExpensiveOSpaceLoader _expensiveLoader;

	internal IEnumerable<RelatedEnd> Relationships
	{
		get
		{
			EnsureRelationshipsInitialized();
			return _relationships.ToArray();
		}
	}

	internal bool NodeVisited
	{
		get
		{
			return _nodeVisited;
		}
		set
		{
			_nodeVisited = value;
		}
	}

	internal IEntityWrapper WrappedOwner
	{
		get
		{
			if (_wrappedOwner == null)
			{
				_wrappedOwner = EntityWrapperFactory.CreateNewWrapper(_owner, null);
			}
			return _wrappedOwner;
		}
	}

	internal virtual EntityWrapperFactory EntityWrapperFactory => _entityWrapperFactory;

	internal bool HasRelationships => _relationships != null;

	private RelationshipManager()
	{
		_entityWrapperFactory = new EntityWrapperFactory();
		_expensiveLoader = new ExpensiveOSpaceLoader();
	}

	internal RelationshipManager(ExpensiveOSpaceLoader expensiveLoader)
	{
		_entityWrapperFactory = new EntityWrapperFactory();
		_expensiveLoader = expensiveLoader ?? new ExpensiveOSpaceLoader();
	}

	internal void SetExpensiveLoader(ExpensiveOSpaceLoader loader)
	{
		_expensiveLoader = loader;
	}

	private void EnsureRelationshipsInitialized()
	{
		if (_relationships == null)
		{
			_relationships = new List<RelatedEnd>();
		}
	}

	public static RelationshipManager Create(IEntityWithRelationships owner)
	{
		Check.NotNull(owner, "owner");
		return new RelationshipManager
		{
			_owner = owner
		};
	}

	internal static RelationshipManager Create()
	{
		return new RelationshipManager();
	}

	internal void SetWrappedOwner(IEntityWrapper wrappedOwner, object expectedOwner)
	{
		_wrappedOwner = wrappedOwner;
		if (_owner != null && expectedOwner != _owner)
		{
			throw new InvalidOperationException(Strings.RelationshipManager_InvalidRelationshipManagerOwner);
		}
		if (_relationships == null)
		{
			return;
		}
		foreach (RelatedEnd relationship in _relationships)
		{
			relationship.SetWrappedOwner(wrappedOwner);
		}
	}

	internal EntityCollection<TTargetEntity> GetRelatedCollection<TSourceEntity, TTargetEntity>(AssociationEndMember sourceMember, AssociationEndMember targetMember, NavigationPropertyAccessor sourceAccessor, NavigationPropertyAccessor targetAccessor, RelatedEnd existingRelatedEnd) where TSourceEntity : class where TTargetEntity : class
	{
		string fullName = sourceMember.DeclaringType.FullName;
		string name = targetMember.Name;
		RelationshipMultiplicity relationshipMultiplicity = sourceMember.RelationshipMultiplicity;
		TryGetCachedRelatedEnd(fullName, name, out var relatedEnd);
		EntityCollection<TTargetEntity> entityCollection = relatedEnd as EntityCollection<TTargetEntity>;
		if (existingRelatedEnd == null)
		{
			if (relatedEnd != null)
			{
				return entityCollection;
			}
			RelationshipNavigation navigation = new RelationshipNavigation((AssociationType)sourceMember.DeclaringType, sourceMember.Name, targetMember.Name, sourceAccessor, targetAccessor);
			return CreateRelatedEnd<TSourceEntity, TTargetEntity>(navigation, relationshipMultiplicity, RelationshipMultiplicity.Many, existingRelatedEnd) as EntityCollection<TTargetEntity>;
		}
		if (relatedEnd != null)
		{
			_relationships.Remove(relatedEnd);
		}
		RelationshipNavigation navigation2 = new RelationshipNavigation((AssociationType)sourceMember.DeclaringType, sourceMember.Name, targetMember.Name, sourceAccessor, targetAccessor);
		EntityCollection<TTargetEntity> entityCollection2 = CreateRelatedEnd<TSourceEntity, TTargetEntity>(navigation2, relationshipMultiplicity, RelationshipMultiplicity.Many, existingRelatedEnd) as EntityCollection<TTargetEntity>;
		if (entityCollection2 != null)
		{
			bool flag = true;
			try
			{
				RemergeCollections(entityCollection, entityCollection2);
				flag = false;
			}
			finally
			{
				if (flag && relatedEnd != null)
				{
					_relationships.Remove(entityCollection2);
					_relationships.Add(relatedEnd);
				}
			}
		}
		return entityCollection2;
	}

	private static void RemergeCollections<TTargetEntity>(EntityCollection<TTargetEntity> previousCollection, EntityCollection<TTargetEntity> collection) where TTargetEntity : class
	{
		int num = 0;
		List<IEntityWrapper> list = new List<IEntityWrapper>(collection.CountInternal);
		foreach (IEntityWrapper wrappedEntity in collection.GetWrappedEntities())
		{
			list.Add(wrappedEntity);
		}
		foreach (IEntityWrapper item in list)
		{
			bool flag = true;
			if (previousCollection != null && previousCollection.ContainsEntity(item))
			{
				num++;
				flag = false;
			}
			if (flag)
			{
				collection.Remove(item, preserveForeignKey: false);
				collection.Add(item);
			}
		}
		if (previousCollection != null && num != previousCollection.CountInternal)
		{
			throw new InvalidOperationException(Strings.Collections_UnableToMergeCollections);
		}
	}

	internal EntityReference<TTargetEntity> GetRelatedReference<TSourceEntity, TTargetEntity>(AssociationEndMember sourceMember, AssociationEndMember targetMember, NavigationPropertyAccessor sourceAccessor, NavigationPropertyAccessor targetAccessor, RelatedEnd existingRelatedEnd) where TSourceEntity : class where TTargetEntity : class
	{
		string fullName = sourceMember.DeclaringType.FullName;
		string name = targetMember.Name;
		RelationshipMultiplicity relationshipMultiplicity = sourceMember.RelationshipMultiplicity;
		if (TryGetCachedRelatedEnd(fullName, name, out var relatedEnd))
		{
			return relatedEnd as EntityReference<TTargetEntity>;
		}
		RelationshipNavigation navigation = new RelationshipNavigation((AssociationType)sourceMember.DeclaringType, sourceMember.Name, targetMember.Name, sourceAccessor, targetAccessor);
		return CreateRelatedEnd<TSourceEntity, TTargetEntity>(navigation, relationshipMultiplicity, RelationshipMultiplicity.One, existingRelatedEnd) as EntityReference<TTargetEntity>;
	}

	internal RelatedEnd GetRelatedEnd(string navigationProperty, bool throwArgumentException = false)
	{
		IEntityWrapper wrappedOwner = WrappedOwner;
		EntityType item = wrappedOwner.Context.MetadataWorkspace.GetItem<EntityType>(wrappedOwner.IdentityType.FullNameWithNesting(), DataSpace.OSpace);
		if (!wrappedOwner.Context.Perspective.TryGetMember(item, navigationProperty, ignoreCase: false, out var outMember) || !(outMember is NavigationProperty))
		{
			string message = Strings.RelationshipManager_NavigationPropertyNotFound(navigationProperty);
			throw throwArgumentException ? ((SystemException)new ArgumentException(message)) : ((SystemException)new InvalidOperationException(message));
		}
		NavigationProperty navigationProperty2 = (NavigationProperty)outMember;
		return GetRelatedEndInternal(navigationProperty2.RelationshipType.FullName, navigationProperty2.ToEndMember.Name);
	}

	public IRelatedEnd GetRelatedEnd(string relationshipName, string targetRoleName)
	{
		return GetRelatedEndInternal(PrependNamespaceToRelationshipName(relationshipName), targetRoleName);
	}

	internal RelatedEnd GetRelatedEndInternal(string relationshipName, string targetRoleName)
	{
		IEntityWrapper wrappedOwner = WrappedOwner;
		if (wrappedOwner.Context == null && wrappedOwner.RequiresRelationshipChangeTracking)
		{
			throw new InvalidOperationException(Strings.RelationshipManager_CannotGetRelatEndForDetachedPocoEntity);
		}
		AssociationType relationshipType = GetRelationshipType(relationshipName);
		return GetRelatedEndInternal(relationshipName, targetRoleName, null, relationshipType);
	}

	private RelatedEnd GetRelatedEndInternal(string relationshipName, string targetRoleName, RelatedEnd existingRelatedEnd, AssociationType relationship)
	{
		GetAssociationEnds(relationship, targetRoleName, out var sourceEnd, out var targetEnd);
		Type clrType = MetadataHelper.GetEntityTypeForEnd(sourceEnd).ClrType;
		IEntityWrapper wrappedOwner = WrappedOwner;
		if (!clrType.IsAssignableFrom(wrappedOwner.IdentityType))
		{
			throw new InvalidOperationException(Strings.RelationshipManager_OwnerIsNotSourceType(wrappedOwner.IdentityType.FullName, clrType.FullName, sourceEnd.Name, relationshipName));
		}
		if (!VerifyRelationship(relationship, sourceEnd.Name))
		{
			return null;
		}
		return DelegateFactory.GetRelatedEnd(this, sourceEnd, targetEnd, existingRelatedEnd);
	}

	internal RelatedEnd GetRelatedEndInternal(AssociationType csAssociationType, AssociationEndMember csTargetEnd)
	{
		IEntityWrapper wrappedOwner = WrappedOwner;
		if (wrappedOwner.Context == null && wrappedOwner.RequiresRelationshipChangeTracking)
		{
			throw new InvalidOperationException(Strings.RelationshipManager_CannotGetRelatEndForDetachedPocoEntity);
		}
		AssociationType relationshipType = GetRelationshipType(csAssociationType);
		GetAssociationEnds(relationshipType, csTargetEnd.Name, out var sourceEnd, out var targetEnd);
		Type clrType = MetadataHelper.GetEntityTypeForEnd(sourceEnd).ClrType;
		if (!clrType.IsAssignableFrom(wrappedOwner.IdentityType))
		{
			throw new InvalidOperationException(Strings.RelationshipManager_OwnerIsNotSourceType(wrappedOwner.IdentityType.FullName, clrType.FullName, sourceEnd.Name, csAssociationType.FullName));
		}
		if (!VerifyRelationship(relationshipType, csAssociationType, sourceEnd.Name))
		{
			return null;
		}
		return DelegateFactory.GetRelatedEnd(this, sourceEnd, targetEnd, null);
	}

	private static void GetAssociationEnds(AssociationType associationType, string targetRoleName, out AssociationEndMember sourceEnd, out AssociationEndMember targetEnd)
	{
		targetEnd = associationType.TargetEnd;
		if (targetEnd.Identity != targetRoleName)
		{
			sourceEnd = targetEnd;
			targetEnd = associationType.SourceEnd;
			if (targetEnd.Identity != targetRoleName)
			{
				throw new InvalidOperationException(Strings.RelationshipManager_InvalidTargetRole(associationType.FullName, targetRoleName));
			}
		}
		else
		{
			sourceEnd = associationType.SourceEnd;
		}
	}

	[Browsable(false)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public void InitializeRelatedReference<TTargetEntity>(string relationshipName, string targetRoleName, EntityReference<TTargetEntity> entityReference) where TTargetEntity : class
	{
		Check.NotNull(relationshipName, "relationshipName");
		Check.NotNull(targetRoleName, "targetRoleName");
		Check.NotNull(entityReference, "entityReference");
		if (entityReference.WrappedOwner.Entity != null)
		{
			throw new InvalidOperationException(Strings.RelationshipManager_ReferenceAlreadyInitialized(Strings.RelationshipManager_InitializeIsForDeserialization));
		}
		IEntityWrapper wrappedOwner = WrappedOwner;
		if (wrappedOwner.Context != null && wrappedOwner.MergeOption != MergeOption.NoTracking)
		{
			throw new InvalidOperationException(Strings.RelationshipManager_RelationshipManagerAttached(Strings.RelationshipManager_InitializeIsForDeserialization));
		}
		relationshipName = PrependNamespaceToRelationshipName(relationshipName);
		AssociationType relationshipType = GetRelationshipType(relationshipName);
		if (TryGetCachedRelatedEnd(relationshipName, targetRoleName, out var relatedEnd))
		{
			if (!relatedEnd.IsEmpty())
			{
				entityReference.InitializeWithValue(relatedEnd);
			}
			_relationships.Remove(relatedEnd);
		}
		if (!(GetRelatedEndInternal(relationshipName, targetRoleName, entityReference, relationshipType) is EntityReference<TTargetEntity>))
		{
			throw new InvalidOperationException(Strings.EntityReference_ExpectedReferenceGotCollection(typeof(TTargetEntity).Name, targetRoleName, relationshipName));
		}
	}

	[Browsable(false)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public void InitializeRelatedCollection<TTargetEntity>(string relationshipName, string targetRoleName, EntityCollection<TTargetEntity> entityCollection) where TTargetEntity : class
	{
		Check.NotNull(relationshipName, "relationshipName");
		Check.NotNull(targetRoleName, "targetRoleName");
		Check.NotNull(entityCollection, "entityCollection");
		if (entityCollection.WrappedOwner.Entity != null)
		{
			throw new InvalidOperationException(Strings.RelationshipManager_CollectionAlreadyInitialized(Strings.RelationshipManager_CollectionInitializeIsForDeserialization));
		}
		IEntityWrapper wrappedOwner = WrappedOwner;
		if (wrappedOwner.Context != null && wrappedOwner.MergeOption != MergeOption.NoTracking)
		{
			throw new InvalidOperationException(Strings.RelationshipManager_CollectionRelationshipManagerAttached(Strings.RelationshipManager_CollectionInitializeIsForDeserialization));
		}
		relationshipName = PrependNamespaceToRelationshipName(relationshipName);
		AssociationType relationshipType = GetRelationshipType(relationshipName);
		if (!(GetRelatedEndInternal(relationshipName, targetRoleName, entityCollection, relationshipType) is EntityCollection<TTargetEntity>))
		{
			throw new InvalidOperationException(Strings.Collections_ExpectedCollectionGotReference(typeof(TTargetEntity).Name, targetRoleName, relationshipName));
		}
	}

	internal string PrependNamespaceToRelationshipName(string relationshipName)
	{
		if (!relationshipName.Contains("."))
		{
			if (EntityProxyFactory.TryGetAssociationTypeFromProxyInfo(WrappedOwner, relationshipName, out var associationType))
			{
				return associationType.FullName;
			}
			if (_relationships != null)
			{
				string text = _relationships.Select((RelatedEnd r) => r.RelationshipName).FirstOrDefault((string n) => n.Substring(n.LastIndexOf('.') + 1) == relationshipName);
				if (text != null)
				{
					return text;
				}
			}
			string text2 = WrappedOwner.IdentityType.FullNameWithNesting();
			ObjectItemCollection objectItemCollection = GetObjectItemCollection(WrappedOwner);
			EdmType value = null;
			if (objectItemCollection != null)
			{
				objectItemCollection.TryGetItem<EdmType>(text2, out value);
			}
			else
			{
				_expensiveLoader.LoadTypesExpensiveWay(WrappedOwner.IdentityType.Assembly())?.TryGetValue(text2, out value);
			}
			if (value is ClrEntityType clrEntityType)
			{
				return clrEntityType.CSpaceNamespaceName + "." + relationshipName;
			}
		}
		return relationshipName;
	}

	private static ObjectItemCollection GetObjectItemCollection(IEntityWrapper wrappedOwner)
	{
		if (wrappedOwner.Context != null)
		{
			return (ObjectItemCollection)wrappedOwner.Context.MetadataWorkspace.GetItemCollection(DataSpace.OSpace);
		}
		return null;
	}

	private bool TryGetOwnerEntityType(out EntityType entityType)
	{
		if (TryGetObjectMappingItemCollection(WrappedOwner, out var collection) && collection.TryGetMap(WrappedOwner.IdentityType.FullNameWithNesting(), DataSpace.OSpace, out var map))
		{
			ObjectTypeMapping objectTypeMapping = (ObjectTypeMapping)map;
			if (Helper.IsEntityType(objectTypeMapping.EdmType))
			{
				entityType = (EntityType)objectTypeMapping.EdmType;
				return true;
			}
		}
		entityType = null;
		return false;
	}

	private static bool TryGetObjectMappingItemCollection(IEntityWrapper wrappedOwner, out DefaultObjectMappingItemCollection collection)
	{
		if (wrappedOwner.Context != null && wrappedOwner.Context.MetadataWorkspace != null)
		{
			collection = (DefaultObjectMappingItemCollection)wrappedOwner.Context.MetadataWorkspace.GetItemCollection(DataSpace.OCSpace);
			return collection != null;
		}
		collection = null;
		return false;
	}

	internal AssociationType GetRelationshipType(AssociationType csAssociationType)
	{
		MetadataWorkspace metadataWorkspace = WrappedOwner.Context.MetadataWorkspace;
		if (metadataWorkspace != null)
		{
			return metadataWorkspace.MetadataOptimization.GetOSpaceAssociationType(csAssociationType, () => GetRelationshipType(csAssociationType.FullName));
		}
		return GetRelationshipType(csAssociationType.FullName);
	}

	internal AssociationType GetRelationshipType(string relationshipName)
	{
		AssociationType associationType = null;
		ObjectItemCollection objectItemCollection = GetObjectItemCollection(WrappedOwner);
		if (objectItemCollection != null)
		{
			associationType = objectItemCollection.GetRelationshipType(relationshipName);
		}
		if (associationType == null)
		{
			EntityProxyFactory.TryGetAssociationTypeFromProxyInfo(WrappedOwner, relationshipName, out associationType);
		}
		if (associationType == null && _relationships != null)
		{
			associationType = (from e in _relationships
				where e.RelationshipName == relationshipName
				select e.RelationMetadata).OfType<AssociationType>().FirstOrDefault();
		}
		if (associationType == null)
		{
			associationType = _expensiveLoader.GetRelationshipTypeExpensiveWay(WrappedOwner.IdentityType, relationshipName);
		}
		if (associationType == null)
		{
			throw UnableToGetMetadata(WrappedOwner, relationshipName);
		}
		return associationType;
	}

	internal static Exception UnableToGetMetadata(IEntityWrapper wrappedOwner, string relationshipName)
	{
		ArgumentException ex = new ArgumentException(Strings.RelationshipManager_UnableToFindRelationshipTypeInMetadata(relationshipName), "relationshipName");
		if (EntityProxyFactory.IsProxyType(wrappedOwner.Entity.GetType()))
		{
			return new InvalidOperationException(Strings.EntityProxyTypeInfo_ProxyMetadataIsUnavailable(wrappedOwner.IdentityType.FullName), ex);
		}
		return ex;
	}

	private static IEnumerable<AssociationEndMember> GetAllTargetEnds(EntityType ownerEntityType, EntitySet ownerEntitySet)
	{
		foreach (AssociationSet assocSet in ownerEntitySet.AssociationSets)
		{
			if (assocSet.ElementType.AssociationEndMembers[1].GetEntityType().IsAssignableFrom(ownerEntityType))
			{
				yield return assocSet.ElementType.AssociationEndMembers[0];
			}
			if (assocSet.ElementType.AssociationEndMembers[0].GetEntityType().IsAssignableFrom(ownerEntityType))
			{
				yield return assocSet.ElementType.AssociationEndMembers[1];
			}
		}
	}

	private IEnumerable<AssociationEndMember> GetAllTargetEnds(Type entityClrType)
	{
		ObjectItemCollection objectItemCollection = GetObjectItemCollection(WrappedOwner);
		IEnumerable<AssociationType> enumerable;
		if (objectItemCollection != null)
		{
			enumerable = objectItemCollection.GetItems<AssociationType>();
		}
		else
		{
			enumerable = EntityProxyFactory.TryGetAllAssociationTypesFromProxyInfo(WrappedOwner);
			if (enumerable == null)
			{
				enumerable = _expensiveLoader.GetAllRelationshipTypesExpensiveWay(entityClrType.Assembly());
			}
		}
		foreach (AssociationType association in enumerable)
		{
			if (association.AssociationEndMembers[0].TypeUsage.EdmType is RefType refType && refType.ElementType.ClrType.IsAssignableFrom(entityClrType))
			{
				yield return association.AssociationEndMembers[1];
			}
			if (association.AssociationEndMembers[1].TypeUsage.EdmType is RefType refType2 && refType2.ElementType.ClrType.IsAssignableFrom(entityClrType))
			{
				yield return association.AssociationEndMembers[0];
			}
		}
	}

	private bool VerifyRelationship(AssociationType relationship, string sourceEndName)
	{
		IEntityWrapper wrappedOwner = WrappedOwner;
		if (wrappedOwner.Context == null)
		{
			return true;
		}
		EntityKey entityKey = wrappedOwner.EntityKey;
		if (entityKey == null)
		{
			return true;
		}
		return VerifyRelationship(wrappedOwner, entityKey, relationship, sourceEndName);
	}

	private bool VerifyRelationship(AssociationType osAssociationType, AssociationType csAssociationType, string sourceEndName)
	{
		IEntityWrapper wrappedOwner = WrappedOwner;
		if (wrappedOwner.Context == null)
		{
			return true;
		}
		EntityKey entityKey = wrappedOwner.EntityKey;
		if (entityKey == null)
		{
			return true;
		}
		if (osAssociationType.Index < 0)
		{
			return VerifyRelationship(wrappedOwner, entityKey, osAssociationType, sourceEndName);
		}
		if (wrappedOwner.Context.MetadataWorkspace.MetadataOptimization.FindCSpaceAssociationSet(csAssociationType, sourceEndName, entityKey.EntitySetName, entityKey.EntityContainerName, out var _) == null)
		{
			throw Error.Collections_NoRelationshipSetMatched(osAssociationType.FullName);
		}
		return true;
	}

	private static bool VerifyRelationship(IEntityWrapper wrappedOwner, EntityKey ownerKey, AssociationType relationship, string sourceEndName)
	{
		if (wrappedOwner.Context.Perspective.TryGetTypeByName(relationship.FullName, ignoreCase: false, out var typeUsage) && wrappedOwner.Context.MetadataWorkspace.MetadataOptimization.FindCSpaceAssociationSet((AssociationType)typeUsage.EdmType, sourceEndName, ownerKey.EntitySetName, ownerKey.EntityContainerName, out var _) == null)
		{
			throw Error.Collections_NoRelationshipSetMatched(relationship.FullName);
		}
		return true;
	}

	public EntityCollection<TTargetEntity> GetRelatedCollection<TTargetEntity>(string relationshipName, string targetRoleName) where TTargetEntity : class
	{
		return (GetRelatedEndInternal(PrependNamespaceToRelationshipName(relationshipName), targetRoleName) as EntityCollection<TTargetEntity>) ?? throw new InvalidOperationException(Strings.Collections_ExpectedCollectionGotReference(typeof(TTargetEntity).Name, targetRoleName, relationshipName));
	}

	public EntityReference<TTargetEntity> GetRelatedReference<TTargetEntity>(string relationshipName, string targetRoleName) where TTargetEntity : class
	{
		return (GetRelatedEndInternal(PrependNamespaceToRelationshipName(relationshipName), targetRoleName) as EntityReference<TTargetEntity>) ?? throw new InvalidOperationException(Strings.EntityReference_ExpectedReferenceGotCollection(typeof(TTargetEntity).Name, targetRoleName, relationshipName));
	}

	internal RelatedEnd GetRelatedEnd(RelationshipNavigation navigation, IRelationshipFixer relationshipFixer)
	{
		if (TryGetCachedRelatedEnd(navigation.RelationshipName, navigation.To, out var relatedEnd))
		{
			return relatedEnd;
		}
		return relationshipFixer.CreateSourceEnd(navigation, this);
	}

	internal RelatedEnd CreateRelatedEnd<TSourceEntity, TTargetEntity>(RelationshipNavigation navigation, RelationshipMultiplicity sourceRoleMultiplicity, RelationshipMultiplicity targetRoleMultiplicity, RelatedEnd existingRelatedEnd) where TSourceEntity : class where TTargetEntity : class
	{
		IRelationshipFixer relationshipFixer = new RelationshipFixer<TSourceEntity, TTargetEntity>(sourceRoleMultiplicity, targetRoleMultiplicity);
		RelatedEnd relatedEnd = null;
		IEntityWrapper wrappedOwner = WrappedOwner;
		switch (targetRoleMultiplicity)
		{
		case RelationshipMultiplicity.ZeroOrOne:
		case RelationshipMultiplicity.One:
			if (existingRelatedEnd != null)
			{
				existingRelatedEnd.InitializeRelatedEnd(wrappedOwner, navigation, relationshipFixer);
				relatedEnd = existingRelatedEnd;
			}
			else
			{
				relatedEnd = new EntityReference<TTargetEntity>(wrappedOwner, navigation, relationshipFixer);
			}
			break;
		case RelationshipMultiplicity.Many:
			if (existingRelatedEnd != null)
			{
				existingRelatedEnd.InitializeRelatedEnd(wrappedOwner, navigation, relationshipFixer);
				relatedEnd = existingRelatedEnd;
			}
			else
			{
				relatedEnd = new EntityCollection<TTargetEntity>(wrappedOwner, navigation, relationshipFixer);
			}
			break;
		default:
		{
			Type typeFromHandle = typeof(RelationshipMultiplicity);
			string name = typeFromHandle.Name;
			string name2 = typeFromHandle.Name;
			int num = (int)targetRoleMultiplicity;
			throw new ArgumentOutOfRangeException(name, Strings.ADP_InvalidEnumerationValue(name2, num.ToString(CultureInfo.InvariantCulture)));
		}
		}
		if (wrappedOwner.Context != null)
		{
			relatedEnd.AttachContext(wrappedOwner.Context, wrappedOwner.MergeOption);
		}
		EnsureRelationshipsInitialized();
		_relationships.Add(relatedEnd);
		return relatedEnd;
	}

	public IEnumerable<IRelatedEnd> GetAllRelatedEnds()
	{
		IEntityWrapper wrappedOwner = WrappedOwner;
		if (wrappedOwner.Context != null && wrappedOwner.Context.MetadataWorkspace != null && TryGetOwnerEntityType(out var entityType))
		{
			EntitySet entitySet = wrappedOwner.Context.GetEntitySet(wrappedOwner.EntityKey.EntitySetName, wrappedOwner.EntityKey.EntityContainerName);
			foreach (AssociationEndMember allTargetEnd in GetAllTargetEnds(entityType, entitySet))
			{
				yield return GetRelatedEnd(allTargetEnd.DeclaringType.FullName, allTargetEnd.Name);
			}
		}
		else
		{
			if (wrappedOwner.Entity == null)
			{
				yield break;
			}
			foreach (AssociationEndMember allTargetEnd2 in GetAllTargetEnds(wrappedOwner.IdentityType))
			{
				yield return GetRelatedEnd(allTargetEnd2.DeclaringType.FullName, allTargetEnd2.Name);
			}
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Browsable(false)]
	[OnSerializing]
	public void OnSerializing(StreamingContext context)
	{
		IEntityWrapper wrappedOwner = WrappedOwner;
		if (!(wrappedOwner.Entity is IEntityWithRelationships))
		{
			throw new InvalidOperationException(Strings.RelatedEnd_CannotSerialize("RelationshipManager"));
		}
		if (wrappedOwner.Context == null || wrappedOwner.MergeOption == MergeOption.NoTracking)
		{
			return;
		}
		foreach (RelatedEnd allRelatedEnd in GetAllRelatedEnds())
		{
			if (allRelatedEnd is EntityReference entityReference && entityReference.EntityKey != null)
			{
				entityReference.DetachedEntityKey = entityReference.EntityKey;
			}
		}
	}

	internal void AddRelatedEntitiesToObjectStateManager(bool doAttach)
	{
		if (_relationships == null)
		{
			return;
		}
		bool flag = true;
		try
		{
			foreach (RelatedEnd relationship in Relationships)
			{
				relationship.Include(addRelationshipAsUnchanged: false, doAttach);
			}
			flag = false;
		}
		finally
		{
			if (flag)
			{
				IEntityWrapper wrappedOwner = WrappedOwner;
				TransactionManager transactionManager = wrappedOwner.Context.ObjectStateManager.TransactionManager;
				wrappedOwner.Context.ObjectStateManager.DegradePromotedRelationships();
				NodeVisited = true;
				RemoveRelatedEntitiesFromObjectStateManager(wrappedOwner);
				if (transactionManager.IsAttachTracking && transactionManager.PromotedKeyEntries.TryGetValue(wrappedOwner.Entity, out var value))
				{
					value.DegradeEntry();
				}
				else
				{
					RelatedEnd.RemoveEntityFromObjectStateManager(wrappedOwner);
				}
			}
		}
	}

	internal static void RemoveRelatedEntitiesFromObjectStateManager(IEntityWrapper wrappedEntity)
	{
		foreach (RelatedEnd relationship in wrappedEntity.RelationshipManager.Relationships)
		{
			if (relationship.ObjectContext != null)
			{
				relationship.Exclude();
				relationship.DetachContext();
			}
		}
	}

	internal void RemoveEntityFromRelationships()
	{
		if (_relationships == null)
		{
			return;
		}
		foreach (RelatedEnd relationship in Relationships)
		{
			relationship.RemoveAll();
		}
	}

	internal void NullAllFKsInDependentsForWhichThisIsThePrincipal()
	{
		if (_relationships == null)
		{
			return;
		}
		List<EntityReference> list = new List<EntityReference>();
		foreach (RelatedEnd relationship in Relationships)
		{
			if (!relationship.IsForeignKey)
			{
				continue;
			}
			foreach (IEntityWrapper wrappedEntity in relationship.GetWrappedEntities())
			{
				RelatedEnd otherEndOfRelationship = relationship.GetOtherEndOfRelationship(wrappedEntity);
				if (otherEndOfRelationship.IsDependentEndOfReferentialConstraint(checkIdentifying: false))
				{
					list.Add((EntityReference)otherEndOfRelationship);
				}
			}
		}
		foreach (EntityReference item in list)
		{
			item.NullAllForeignKeys();
		}
	}

	internal void DetachEntityFromRelationships(EntityState ownerEntityState)
	{
		if (_relationships == null)
		{
			return;
		}
		foreach (RelatedEnd relationship in Relationships)
		{
			relationship.DetachAll(ownerEntityState);
		}
	}

	internal void RemoveEntity(string toRole, string relationshipName, IEntityWrapper wrappedEntity)
	{
		if (TryGetCachedRelatedEnd(relationshipName, toRole, out var relatedEnd))
		{
			relatedEnd.Remove(wrappedEntity, preserveForeignKey: false);
		}
	}

	internal void ClearRelatedEndWrappers()
	{
		if (_relationships == null)
		{
			return;
		}
		foreach (RelatedEnd relationship in Relationships)
		{
			relationship.ClearWrappedValues();
		}
	}

	internal void RetrieveReferentialConstraintProperties(out Dictionary<string, KeyValuePair<object, IntBox>> properties, HashSet<object> visited, bool includeOwnValues)
	{
		IEntityWrapper wrappedOwner = WrappedOwner;
		properties = new Dictionary<string, KeyValuePair<object, IntBox>>();
		EntityKey entityKey = wrappedOwner.EntityKey;
		if (entityKey.IsTemporary)
		{
			FindNamesOfReferentialConstraintProperties(out var propertiesToRetrieve, out var _, skipFK: false);
			if (propertiesToRetrieve != null)
			{
				if (_relationships != null)
				{
					foreach (RelatedEnd relationship in _relationships)
					{
						relationship.RetrieveReferentialConstraintProperties(properties, visited);
					}
				}
				if (!CheckIfAllPropertiesWereRetrieved(properties, propertiesToRetrieve))
				{
					wrappedOwner.Context.ObjectStateManager.FindEntityEntry(entityKey).RetrieveReferentialConstraintPropertiesFromKeyEntries(properties);
					if (!CheckIfAllPropertiesWereRetrieved(properties, propertiesToRetrieve))
					{
						throw new InvalidOperationException(Strings.RelationshipManager_UnableToRetrieveReferentialConstraintProperties);
					}
				}
			}
		}
		if (!entityKey.IsTemporary || includeOwnValues)
		{
			wrappedOwner.Context.ObjectStateManager.FindEntityEntry(entityKey).GetOtherKeyProperties(properties);
		}
	}

	private static bool CheckIfAllPropertiesWereRetrieved(Dictionary<string, KeyValuePair<object, IntBox>> properties, List<string> propertiesToRetrieve)
	{
		bool flag = true;
		List<int> list = new List<int>();
		ICollection<KeyValuePair<object, IntBox>> values = properties.Values;
		foreach (KeyValuePair<object, IntBox> item in values)
		{
			list.Add(item.Value.Value);
		}
		foreach (string item2 in propertiesToRetrieve)
		{
			if (!properties.ContainsKey(item2))
			{
				flag = false;
				break;
			}
			KeyValuePair<object, IntBox> keyValuePair = properties[item2];
			keyValuePair.Value.Value = keyValuePair.Value.Value - 1;
			if (keyValuePair.Value.Value < 0)
			{
				flag = false;
				break;
			}
		}
		if (flag)
		{
			foreach (KeyValuePair<object, IntBox> item3 in values)
			{
				if (item3.Value.Value != 0)
				{
					flag = false;
					break;
				}
			}
		}
		if (!flag)
		{
			IEnumerator<int> enumerator3 = list.GetEnumerator();
			foreach (KeyValuePair<object, IntBox> item4 in values)
			{
				enumerator3.MoveNext();
				item4.Value.Value = enumerator3.Current;
			}
		}
		return flag;
	}

	internal void CheckReferentialConstraintProperties(EntityEntry ownerEntry)
	{
		if (!HasReferentialConstraintPropertiesToCheck() || _relationships == null)
		{
			return;
		}
		foreach (RelatedEnd relationship in _relationships)
		{
			relationship.CheckReferentialConstraintProperties(ownerEntry);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Browsable(false)]
	[OnDeserialized]
	public void OnDeserialized(StreamingContext context)
	{
		_entityWrapperFactory = new EntityWrapperFactory();
		_expensiveLoader = new ExpensiveOSpaceLoader();
		_wrappedOwner = EntityWrapperFactory.WrapEntityUsingContext(_owner, null);
	}

	private bool TryGetCachedRelatedEnd(string relationshipName, string targetRoleName, out RelatedEnd relatedEnd)
	{
		relatedEnd = null;
		if (_relationships != null)
		{
			foreach (RelatedEnd relationship in _relationships)
			{
				RelationshipNavigation relationshipNavigation = relationship.RelationshipNavigation;
				if (relationshipNavigation.RelationshipName == relationshipName && relationshipNavigation.To == targetRoleName)
				{
					relatedEnd = relationship;
					return true;
				}
			}
		}
		return false;
	}

	internal bool FindNamesOfReferentialConstraintProperties(out List<string> propertiesToRetrieve, out bool propertiesToPropagateExist, bool skipFK)
	{
		IEntityWrapper wrappedOwner = WrappedOwner;
		EntityKey obj = wrappedOwner.EntityKey ?? throw Error.EntityKey_UnexpectedNull();
		propertiesToRetrieve = null;
		propertiesToPropagateExist = false;
		if (wrappedOwner.Context == null)
		{
			throw new InvalidOperationException(Strings.RelationshipManager_UnexpectedNullContext);
		}
		EntitySet entitySet = obj.GetEntitySet(wrappedOwner.Context.MetadataWorkspace);
		ReadOnlyCollection<AssociationSet> associationSets = entitySet.AssociationSets;
		bool result = false;
		foreach (AssociationSet item in associationSets)
		{
			if (skipFK && item.ElementType.IsForeignKey)
			{
				result = true;
				continue;
			}
			foreach (ReferentialConstraint referentialConstraint in item.ElementType.ReferentialConstraints)
			{
				if (referentialConstraint.ToRole.TypeUsage.EdmType == entitySet.ElementType.GetReferenceType())
				{
					propertiesToRetrieve = propertiesToRetrieve ?? new List<string>();
					foreach (EdmProperty toProperty in referentialConstraint.ToProperties)
					{
						propertiesToRetrieve.Add(toProperty.Name);
					}
				}
				if (referentialConstraint.FromRole.TypeUsage.EdmType == entitySet.ElementType.GetReferenceType())
				{
					propertiesToPropagateExist = true;
				}
			}
		}
		return result;
	}

	internal bool HasReferentialConstraintPropertiesToCheck()
	{
		IEntityWrapper wrappedOwner = WrappedOwner;
		EntityKey obj = wrappedOwner.EntityKey ?? throw Error.EntityKey_UnexpectedNull();
		if (wrappedOwner.Context == null)
		{
			throw new InvalidOperationException(Strings.RelationshipManager_UnexpectedNullContext);
		}
		EntitySet entitySet = obj.GetEntitySet(wrappedOwner.Context.MetadataWorkspace);
		foreach (AssociationSet associationSet in entitySet.AssociationSets)
		{
			foreach (ReferentialConstraint referentialConstraint in associationSet.ElementType.ReferentialConstraints)
			{
				if (referentialConstraint.ToRole.TypeUsage.EdmType == entitySet.ElementType.GetReferenceType())
				{
					return true;
				}
				if (referentialConstraint.FromRole.TypeUsage.EdmType == entitySet.ElementType.GetReferenceType())
				{
					return true;
				}
			}
		}
		return false;
	}

	internal bool IsOwner(IEntityWrapper wrappedEntity)
	{
		IEntityWrapper wrappedOwner = WrappedOwner;
		return wrappedEntity.Entity == wrappedOwner.Entity;
	}

	internal void AttachContextToRelatedEnds(ObjectContext context, EntitySet entitySet, MergeOption mergeOption)
	{
		if (_relationships == null)
		{
			return;
		}
		foreach (RelatedEnd relationship in Relationships)
		{
			relationship.FindRelationshipSet(context, entitySet, out var _, out var relationshipSet);
			if (relationshipSet != null || !relationship.IsEmpty())
			{
				relationship.AttachContext(context, entitySet, mergeOption);
			}
			else
			{
				_relationships.Remove(relationship);
			}
		}
	}

	internal void ResetContextOnRelatedEnds(ObjectContext context, EntitySet entitySet, MergeOption mergeOption)
	{
		if (_relationships == null)
		{
			return;
		}
		foreach (RelatedEnd relationship in Relationships)
		{
			relationship.AttachContext(context, entitySet, mergeOption);
			foreach (IEntityWrapper wrappedEntity in relationship.GetWrappedEntities())
			{
				wrappedEntity.ResetContext(context, relationship.GetTargetEntitySetFromRelationshipSet(), mergeOption);
			}
		}
	}

	internal void DetachContextFromRelatedEnds()
	{
		if (_relationships == null)
		{
			return;
		}
		foreach (RelatedEnd relationship in _relationships)
		{
			relationship.DetachContext();
		}
	}

	[Conditional("DEBUG")]
	internal void VerifyIsNotRelated()
	{
		if (_relationships == null)
		{
			return;
		}
		foreach (RelatedEnd relationship in _relationships)
		{
			relationship.IsEmpty();
		}
	}
}
