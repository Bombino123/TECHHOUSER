using System.Collections.Generic;
using System.Data.Entity.Utilities;
using System.Linq;
using System.Threading;

namespace System.Data.Entity.Core.Metadata.Edm;

public class EntityType : EntityTypeBase
{
	private ReadOnlyMetadataCollection<EdmProperty> _properties;

	private RefType _referenceType;

	private RowType _keyRow;

	private readonly List<ForeignKeyBuilder> _foreignKeyBuilders = new List<ForeignKeyBuilder>();

	private readonly object _navigationPropertiesCacheLock = new object();

	private ReadOnlyMetadataCollection<NavigationProperty> _navigationPropertiesCache;

	internal IEnumerable<ForeignKeyBuilder> ForeignKeyBuilders => _foreignKeyBuilders;

	public override BuiltInTypeKind BuiltInTypeKind => BuiltInTypeKind.EntityType;

	public ReadOnlyMetadataCollection<NavigationProperty> DeclaredNavigationProperties => GetDeclaredOnlyMembers<NavigationProperty>();

	public ReadOnlyMetadataCollection<NavigationProperty> NavigationProperties
	{
		get
		{
			ReadOnlyMetadataCollection<NavigationProperty> navigationPropertiesCache = _navigationPropertiesCache;
			if (navigationPropertiesCache == null)
			{
				lock (_navigationPropertiesCacheLock)
				{
					if (_navigationPropertiesCache == null)
					{
						base.Members.SourceAccessed += ResetNavigationProperties;
						_navigationPropertiesCache = new FilteredReadOnlyMetadataCollection<NavigationProperty, EdmMember>(base.Members, Helper.IsNavigationProperty);
					}
					navigationPropertiesCache = _navigationPropertiesCache;
				}
			}
			return navigationPropertiesCache;
		}
	}

	public ReadOnlyMetadataCollection<EdmProperty> DeclaredProperties => GetDeclaredOnlyMembers<EdmProperty>();

	public ReadOnlyMetadataCollection<EdmMember> DeclaredMembers => GetDeclaredOnlyMembers<EdmMember>();

	public virtual ReadOnlyMetadataCollection<EdmProperty> Properties
	{
		get
		{
			if (!base.IsReadOnly)
			{
				return new FilteredReadOnlyMetadataCollection<EdmProperty, EdmMember>(base.Members, Helper.IsEdmProperty);
			}
			if (_properties == null)
			{
				Interlocked.CompareExchange(ref _properties, new FilteredReadOnlyMetadataCollection<EdmProperty, EdmMember>(base.Members, Helper.IsEdmProperty), null);
			}
			return _properties;
		}
	}

	internal EntityType(string name, string namespaceName, DataSpace dataSpace)
		: base(name, namespaceName, dataSpace)
	{
	}

	internal EntityType(string name, string namespaceName, DataSpace dataSpace, IEnumerable<string> keyMemberNames, IEnumerable<EdmMember> members)
		: base(name, namespaceName, dataSpace)
	{
		if (members != null)
		{
			EntityTypeBase.CheckAndAddMembers(members, this);
		}
		if (keyMemberNames != null)
		{
			CheckAndAddKeyMembers(keyMemberNames);
		}
	}

	internal void RemoveForeignKey(ForeignKeyBuilder foreignKeyBuilder)
	{
		Util.ThrowIfReadOnly(this);
		foreignKeyBuilder.SetOwner(null);
		_foreignKeyBuilders.Remove(foreignKeyBuilder);
	}

	internal void AddForeignKey(ForeignKeyBuilder foreignKeyBuilder)
	{
		Util.ThrowIfReadOnly(this);
		foreignKeyBuilder.SetOwner(this);
		_foreignKeyBuilders.Add(foreignKeyBuilder);
	}

	internal override void ValidateMemberForAdd(EdmMember member)
	{
	}

	private void ResetNavigationProperties(object sender, EventArgs e)
	{
		if (_navigationPropertiesCache == null)
		{
			return;
		}
		lock (_navigationPropertiesCacheLock)
		{
			if (_navigationPropertiesCache != null)
			{
				_navigationPropertiesCache = null;
				base.Members.SourceAccessed -= ResetNavigationProperties;
			}
		}
	}

	public RefType GetReferenceType()
	{
		if (_referenceType == null)
		{
			Interlocked.CompareExchange(ref _referenceType, new RefType(this), null);
		}
		return _referenceType;
	}

	internal RowType GetKeyRowType()
	{
		if (_keyRow == null)
		{
			List<EdmProperty> list = new List<EdmProperty>(KeyMembers.Count);
			list.AddRange(KeyMembers.Select((EdmMember keyMember) => new EdmProperty(keyMember.Name, Helper.GetModelTypeUsage(keyMember))));
			Interlocked.CompareExchange(ref _keyRow, new RowType(list), null);
		}
		return _keyRow;
	}

	internal bool TryGetNavigationProperty(string relationshipType, string fromName, string toName, out NavigationProperty navigationProperty)
	{
		foreach (NavigationProperty navigationProperty2 in NavigationProperties)
		{
			if (navigationProperty2.RelationshipType.FullName == relationshipType && navigationProperty2.FromEndMember.Name == fromName && navigationProperty2.ToEndMember.Name == toName)
			{
				navigationProperty = navigationProperty2;
				return true;
			}
		}
		navigationProperty = null;
		return false;
	}

	public static EntityType Create(string name, string namespaceName, DataSpace dataSpace, IEnumerable<string> keyMemberNames, IEnumerable<EdmMember> members, IEnumerable<MetadataProperty> metadataProperties)
	{
		Check.NotEmpty(name, "name");
		Check.NotEmpty(namespaceName, "namespaceName");
		EntityType entityType = new EntityType(name, namespaceName, dataSpace, keyMemberNames, members);
		if (metadataProperties != null)
		{
			entityType.AddMetadataProperties(metadataProperties);
		}
		entityType.SetReadOnly();
		return entityType;
	}

	public static EntityType Create(string name, string namespaceName, DataSpace dataSpace, EntityType baseType, IEnumerable<string> keyMemberNames, IEnumerable<EdmMember> members, IEnumerable<MetadataProperty> metadataProperties)
	{
		Check.NotEmpty(name, "name");
		Check.NotEmpty(namespaceName, "namespaceName");
		Check.NotNull(baseType, "baseType");
		EntityType entityType = new EntityType(name, namespaceName, dataSpace, keyMemberNames, members)
		{
			BaseType = baseType
		};
		if (metadataProperties != null)
		{
			entityType.AddMetadataProperties(metadataProperties);
		}
		entityType.SetReadOnly();
		return entityType;
	}

	public void AddNavigationProperty(NavigationProperty property)
	{
		Check.NotNull(property, "property");
		AddMember(property, forceAdd: true);
	}
}
