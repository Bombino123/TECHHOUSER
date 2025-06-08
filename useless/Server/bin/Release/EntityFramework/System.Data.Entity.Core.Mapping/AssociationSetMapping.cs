using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Utilities;
using System.Linq;

namespace System.Data.Entity.Core.Mapping;

public class AssociationSetMapping : EntitySetBaseMapping
{
	private readonly AssociationSet _associationSet;

	private AssociationTypeMapping _associationTypeMapping;

	private AssociationSetModificationFunctionMapping _modificationFunctionMapping;

	public AssociationSet AssociationSet => _associationSet;

	internal override EntitySetBase Set => AssociationSet;

	public AssociationTypeMapping AssociationTypeMapping
	{
		get
		{
			return _associationTypeMapping;
		}
		internal set
		{
			_associationTypeMapping = value;
		}
	}

	internal override IEnumerable<TypeMapping> TypeMappings
	{
		get
		{
			yield return _associationTypeMapping;
		}
	}

	public AssociationSetModificationFunctionMapping ModificationFunctionMapping
	{
		get
		{
			return _modificationFunctionMapping;
		}
		set
		{
			ThrowIfReadOnly();
			_modificationFunctionMapping = value;
		}
	}

	public EntitySet StoreEntitySet
	{
		get
		{
			if (SingleFragment == null)
			{
				return null;
			}
			return SingleFragment.StoreEntitySet;
		}
		internal set
		{
			SingleFragment.StoreEntitySet = value;
		}
	}

	internal EntityType Table
	{
		get
		{
			if (StoreEntitySet == null)
			{
				return null;
			}
			return StoreEntitySet.ElementType;
		}
	}

	public EndPropertyMapping SourceEndMapping
	{
		get
		{
			if (SingleFragment == null)
			{
				return null;
			}
			return SingleFragment.PropertyMappings.OfType<EndPropertyMapping>().FirstOrDefault();
		}
		set
		{
			Check.NotNull(value, "value");
			ThrowIfReadOnly();
			SingleFragment.AddPropertyMapping(value);
		}
	}

	public EndPropertyMapping TargetEndMapping
	{
		get
		{
			if (SingleFragment == null)
			{
				return null;
			}
			return SingleFragment.PropertyMappings.OfType<EndPropertyMapping>().ElementAtOrDefault(1);
		}
		set
		{
			Check.NotNull(value, "value");
			ThrowIfReadOnly();
			SingleFragment.AddPropertyMapping(value);
		}
	}

	public ReadOnlyCollection<ConditionPropertyMapping> Conditions
	{
		get
		{
			if (SingleFragment == null)
			{
				return new ReadOnlyCollection<ConditionPropertyMapping>(new List<ConditionPropertyMapping>());
			}
			return SingleFragment.Conditions;
		}
	}

	private MappingFragment SingleFragment
	{
		get
		{
			if (_associationTypeMapping == null)
			{
				return null;
			}
			return _associationTypeMapping.MappingFragment;
		}
	}

	public AssociationSetMapping(AssociationSet associationSet, EntitySet storeEntitySet, EntityContainerMapping containerMapping)
		: base(containerMapping)
	{
		Check.NotNull(associationSet, "associationSet");
		Check.NotNull(storeEntitySet, "storeEntitySet");
		_associationSet = associationSet;
		_associationTypeMapping = new AssociationTypeMapping(associationSet.ElementType, this);
		_associationTypeMapping.MappingFragment = new MappingFragment(storeEntitySet, _associationTypeMapping, makeColumnsDistinct: false);
	}

	internal AssociationSetMapping(AssociationSet associationSet, EntitySet storeEntitySet)
		: this(associationSet, storeEntitySet, null)
	{
	}

	internal AssociationSetMapping(AssociationSet associationSet, EntityContainerMapping containerMapping)
		: base(containerMapping)
	{
		_associationSet = associationSet;
	}

	public void AddCondition(ConditionPropertyMapping condition)
	{
		Check.NotNull(condition, "condition");
		ThrowIfReadOnly();
		if (SingleFragment != null)
		{
			SingleFragment.AddCondition(condition);
		}
	}

	public void RemoveCondition(ConditionPropertyMapping condition)
	{
		Check.NotNull(condition, "condition");
		ThrowIfReadOnly();
		if (SingleFragment != null)
		{
			SingleFragment.RemoveCondition(condition);
		}
	}

	internal override void SetReadOnly()
	{
		MappingItem.SetReadOnly(_associationTypeMapping);
		MappingItem.SetReadOnly(_modificationFunctionMapping);
		base.SetReadOnly();
	}
}
