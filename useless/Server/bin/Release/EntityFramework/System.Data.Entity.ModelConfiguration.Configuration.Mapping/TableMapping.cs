using System.Collections.Generic;
using System.Data.Entity.Core.Mapping;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.ModelConfiguration.Edm;
using System.Diagnostics;
using System.Linq;

namespace System.Data.Entity.ModelConfiguration.Configuration.Mapping;

[DebuggerDisplay("{Table.Name}")]
internal class TableMapping
{
	private readonly EntityType _table;

	private readonly SortedEntityTypeIndex _entityTypes;

	private readonly List<ColumnMapping> _columns;

	public EntityType Table => _table;

	public SortedEntityTypeIndex EntityTypes => _entityTypes;

	public IEnumerable<ColumnMapping> ColumnMappings => _columns;

	public TableMapping(EntityType table)
	{
		_table = table;
		_entityTypes = new SortedEntityTypeIndex();
		_columns = new List<ColumnMapping>();
	}

	public void AddEntityTypeMappingFragment(EntitySet entitySet, EntityType entityType, MappingFragment fragment)
	{
		_entityTypes.Add(entitySet, entityType);
		EdmProperty defaultDiscriminator = fragment.GetDefaultDiscriminator();
		foreach (ColumnMappingBuilder cm in fragment.ColumnMappings)
		{
			FindOrCreateColumnMapping(cm.ColumnProperty).AddMapping(entityType, cm.PropertyPath, fragment.ColumnConditions.Where((ConditionPropertyMapping cc) => cc.Column == cm.ColumnProperty), defaultDiscriminator == cm.ColumnProperty);
		}
		foreach (ConditionPropertyMapping item in fragment.ColumnConditions.Where((ConditionPropertyMapping cc) => fragment.ColumnMappings.All((ColumnMappingBuilder pm) => pm.ColumnProperty != cc.Column)))
		{
			FindOrCreateColumnMapping(item.Column).AddMapping(entityType, null, new ConditionPropertyMapping[1] { item }, defaultDiscriminator == item.Column);
		}
	}

	private ColumnMapping FindOrCreateColumnMapping(EdmProperty column)
	{
		ColumnMapping columnMapping = _columns.SingleOrDefault((ColumnMapping c) => c.Column == column);
		if (columnMapping == null)
		{
			columnMapping = new ColumnMapping(column);
			_columns.Add(columnMapping);
		}
		return columnMapping;
	}
}
