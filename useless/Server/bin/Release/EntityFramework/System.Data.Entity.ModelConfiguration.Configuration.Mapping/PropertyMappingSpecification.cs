using System.Collections.Generic;
using System.Data.Entity.Core.Mapping;
using System.Data.Entity.Core.Metadata.Edm;

namespace System.Data.Entity.ModelConfiguration.Configuration.Mapping;

internal class PropertyMappingSpecification
{
	private readonly EntityType _entityType;

	private readonly IList<EdmProperty> _propertyPath;

	private readonly IList<ConditionPropertyMapping> _conditions;

	private readonly bool _isDefaultDiscriminatorCondition;

	public EntityType EntityType => _entityType;

	public IList<EdmProperty> PropertyPath => _propertyPath;

	public IList<ConditionPropertyMapping> Conditions => _conditions;

	public bool IsDefaultDiscriminatorCondition => _isDefaultDiscriminatorCondition;

	public PropertyMappingSpecification(EntityType entityType, IList<EdmProperty> propertyPath, IList<ConditionPropertyMapping> conditions, bool isDefaultDiscriminatorCondition)
	{
		_entityType = entityType;
		_propertyPath = propertyPath;
		_conditions = conditions;
		_isDefaultDiscriminatorCondition = isDefaultDiscriminatorCondition;
	}
}
