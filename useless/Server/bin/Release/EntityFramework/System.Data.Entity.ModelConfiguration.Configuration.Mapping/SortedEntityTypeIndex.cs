using System.Collections.Generic;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.ModelConfiguration.Edm;

namespace System.Data.Entity.ModelConfiguration.Configuration.Mapping;

internal class SortedEntityTypeIndex
{
	private static readonly EntityType[] _emptyTypes = new EntityType[0];

	private readonly Dictionary<EntitySet, List<EntityType>> _entityTypes;

	public SortedEntityTypeIndex()
	{
		_entityTypes = new Dictionary<EntitySet, List<EntityType>>();
	}

	public void Add(EntitySet entitySet, EntityType entityType)
	{
		int i = 0;
		if (!_entityTypes.TryGetValue(entitySet, out var value))
		{
			value = new List<EntityType>();
			_entityTypes.Add(entitySet, value);
		}
		for (; i < value.Count; i++)
		{
			if (value[i] == entityType)
			{
				return;
			}
			if (entityType.IsAncestorOf(value[i]))
			{
				break;
			}
		}
		value.Insert(i, entityType);
	}

	public bool Contains(EntitySet entitySet, EntityType entityType)
	{
		if (_entityTypes.TryGetValue(entitySet, out var value))
		{
			return value.Contains(entityType);
		}
		return false;
	}

	public bool IsRoot(EntitySet entitySet, EntityType entityType)
	{
		bool result = true;
		foreach (EntityType item in _entityTypes[entitySet])
		{
			if (item != entityType && item.IsAncestorOf(entityType))
			{
				result = false;
			}
		}
		return result;
	}

	public IEnumerable<EntitySet> GetEntitySets()
	{
		return _entityTypes.Keys;
	}

	public IEnumerable<EntityType> GetEntityTypes(EntitySet entitySet)
	{
		if (_entityTypes.TryGetValue(entitySet, out var value))
		{
			return value;
		}
		return _emptyTypes;
	}
}
