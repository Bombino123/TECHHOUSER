using System.Collections.Generic;

namespace System.Data.Entity.Core.Query.PlanCompiler;

internal class PropertyRefList
{
	private readonly Dictionary<PropertyRef, PropertyRef> m_propertyReferences;

	private bool m_allProperties;

	internal static PropertyRefList All = new PropertyRefList(allProps: true);

	internal bool AllProperties => m_allProperties;

	internal IEnumerable<PropertyRef> Properties => m_propertyReferences.Keys;

	internal PropertyRefList()
		: this(allProps: false)
	{
	}

	private PropertyRefList(bool allProps)
	{
		m_propertyReferences = new Dictionary<PropertyRef, PropertyRef>();
		if (allProps)
		{
			MakeAllProperties();
		}
	}

	private void MakeAllProperties()
	{
		m_allProperties = true;
		m_propertyReferences.Clear();
		m_propertyReferences.Add(AllPropertyRef.Instance, AllPropertyRef.Instance);
	}

	internal void Add(PropertyRef property)
	{
		if (!m_allProperties)
		{
			if (property is AllPropertyRef)
			{
				MakeAllProperties();
			}
			else
			{
				m_propertyReferences[property] = property;
			}
		}
	}

	internal void Append(PropertyRefList propertyRefs)
	{
		if (m_allProperties)
		{
			return;
		}
		foreach (PropertyRef key in propertyRefs.m_propertyReferences.Keys)
		{
			Add(key);
		}
	}

	internal PropertyRefList Clone()
	{
		PropertyRefList propertyRefList = new PropertyRefList(m_allProperties);
		foreach (PropertyRef key in m_propertyReferences.Keys)
		{
			propertyRefList.Add(key);
		}
		return propertyRefList;
	}

	internal bool Contains(PropertyRef p)
	{
		if (!m_allProperties)
		{
			return m_propertyReferences.ContainsKey(p);
		}
		return true;
	}

	public override string ToString()
	{
		string text = "{";
		foreach (PropertyRef key in m_propertyReferences.Keys)
		{
			text = text + key?.ToString() + ",";
		}
		return text + "}";
	}
}
