using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity.Core.Common.Utils;
using System.Data.Entity.Core.Metadata.Edm;
using System.Linq;

namespace System.Data.Entity.Core.Mapping;

public abstract class EntitySetBaseMapping : MappingItem
{
	private readonly EntityContainerMapping _containerMapping;

	private string _queryView;

	private readonly Dictionary<Pair<EntitySetBase, Pair<EntityTypeBase, bool>>, string> _typeSpecificQueryViews = new Dictionary<Pair<EntitySetBase, Pair<EntityTypeBase, bool>>, string>(Pair<EntitySetBase, Pair<EntityTypeBase, bool>>.PairComparer.Instance);

	public EntityContainerMapping ContainerMapping => _containerMapping;

	internal EntityContainerMapping EntityContainerMapping => ContainerMapping;

	public string QueryView
	{
		get
		{
			return _queryView;
		}
		set
		{
			ThrowIfReadOnly();
			_queryView = value;
		}
	}

	internal abstract EntitySetBase Set { get; }

	internal abstract IEnumerable<TypeMapping> TypeMappings { get; }

	internal virtual bool HasNoContent
	{
		get
		{
			if (QueryView != null)
			{
				return false;
			}
			foreach (TypeMapping typeMapping in TypeMappings)
			{
				foreach (MappingFragment mappingFragment in typeMapping.MappingFragments)
				{
					if (mappingFragment.AllProperties.Any())
					{
						return false;
					}
				}
			}
			return true;
		}
	}

	internal int StartLineNumber { get; set; }

	internal int StartLinePosition { get; set; }

	internal bool HasModificationFunctionMapping { get; set; }

	internal EntitySetBaseMapping(EntityContainerMapping containerMapping)
	{
		_containerMapping = containerMapping;
	}

	internal bool ContainsTypeSpecificQueryView(Pair<EntitySetBase, Pair<EntityTypeBase, bool>> key)
	{
		return _typeSpecificQueryViews.ContainsKey(key);
	}

	internal void AddTypeSpecificQueryView(Pair<EntitySetBase, Pair<EntityTypeBase, bool>> key, string viewString)
	{
		_typeSpecificQueryViews.Add(key, viewString);
	}

	internal ReadOnlyCollection<Pair<EntitySetBase, Pair<EntityTypeBase, bool>>> GetTypeSpecificQVKeys()
	{
		return new ReadOnlyCollection<Pair<EntitySetBase, Pair<EntityTypeBase, bool>>>(_typeSpecificQueryViews.Keys.ToList());
	}

	internal string GetTypeSpecificQueryView(Pair<EntitySetBase, Pair<EntityTypeBase, bool>> key)
	{
		return _typeSpecificQueryViews[key];
	}
}
