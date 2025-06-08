using System.Collections.Generic;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Utilities;
using System.Globalization;
using System.Linq;

namespace System.Data.Entity.Core.Mapping;

public sealed class EntityTypeModificationFunctionMapping : MappingItem
{
	private readonly EntityType _entityType;

	private readonly ModificationFunctionMapping _deleteFunctionMapping;

	private readonly ModificationFunctionMapping _insertFunctionMapping;

	private readonly ModificationFunctionMapping _updateFunctionMapping;

	public EntityType EntityType => _entityType;

	public ModificationFunctionMapping DeleteFunctionMapping => _deleteFunctionMapping;

	public ModificationFunctionMapping InsertFunctionMapping => _insertFunctionMapping;

	public ModificationFunctionMapping UpdateFunctionMapping => _updateFunctionMapping;

	internal IEnumerable<ModificationFunctionParameterBinding> PrimaryParameterBindings
	{
		get
		{
			IEnumerable<ModificationFunctionParameterBinding> enumerable = Enumerable.Empty<ModificationFunctionParameterBinding>();
			if (DeleteFunctionMapping != null)
			{
				enumerable = enumerable.Concat(DeleteFunctionMapping.ParameterBindings);
			}
			if (InsertFunctionMapping != null)
			{
				enumerable = enumerable.Concat(InsertFunctionMapping.ParameterBindings);
			}
			if (UpdateFunctionMapping != null)
			{
				enumerable = enumerable.Concat(UpdateFunctionMapping.ParameterBindings.Where((ModificationFunctionParameterBinding pb) => pb.IsCurrent));
			}
			return enumerable;
		}
	}

	public EntityTypeModificationFunctionMapping(EntityType entityType, ModificationFunctionMapping deleteFunctionMapping, ModificationFunctionMapping insertFunctionMapping, ModificationFunctionMapping updateFunctionMapping)
	{
		Check.NotNull(entityType, "entityType");
		_entityType = entityType;
		_deleteFunctionMapping = deleteFunctionMapping;
		_insertFunctionMapping = insertFunctionMapping;
		_updateFunctionMapping = updateFunctionMapping;
	}

	public override string ToString()
	{
		return string.Format(CultureInfo.InvariantCulture, "ET{{{0}}}:{4}DFunc={{{1}}},{4}IFunc={{{2}}},{4}UFunc={{{3}}}", EntityType, DeleteFunctionMapping, InsertFunctionMapping, UpdateFunctionMapping, Environment.NewLine + "  ");
	}

	internal override void SetReadOnly()
	{
		MappingItem.SetReadOnly(_deleteFunctionMapping);
		MappingItem.SetReadOnly(_insertFunctionMapping);
		MappingItem.SetReadOnly(_updateFunctionMapping);
		base.SetReadOnly();
	}
}
