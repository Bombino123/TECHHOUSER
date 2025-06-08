using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity.Core.Common.Utils;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Utilities;
using System.Linq;

namespace System.Data.Entity.Core.Mapping;

public sealed class FunctionImportEntityTypeMapping : FunctionImportStructuralTypeMapping
{
	private readonly ReadOnlyCollection<EntityType> _entityTypes;

	private readonly ReadOnlyCollection<EntityType> _isOfTypeEntityTypes;

	private readonly ReadOnlyCollection<FunctionImportEntityTypeMappingCondition> _conditions;

	public ReadOnlyCollection<EntityType> EntityTypes => _entityTypes;

	public ReadOnlyCollection<EntityType> IsOfTypeEntityTypes => _isOfTypeEntityTypes;

	public ReadOnlyCollection<FunctionImportEntityTypeMappingCondition> Conditions => _conditions;

	public FunctionImportEntityTypeMapping(IEnumerable<EntityType> isOfTypeEntityTypes, IEnumerable<EntityType> entityTypes, Collection<FunctionImportReturnTypePropertyMapping> properties, IEnumerable<FunctionImportEntityTypeMappingCondition> conditions)
		: this(Check.NotNull(isOfTypeEntityTypes, "isOfTypeEntityTypes"), Check.NotNull(entityTypes, "entityTypes"), Check.NotNull(conditions, "conditions"), Check.NotNull(properties, "properties"), LineInfo.Empty)
	{
	}

	internal FunctionImportEntityTypeMapping(IEnumerable<EntityType> isOfTypeEntityTypes, IEnumerable<EntityType> entityTypes, IEnumerable<FunctionImportEntityTypeMappingCondition> conditions, Collection<FunctionImportReturnTypePropertyMapping> columnsRenameList, LineInfo lineInfo)
		: base(columnsRenameList, lineInfo)
	{
		_isOfTypeEntityTypes = new ReadOnlyCollection<EntityType>(isOfTypeEntityTypes.ToList());
		_entityTypes = new ReadOnlyCollection<EntityType>(entityTypes.ToList());
		_conditions = new ReadOnlyCollection<FunctionImportEntityTypeMappingCondition>(conditions.ToList());
	}

	internal override void SetReadOnly()
	{
		MappingItem.SetReadOnly(_conditions);
		base.SetReadOnly();
	}

	internal IEnumerable<EntityType> GetMappedEntityTypes(ItemCollection itemCollection)
	{
		return EntityTypes.Concat(IsOfTypeEntityTypes.SelectMany((EntityType entityType) => MetadataHelper.GetTypeAndSubtypesOf(entityType, itemCollection, includeAbstractTypes: false).Cast<EntityType>()));
	}

	internal IEnumerable<string> GetDiscriminatorColumns()
	{
		return Conditions.Select((FunctionImportEntityTypeMappingCondition condition) => condition.ColumnName);
	}
}
