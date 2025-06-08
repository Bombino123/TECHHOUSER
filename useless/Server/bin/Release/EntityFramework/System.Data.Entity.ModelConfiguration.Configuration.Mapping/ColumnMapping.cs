using System.Collections.Generic;
using System.Data.Entity.Core.Mapping;
using System.Data.Entity.Core.Metadata.Edm;
using System.Diagnostics;
using System.Linq;

namespace System.Data.Entity.ModelConfiguration.Configuration.Mapping;

[DebuggerDisplay("{Column.Name}")]
internal class ColumnMapping
{
	private readonly EdmProperty _column;

	private readonly List<PropertyMappingSpecification> _propertyMappings;

	public EdmProperty Column => _column;

	public IList<PropertyMappingSpecification> PropertyMappings => _propertyMappings;

	public ColumnMapping(EdmProperty column)
	{
		_column = column;
		_propertyMappings = new List<PropertyMappingSpecification>();
	}

	public void AddMapping(EntityType entityType, IList<EdmProperty> propertyPath, IEnumerable<ConditionPropertyMapping> conditions, bool isDefaultDiscriminatorCondition)
	{
		_propertyMappings.Add(new PropertyMappingSpecification(entityType, propertyPath, conditions.ToList(), isDefaultDiscriminatorCondition));
	}
}
