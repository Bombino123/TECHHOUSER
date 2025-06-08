using System.Collections.ObjectModel;

namespace System.Data.Entity.Core.Mapping;

public abstract class StructuralTypeMapping : MappingItem
{
	public abstract ReadOnlyCollection<PropertyMapping> PropertyMappings { get; }

	public abstract ReadOnlyCollection<ConditionPropertyMapping> Conditions { get; }

	public abstract void AddPropertyMapping(PropertyMapping propertyMapping);

	public abstract void RemovePropertyMapping(PropertyMapping propertyMapping);

	public abstract void AddCondition(ConditionPropertyMapping condition);

	public abstract void RemoveCondition(ConditionPropertyMapping condition);
}
