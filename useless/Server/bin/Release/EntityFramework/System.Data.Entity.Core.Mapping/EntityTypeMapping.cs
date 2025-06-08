using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Utilities;
using System.Linq;

namespace System.Data.Entity.Core.Mapping;

public class EntityTypeMapping : TypeMapping
{
	private readonly EntitySetMapping _entitySetMapping;

	private readonly List<MappingFragment> _fragments;

	private readonly Dictionary<string, EntityType> m_entityTypes = new Dictionary<string, EntityType>(StringComparer.Ordinal);

	private readonly Dictionary<string, EntityType> m_isOfEntityTypes = new Dictionary<string, EntityType>(StringComparer.Ordinal);

	private EntityType _entityType;

	public EntitySetMapping EntitySetMapping => _entitySetMapping;

	internal override EntitySetBaseMapping SetMapping => EntitySetMapping;

	public EntityType EntityType => _entityType ?? (_entityType = m_entityTypes.Values.SingleOrDefault());

	public bool IsHierarchyMapping
	{
		get
		{
			if (m_isOfEntityTypes.Count <= 0)
			{
				return m_entityTypes.Count > 1;
			}
			return true;
		}
	}

	public ReadOnlyCollection<MappingFragment> Fragments => new ReadOnlyCollection<MappingFragment>(_fragments);

	internal override ReadOnlyCollection<MappingFragment> MappingFragments => Fragments;

	public ReadOnlyCollection<EntityTypeBase> EntityTypes => new ReadOnlyCollection<EntityTypeBase>(new List<EntityTypeBase>(m_entityTypes.Values));

	internal override ReadOnlyCollection<EntityTypeBase> Types => EntityTypes;

	public ReadOnlyCollection<EntityTypeBase> IsOfEntityTypes => new ReadOnlyCollection<EntityTypeBase>(new List<EntityTypeBase>(m_isOfEntityTypes.Values));

	internal override ReadOnlyCollection<EntityTypeBase> IsOfTypes => IsOfEntityTypes;

	public EntityTypeMapping(EntitySetMapping entitySetMapping)
	{
		_entitySetMapping = entitySetMapping;
		_fragments = new List<MappingFragment>();
	}

	public void AddType(EntityType type)
	{
		Check.NotNull(type, "type");
		ThrowIfReadOnly();
		m_entityTypes.Add(type.FullName, type);
	}

	public void RemoveType(EntityType type)
	{
		Check.NotNull(type, "type");
		ThrowIfReadOnly();
		m_entityTypes.Remove(type.FullName);
	}

	public void AddIsOfType(EntityType type)
	{
		Check.NotNull(type, "type");
		ThrowIfReadOnly();
		m_isOfEntityTypes.Add(type.FullName, type);
	}

	public void RemoveIsOfType(EntityType type)
	{
		Check.NotNull(type, "type");
		ThrowIfReadOnly();
		m_isOfEntityTypes.Remove(type.FullName);
	}

	public void AddFragment(MappingFragment fragment)
	{
		Check.NotNull(fragment, "fragment");
		ThrowIfReadOnly();
		_fragments.Add(fragment);
	}

	public void RemoveFragment(MappingFragment fragment)
	{
		Check.NotNull(fragment, "fragment");
		ThrowIfReadOnly();
		_fragments.Remove(fragment);
	}

	internal override void SetReadOnly()
	{
		_fragments.TrimExcess();
		MappingItem.SetReadOnly(_fragments);
		base.SetReadOnly();
	}

	internal EntityType GetContainerType(string memberName)
	{
		foreach (EntityType value in m_entityTypes.Values)
		{
			if (value.Properties.Contains(memberName))
			{
				return value;
			}
		}
		foreach (EntityType value2 in m_isOfEntityTypes.Values)
		{
			if (value2.Properties.Contains(memberName))
			{
				return value2;
			}
		}
		return null;
	}
}
