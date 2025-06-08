using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Utilities;
using System.Linq;

namespace System.Data.Entity.Core.Mapping;

public class ComplexTypeMapping : StructuralTypeMapping
{
	private readonly Dictionary<string, PropertyMapping> m_properties = new Dictionary<string, PropertyMapping>(StringComparer.Ordinal);

	private readonly Dictionary<EdmProperty, ConditionPropertyMapping> m_conditionProperties = new Dictionary<EdmProperty, ConditionPropertyMapping>(EqualityComparer<EdmProperty>.Default);

	private readonly Dictionary<string, ComplexType> m_types = new Dictionary<string, ComplexType>(StringComparer.Ordinal);

	private readonly Dictionary<string, ComplexType> m_isOfTypes = new Dictionary<string, ComplexType>(StringComparer.Ordinal);

	public ComplexType ComplexType => m_types.Values.SingleOrDefault();

	internal ReadOnlyCollection<ComplexType> Types => new ReadOnlyCollection<ComplexType>(new List<ComplexType>(m_types.Values));

	internal ReadOnlyCollection<ComplexType> IsOfTypes => new ReadOnlyCollection<ComplexType>(new List<ComplexType>(m_isOfTypes.Values));

	public override ReadOnlyCollection<PropertyMapping> PropertyMappings => new ReadOnlyCollection<PropertyMapping>(new List<PropertyMapping>(m_properties.Values));

	public override ReadOnlyCollection<ConditionPropertyMapping> Conditions => new ReadOnlyCollection<ConditionPropertyMapping>(new List<ConditionPropertyMapping>(m_conditionProperties.Values));

	internal ReadOnlyCollection<PropertyMapping> AllProperties
	{
		get
		{
			List<PropertyMapping> list = new List<PropertyMapping>();
			list.AddRange(m_properties.Values);
			list.AddRange(m_conditionProperties.Values);
			return new ReadOnlyCollection<PropertyMapping>(list);
		}
	}

	public ComplexTypeMapping(ComplexType complexType)
	{
		Check.NotNull(complexType, "complexType");
		AddType(complexType);
	}

	internal ComplexTypeMapping(bool isPartial)
	{
	}

	internal void AddType(ComplexType type)
	{
		m_types.Add(type.FullName, type);
	}

	internal void AddIsOfType(ComplexType type)
	{
		m_isOfTypes.Add(type.FullName, type);
	}

	public override void AddPropertyMapping(PropertyMapping propertyMapping)
	{
		Check.NotNull(propertyMapping, "propertyMapping");
		ThrowIfReadOnly();
		m_properties.Add(propertyMapping.Property.Name, propertyMapping);
	}

	public override void RemovePropertyMapping(PropertyMapping propertyMapping)
	{
		Check.NotNull(propertyMapping, "propertyMapping");
		ThrowIfReadOnly();
		m_properties.Remove(propertyMapping.Property.Name);
	}

	public override void AddCondition(ConditionPropertyMapping condition)
	{
		Check.NotNull(condition, "condition");
		ThrowIfReadOnly();
		AddConditionProperty(condition, delegate
		{
		});
	}

	public override void RemoveCondition(ConditionPropertyMapping condition)
	{
		Check.NotNull(condition, "condition");
		ThrowIfReadOnly();
		m_conditionProperties.Remove(condition.Property ?? condition.Column);
	}

	internal override void SetReadOnly()
	{
		MappingItem.SetReadOnly(m_properties.Values);
		MappingItem.SetReadOnly(m_conditionProperties.Values);
		base.SetReadOnly();
	}

	internal void AddConditionProperty(ConditionPropertyMapping conditionPropertyMap, Action<EdmMember> duplicateMemberConditionError)
	{
		EdmProperty edmProperty = ((conditionPropertyMap.Property != null) ? conditionPropertyMap.Property : conditionPropertyMap.Column);
		if (!m_conditionProperties.ContainsKey(edmProperty))
		{
			m_conditionProperties.Add(edmProperty, conditionPropertyMap);
		}
		else
		{
			duplicateMemberConditionError(edmProperty);
		}
	}

	internal ComplexType GetOwnerType(string memberName)
	{
		foreach (ComplexType value in m_types.Values)
		{
			if (value.Members.TryGetValue(memberName, ignoreCase: false, out var item) && item is EdmProperty)
			{
				return value;
			}
		}
		foreach (ComplexType value2 in m_isOfTypes.Values)
		{
			if (value2.Members.TryGetValue(memberName, ignoreCase: false, out var item2) && item2 is EdmProperty)
			{
				return value2;
			}
		}
		return null;
	}
}
