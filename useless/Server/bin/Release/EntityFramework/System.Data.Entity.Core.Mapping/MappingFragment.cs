using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Resources;
using System.Data.Entity.Utilities;
using System.Linq;

namespace System.Data.Entity.Core.Mapping;

public class MappingFragment : StructuralTypeMapping
{
	private readonly List<ColumnMappingBuilder> _columnMappings = new List<ColumnMappingBuilder>();

	private EntitySet m_tableExtent;

	private readonly TypeMapping m_typeMapping;

	private readonly Dictionary<EdmProperty, ConditionPropertyMapping> m_conditionProperties = new Dictionary<EdmProperty, ConditionPropertyMapping>(EqualityComparer<EdmProperty>.Default);

	private readonly List<PropertyMapping> m_properties = new List<PropertyMapping>();

	private readonly bool m_isSQueryDistinct;

	internal IEnumerable<ColumnMappingBuilder> ColumnMappings => _columnMappings;

	public EntitySet StoreEntitySet
	{
		get
		{
			return m_tableExtent;
		}
		internal set
		{
			m_tableExtent = value;
		}
	}

	internal EntitySet TableSet
	{
		get
		{
			return StoreEntitySet;
		}
		set
		{
			StoreEntitySet = value;
		}
	}

	internal EntityType Table => m_tableExtent.ElementType;

	public TypeMapping TypeMapping => m_typeMapping;

	public bool MakeColumnsDistinct => m_isSQueryDistinct;

	internal bool IsSQueryDistinct => MakeColumnsDistinct;

	internal ReadOnlyCollection<PropertyMapping> AllProperties
	{
		get
		{
			List<PropertyMapping> list = new List<PropertyMapping>();
			list.AddRange(m_properties);
			list.AddRange(m_conditionProperties.Values);
			return new ReadOnlyCollection<PropertyMapping>(list);
		}
	}

	public override ReadOnlyCollection<PropertyMapping> PropertyMappings => new ReadOnlyCollection<PropertyMapping>(m_properties);

	public override ReadOnlyCollection<ConditionPropertyMapping> Conditions => new ReadOnlyCollection<ConditionPropertyMapping>(new List<ConditionPropertyMapping>(m_conditionProperties.Values));

	internal IEnumerable<ColumnMappingBuilder> FlattenedProperties => GetFlattenedProperties(m_properties, new List<EdmProperty>());

	internal IEnumerable<ConditionPropertyMapping> ColumnConditions => m_conditionProperties.Values;

	internal int StartLineNumber { get; set; }

	internal int StartLinePosition { get; set; }

	internal string SourceLocation => m_typeMapping.SetMapping.EntityContainerMapping.SourceLocation;

	public MappingFragment(EntitySet storeEntitySet, TypeMapping typeMapping, bool makeColumnsDistinct)
	{
		Check.NotNull(storeEntitySet, "storeEntitySet");
		Check.NotNull(typeMapping, "typeMapping");
		m_tableExtent = storeEntitySet;
		m_typeMapping = typeMapping;
		m_isSQueryDistinct = makeColumnsDistinct;
	}

	internal void AddColumnMapping(ColumnMappingBuilder columnMappingBuilder)
	{
		Check.NotNull(columnMappingBuilder, "columnMappingBuilder");
		if (!columnMappingBuilder.PropertyPath.Any() || _columnMappings.Contains(columnMappingBuilder))
		{
			throw new ArgumentException(Strings.InvalidColumnBuilderArgument("columnBuilderMapping"));
		}
		_columnMappings.Add(columnMappingBuilder);
		StructuralTypeMapping structuralTypeMapping = this;
		int i;
		EdmProperty property;
		for (i = 0; i < columnMappingBuilder.PropertyPath.Count - 1; i++)
		{
			property = columnMappingBuilder.PropertyPath[i];
			ComplexPropertyMapping complexPropertyMapping = structuralTypeMapping.PropertyMappings.OfType<ComplexPropertyMapping>().SingleOrDefault((ComplexPropertyMapping pm) => pm.Property == property);
			ComplexTypeMapping complexTypeMapping = null;
			if (complexPropertyMapping == null)
			{
				complexTypeMapping = new ComplexTypeMapping(isPartial: false);
				complexTypeMapping.AddType(property.ComplexType);
				complexPropertyMapping = new ComplexPropertyMapping(property);
				complexPropertyMapping.AddTypeMapping(complexTypeMapping);
				structuralTypeMapping.AddPropertyMapping(complexPropertyMapping);
			}
			structuralTypeMapping = complexTypeMapping ?? complexPropertyMapping.TypeMappings.Single();
		}
		property = columnMappingBuilder.PropertyPath[i];
		ScalarPropertyMapping scalarPropertyMapping = structuralTypeMapping.PropertyMappings.OfType<ScalarPropertyMapping>().SingleOrDefault((ScalarPropertyMapping pm) => pm.Property == property);
		if (scalarPropertyMapping == null)
		{
			scalarPropertyMapping = new ScalarPropertyMapping(property, columnMappingBuilder.ColumnProperty);
			structuralTypeMapping.AddPropertyMapping(scalarPropertyMapping);
			columnMappingBuilder.SetTarget(scalarPropertyMapping);
		}
		else
		{
			scalarPropertyMapping.Column = columnMappingBuilder.ColumnProperty;
		}
	}

	internal void RemoveColumnMapping(ColumnMappingBuilder columnMappingBuilder)
	{
		_columnMappings.Remove(columnMappingBuilder);
		RemoveColumnMapping(this, columnMappingBuilder.PropertyPath);
	}

	private static void RemoveColumnMapping(StructuralTypeMapping structuralTypeMapping, IEnumerable<EdmProperty> propertyPath)
	{
		PropertyMapping propertyMapping = structuralTypeMapping.PropertyMappings.Single((PropertyMapping pm) => pm.Property == propertyPath.First());
		if (propertyMapping is ScalarPropertyMapping)
		{
			structuralTypeMapping.RemovePropertyMapping(propertyMapping);
			return;
		}
		ComplexPropertyMapping complexPropertyMapping = (ComplexPropertyMapping)propertyMapping;
		ComplexTypeMapping complexTypeMapping = complexPropertyMapping.TypeMappings.Single();
		RemoveColumnMapping(complexTypeMapping, propertyPath.Skip(1));
		if (!complexTypeMapping.PropertyMappings.Any())
		{
			structuralTypeMapping.RemovePropertyMapping(complexPropertyMapping);
		}
	}

	private static IEnumerable<ColumnMappingBuilder> GetFlattenedProperties(IEnumerable<PropertyMapping> propertyMappings, List<EdmProperty> propertyPath)
	{
		foreach (PropertyMapping propertyMapping in propertyMappings)
		{
			propertyPath.Add(propertyMapping.Property);
			if (propertyMapping is ComplexPropertyMapping complexPropertyMapping)
			{
				foreach (ColumnMappingBuilder flattenedProperty in GetFlattenedProperties(complexPropertyMapping.TypeMappings.Single().PropertyMappings, propertyPath))
				{
					yield return flattenedProperty;
				}
			}
			else if (propertyMapping is ScalarPropertyMapping scalarPropertyMapping)
			{
				yield return new ColumnMappingBuilder(scalarPropertyMapping.Column, propertyPath.ToList());
			}
			propertyPath.Remove(propertyMapping.Property);
		}
	}

	public override void AddPropertyMapping(PropertyMapping propertyMapping)
	{
		Check.NotNull(propertyMapping, "propertyMapping");
		ThrowIfReadOnly();
		m_properties.Add(propertyMapping);
	}

	public override void RemovePropertyMapping(PropertyMapping propertyMapping)
	{
		Check.NotNull(propertyMapping, "propertyMapping");
		ThrowIfReadOnly();
		m_properties.Remove(propertyMapping);
	}

	public override void AddCondition(ConditionPropertyMapping condition)
	{
		Check.NotNull(condition, "condition");
		ThrowIfReadOnly();
		AddConditionProperty(condition);
	}

	public override void RemoveCondition(ConditionPropertyMapping condition)
	{
		Check.NotNull(condition, "condition");
		ThrowIfReadOnly();
		RemoveConditionProperty(condition);
	}

	internal void ClearConditions()
	{
		m_conditionProperties.Clear();
	}

	internal override void SetReadOnly()
	{
		m_properties.TrimExcess();
		MappingItem.SetReadOnly(m_properties);
		MappingItem.SetReadOnly(m_conditionProperties.Values);
		base.SetReadOnly();
	}

	internal void RemoveConditionProperty(ConditionPropertyMapping condition)
	{
		EdmProperty key = condition.Property ?? condition.Column;
		m_conditionProperties.Remove(key);
	}

	internal void AddConditionProperty(ConditionPropertyMapping conditionPropertyMap)
	{
		AddConditionProperty(conditionPropertyMap, delegate
		{
		});
	}

	internal void AddConditionProperty(ConditionPropertyMapping conditionPropertyMap, Action<EdmMember> duplicateMemberConditionError)
	{
		EdmProperty edmProperty = conditionPropertyMap.Property ?? conditionPropertyMap.Column;
		if (!m_conditionProperties.ContainsKey(edmProperty))
		{
			m_conditionProperties.Add(edmProperty, conditionPropertyMap);
		}
		else
		{
			duplicateMemberConditionError(edmProperty);
		}
	}
}
