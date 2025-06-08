using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Utilities;
using System.Diagnostics;

namespace System.Data.Entity.Core.Mapping;

public class EntitySetMapping : EntitySetBaseMapping
{
	private readonly EntitySet _entitySet;

	private readonly List<EntityTypeMapping> _entityTypeMappings;

	private readonly List<EntityTypeModificationFunctionMapping> _modificationFunctionMappings;

	private Lazy<List<AssociationSetEnd>> _implicitlyMappedAssociationSetEnds;

	public EntitySet EntitySet => _entitySet;

	internal override EntitySetBase Set => EntitySet;

	public ReadOnlyCollection<EntityTypeMapping> EntityTypeMappings => new ReadOnlyCollection<EntityTypeMapping>(_entityTypeMappings);

	internal override IEnumerable<TypeMapping> TypeMappings => _entityTypeMappings;

	public ReadOnlyCollection<EntityTypeModificationFunctionMapping> ModificationFunctionMappings => new ReadOnlyCollection<EntityTypeModificationFunctionMapping>(_modificationFunctionMappings);

	internal IEnumerable<AssociationSetEnd> ImplicitlyMappedAssociationSetEnds => _implicitlyMappedAssociationSetEnds.Value;

	internal override bool HasNoContent
	{
		get
		{
			if (_modificationFunctionMappings.Count != 0)
			{
				return false;
			}
			return base.HasNoContent;
		}
	}

	public EntitySetMapping(EntitySet entitySet, EntityContainerMapping containerMapping)
		: base(containerMapping)
	{
		Check.NotNull(entitySet, "entitySet");
		_entitySet = entitySet;
		_entityTypeMappings = new List<EntityTypeMapping>();
		_modificationFunctionMappings = new List<EntityTypeModificationFunctionMapping>();
		_implicitlyMappedAssociationSetEnds = new Lazy<List<AssociationSetEnd>>(InitializeImplicitlyMappedAssociationSetEnds);
	}

	public void AddTypeMapping(EntityTypeMapping typeMapping)
	{
		Check.NotNull(typeMapping, "typeMapping");
		ThrowIfReadOnly();
		_entityTypeMappings.Add(typeMapping);
	}

	public void RemoveTypeMapping(EntityTypeMapping typeMapping)
	{
		Check.NotNull(typeMapping, "typeMapping");
		ThrowIfReadOnly();
		_entityTypeMappings.Remove(typeMapping);
	}

	internal void ClearModificationFunctionMappings()
	{
		_modificationFunctionMappings.Clear();
	}

	public void AddModificationFunctionMapping(EntityTypeModificationFunctionMapping modificationFunctionMapping)
	{
		Check.NotNull(modificationFunctionMapping, "modificationFunctionMapping");
		ThrowIfReadOnly();
		_modificationFunctionMappings.Add(modificationFunctionMapping);
		if (_implicitlyMappedAssociationSetEnds.IsValueCreated)
		{
			_implicitlyMappedAssociationSetEnds = new Lazy<List<AssociationSetEnd>>(InitializeImplicitlyMappedAssociationSetEnds);
		}
	}

	public void RemoveModificationFunctionMapping(EntityTypeModificationFunctionMapping modificationFunctionMapping)
	{
		Check.NotNull(modificationFunctionMapping, "modificationFunctionMapping");
		ThrowIfReadOnly();
		_modificationFunctionMappings.Remove(modificationFunctionMapping);
		if (_implicitlyMappedAssociationSetEnds.IsValueCreated)
		{
			_implicitlyMappedAssociationSetEnds = new Lazy<List<AssociationSetEnd>>(InitializeImplicitlyMappedAssociationSetEnds);
		}
	}

	internal override void SetReadOnly()
	{
		_entityTypeMappings.TrimExcess();
		_modificationFunctionMappings.TrimExcess();
		if (_implicitlyMappedAssociationSetEnds.IsValueCreated)
		{
			_implicitlyMappedAssociationSetEnds.Value.TrimExcess();
		}
		MappingItem.SetReadOnly(_entityTypeMappings);
		MappingItem.SetReadOnly(_modificationFunctionMappings);
		base.SetReadOnly();
	}

	[Conditional("DEBUG")]
	private void AssertModificationFunctionMappingInvariants(EntityTypeModificationFunctionMapping modificationFunctionMapping)
	{
		foreach (EntityTypeModificationFunctionMapping modificationFunctionMapping2 in _modificationFunctionMappings)
		{
			_ = modificationFunctionMapping2;
		}
	}

	private List<AssociationSetEnd> InitializeImplicitlyMappedAssociationSetEnds()
	{
		List<AssociationSetEnd> list = new List<AssociationSetEnd>();
		foreach (EntityTypeModificationFunctionMapping modificationFunctionMapping in _modificationFunctionMappings)
		{
			if (modificationFunctionMapping.DeleteFunctionMapping != null)
			{
				list.AddRange(modificationFunctionMapping.DeleteFunctionMapping.CollocatedAssociationSetEnds);
			}
			if (modificationFunctionMapping.InsertFunctionMapping != null)
			{
				list.AddRange(modificationFunctionMapping.InsertFunctionMapping.CollocatedAssociationSetEnds);
			}
			if (modificationFunctionMapping.UpdateFunctionMapping != null)
			{
				list.AddRange(modificationFunctionMapping.UpdateFunctionMapping.CollocatedAssociationSetEnds);
			}
		}
		if (base.IsReadOnly)
		{
			list.TrimExcess();
		}
		return list;
	}
}
