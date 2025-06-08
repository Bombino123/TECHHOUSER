using System.Collections.Generic;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Core.Query.InternalTrees;

namespace System.Data.Entity.Core.Query.PlanCompiler;

internal class RootTypeInfo : TypeInfo
{
	private readonly List<PropertyRef> m_propertyRefList;

	private readonly Dictionary<PropertyRef, EdmProperty> m_propertyMap;

	private EdmProperty m_nullSentinelProperty;

	private EdmProperty m_typeIdProperty;

	private readonly ExplicitDiscriminatorMap m_discriminatorMap;

	private EdmProperty m_entitySetIdProperty;

	private RowType m_flattenedType;

	private TypeUsage m_flattenedTypeUsage;

	internal TypeIdKind TypeIdKind { get; set; }

	internal TypeUsage TypeIdType { get; set; }

	internal new RowType FlattenedType
	{
		get
		{
			return m_flattenedType;
		}
		set
		{
			m_flattenedType = value;
			m_flattenedTypeUsage = TypeUsage.Create(value);
		}
	}

	internal new TypeUsage FlattenedTypeUsage => m_flattenedTypeUsage;

	internal ExplicitDiscriminatorMap DiscriminatorMap => m_discriminatorMap;

	internal new EdmProperty EntitySetIdProperty => m_entitySetIdProperty;

	internal new EdmProperty NullSentinelProperty => m_nullSentinelProperty;

	internal new IEnumerable<PropertyRef> PropertyRefList => m_propertyRefList;

	internal new EdmProperty TypeIdProperty => m_typeIdProperty;

	internal RootTypeInfo(TypeUsage type, ExplicitDiscriminatorMap discriminatorMap)
		: base(type, null)
	{
		PlanCompiler.Assert(type.EdmType.BaseType == null, "only root types allowed here");
		m_propertyMap = new Dictionary<PropertyRef, EdmProperty>();
		m_propertyRefList = new List<PropertyRef>();
		m_discriminatorMap = discriminatorMap;
		TypeIdKind = TypeIdKind.Generated;
	}

	internal void AddPropertyMapping(PropertyRef propertyRef, EdmProperty newProperty)
	{
		m_propertyMap[propertyRef] = newProperty;
		if (propertyRef is TypeIdPropertyRef)
		{
			m_typeIdProperty = newProperty;
		}
		else if (propertyRef is EntitySetIdPropertyRef)
		{
			m_entitySetIdProperty = newProperty;
		}
		else if (propertyRef is NullSentinelPropertyRef)
		{
			m_nullSentinelProperty = newProperty;
		}
	}

	internal void AddPropertyRef(PropertyRef propertyRef)
	{
		m_propertyRefList.Add(propertyRef);
	}

	internal int GetNestedStructureOffset(PropertyRef property)
	{
		for (int i = 0; i < m_propertyRefList.Count; i++)
		{
			if (m_propertyRefList[i] is NestedPropertyRef nestedPropertyRef && nestedPropertyRef.InnerProperty.Equals(property))
			{
				return i;
			}
		}
		PlanCompiler.Assert(condition: false, "no complex structure " + property?.ToString() + " found in TypeInfo");
		return 0;
	}

	internal new bool TryGetNewProperty(PropertyRef propertyRef, bool throwIfMissing, out EdmProperty property)
	{
		bool flag = m_propertyMap.TryGetValue(propertyRef, out property);
		if (throwIfMissing && !flag)
		{
			PlanCompiler.Assert(condition: false, "Unable to find property " + propertyRef?.ToString() + " in type " + base.Type.EdmType.Identity);
		}
		return flag;
	}
}
