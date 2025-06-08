using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity.Core.Common.Utils;
using System.Data.Entity.Core.Mapping.ViewGeneration;
using System.Data.Entity.Core.Mapping.ViewGeneration.Structures;
using System.Data.Entity.Core.Mapping.ViewGeneration.Validation;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Utilities;
using System.Linq;

namespace System.Data.Entity.Core.Mapping;

public class EntityContainerMapping : MappingBase
{
	private readonly string identity;

	private readonly bool m_validate;

	private readonly bool m_generateUpdateViews;

	private readonly EntityContainer m_entityContainer;

	private readonly EntityContainer m_storageEntityContainer;

	private readonly Dictionary<string, EntitySetBaseMapping> m_entitySetMappings = new Dictionary<string, EntitySetBaseMapping>(StringComparer.Ordinal);

	private readonly Dictionary<string, EntitySetBaseMapping> m_associationSetMappings = new Dictionary<string, EntitySetBaseMapping>(StringComparer.Ordinal);

	private readonly Dictionary<EdmFunction, FunctionImportMapping> m_functionImportMappings = new Dictionary<EdmFunction, FunctionImportMapping>();

	private readonly StorageMappingItemCollection m_storageMappingItemCollection;

	private readonly Memoizer<InputForComputingCellGroups, OutputFromComputeCellGroups> m_memoizedCellGroupEvaluator;

	public StorageMappingItemCollection MappingItemCollection => m_storageMappingItemCollection;

	internal StorageMappingItemCollection StorageMappingItemCollection => MappingItemCollection;

	public override BuiltInTypeKind BuiltInTypeKind => BuiltInTypeKind.MetadataItem;

	internal override MetadataItem EdmItem => m_entityContainer;

	internal override string Identity => identity;

	internal bool IsEmpty
	{
		get
		{
			if (m_entitySetMappings.Count == 0)
			{
				return m_associationSetMappings.Count == 0;
			}
			return false;
		}
	}

	internal bool HasViews
	{
		get
		{
			if (!HasMappingFragments())
			{
				return AllSetMaps.Any((EntitySetBaseMapping setMap) => setMap.QueryView != null);
			}
			return true;
		}
	}

	internal string SourceLocation { get; set; }

	public EntityContainer ConceptualEntityContainer => m_entityContainer;

	internal EntityContainer EdmEntityContainer => ConceptualEntityContainer;

	public EntityContainer StoreEntityContainer => m_storageEntityContainer;

	internal EntityContainer StorageEntityContainer => StoreEntityContainer;

	internal ReadOnlyCollection<EntitySetBaseMapping> EntitySetMaps => new ReadOnlyCollection<EntitySetBaseMapping>(new List<EntitySetBaseMapping>(m_entitySetMappings.Values));

	public virtual IEnumerable<EntitySetMapping> EntitySetMappings => EntitySetMaps.OfType<EntitySetMapping>();

	public virtual IEnumerable<AssociationSetMapping> AssociationSetMappings => RelationshipSetMaps.OfType<AssociationSetMapping>();

	public IEnumerable<FunctionImportMapping> FunctionImportMappings => m_functionImportMappings.Values;

	internal ReadOnlyCollection<EntitySetBaseMapping> RelationshipSetMaps => new ReadOnlyCollection<EntitySetBaseMapping>(new List<EntitySetBaseMapping>(m_associationSetMappings.Values));

	internal IEnumerable<EntitySetBaseMapping> AllSetMaps => m_entitySetMappings.Values.Concat(m_associationSetMappings.Values);

	internal int StartLineNumber { get; set; }

	internal int StartLinePosition { get; set; }

	internal bool Validate => m_validate;

	public bool GenerateUpdateViews => m_generateUpdateViews;

	public EntityContainerMapping(EntityContainer conceptualEntityContainer, EntityContainer storeEntityContainer, StorageMappingItemCollection mappingItemCollection, bool generateUpdateViews)
		: this(conceptualEntityContainer, storeEntityContainer, mappingItemCollection, validate: true, generateUpdateViews)
	{
	}

	internal EntityContainerMapping(EntityContainer entityContainer, EntityContainer storageEntityContainer, StorageMappingItemCollection storageMappingItemCollection, bool validate, bool generateUpdateViews)
		: base(MetadataFlags.CSSpace)
	{
		Check.NotNull(entityContainer, "entityContainer");
		m_entityContainer = entityContainer;
		m_storageEntityContainer = storageEntityContainer;
		m_storageMappingItemCollection = storageMappingItemCollection;
		m_memoizedCellGroupEvaluator = new Memoizer<InputForComputingCellGroups, OutputFromComputeCellGroups>(ComputeCellGroups, default(InputForComputingCellGroups));
		identity = entityContainer.Identity;
		m_validate = validate;
		m_generateUpdateViews = generateUpdateViews;
	}

	internal EntityContainerMapping(EntityContainer entityContainer)
		: this(entityContainer, null, null, validate: false, generateUpdateViews: false)
	{
	}

	internal EntityContainerMapping()
	{
	}

	internal EntitySetBaseMapping GetEntitySetMapping(string setName)
	{
		EntitySetBaseMapping value = null;
		m_entitySetMappings.TryGetValue(setName, out value);
		return value;
	}

	internal EntitySetBaseMapping GetAssociationSetMapping(string setName)
	{
		EntitySetBaseMapping value = null;
		m_associationSetMappings.TryGetValue(setName, out value);
		return value;
	}

	internal IEnumerable<AssociationSetMapping> GetRelationshipSetMappingsFor(EntitySetBase edmEntitySet, EntitySetBase storeEntitySet)
	{
		return from AssociationSetMapping w in m_associationSetMappings.Values
			where w.StoreEntitySet != null && w.StoreEntitySet == storeEntitySet
			select w into associationSetMap
			where (associationSetMap.Set as AssociationSet).AssociationSetEnds.Any((AssociationSetEnd associationSetEnd) => associationSetEnd.EntitySet == edmEntitySet)
			select associationSetMap;
	}

	internal EntitySetBaseMapping GetSetMapping(string setName)
	{
		EntitySetBaseMapping entitySetBaseMapping = GetEntitySetMapping(setName);
		if (entitySetBaseMapping == null)
		{
			entitySetBaseMapping = GetAssociationSetMapping(setName);
		}
		return entitySetBaseMapping;
	}

	public void AddSetMapping(EntitySetMapping setMapping)
	{
		Check.NotNull(setMapping, "setMapping");
		Util.ThrowIfReadOnly(this);
		if (!m_entitySetMappings.ContainsKey(setMapping.Set.Name))
		{
			m_entitySetMappings.Add(setMapping.Set.Name, setMapping);
		}
	}

	public void RemoveSetMapping(EntitySetMapping setMapping)
	{
		Check.NotNull(setMapping, "setMapping");
		Util.ThrowIfReadOnly(this);
		m_entitySetMappings.Remove(setMapping.Set.Name);
	}

	public void AddSetMapping(AssociationSetMapping setMapping)
	{
		Check.NotNull(setMapping, "setMapping");
		Util.ThrowIfReadOnly(this);
		if (!m_associationSetMappings.ContainsKey(setMapping.Set.Name))
		{
			m_associationSetMappings.Add(setMapping.Set.Name, setMapping);
		}
	}

	public void RemoveSetMapping(AssociationSetMapping setMapping)
	{
		Check.NotNull(setMapping, "setMapping");
		Util.ThrowIfReadOnly(this);
		m_associationSetMappings.Remove(setMapping.Set.Name);
	}

	internal bool ContainsAssociationSetMapping(AssociationSet associationSet)
	{
		return m_associationSetMappings.ContainsKey(associationSet.Name);
	}

	public void AddFunctionImportMapping(FunctionImportMapping functionImportMapping)
	{
		Check.NotNull(functionImportMapping, "functionImportMapping");
		Util.ThrowIfReadOnly(this);
		m_functionImportMappings.Add(functionImportMapping.FunctionImport, functionImportMapping);
	}

	public void RemoveFunctionImportMapping(FunctionImportMapping functionImportMapping)
	{
		Check.NotNull(functionImportMapping, "functionImportMapping");
		Util.ThrowIfReadOnly(this);
		m_functionImportMappings.Remove(functionImportMapping.FunctionImport);
	}

	internal override void SetReadOnly()
	{
		MappingItem.SetReadOnly(m_entitySetMappings.Values);
		MappingItem.SetReadOnly(m_associationSetMappings.Values);
		MappingItem.SetReadOnly(m_functionImportMappings.Values);
		base.SetReadOnly();
	}

	internal bool HasQueryViewForSetMap(string setName)
	{
		EntitySetBaseMapping setMapping = GetSetMapping(setName);
		if (setMapping != null)
		{
			return setMapping.QueryView != null;
		}
		return false;
	}

	internal bool HasMappingFragments()
	{
		foreach (EntitySetBaseMapping allSetMap in AllSetMaps)
		{
			foreach (TypeMapping typeMapping in allSetMap.TypeMappings)
			{
				if (typeMapping.MappingFragments.Count > 0)
				{
					return true;
				}
			}
		}
		return false;
	}

	internal virtual bool TryGetFunctionImportMapping(EdmFunction functionImport, out FunctionImportMapping mapping)
	{
		return m_functionImportMappings.TryGetValue(functionImport, out mapping);
	}

	internal OutputFromComputeCellGroups GetCellgroups(InputForComputingCellGroups args)
	{
		return m_memoizedCellGroupEvaluator.Evaluate(args);
	}

	private OutputFromComputeCellGroups ComputeCellGroups(InputForComputingCellGroups args)
	{
		OutputFromComputeCellGroups result = default(OutputFromComputeCellGroups);
		result.Success = true;
		CellCreator cellCreator = new CellCreator(args.ContainerMapping);
		result.Cells = cellCreator.GenerateCells();
		result.Identifiers = cellCreator.Identifiers;
		if (result.Cells.Count <= 0)
		{
			result.Success = false;
			return result;
		}
		result.ForeignKeyConstraints = ForeignConstraint.GetForeignConstraints(args.ContainerMapping.StorageEntityContainer);
		List<Set<Cell>> source = new CellPartitioner(result.Cells, result.ForeignKeyConstraints).GroupRelatedCells();
		result.CellGroups = source.Select((Set<Cell> setOfCells) => new Set<Cell>(setOfCells.Select((Cell cell) => new Cell(cell)))).ToList();
		return result;
	}
}
