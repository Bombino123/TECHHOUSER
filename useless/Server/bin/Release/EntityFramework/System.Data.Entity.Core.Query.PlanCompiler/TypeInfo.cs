using System.Collections.Generic;
using System.Data.Entity.Core.Common;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Core.Query.InternalTrees;

namespace System.Data.Entity.Core.Query.PlanCompiler;

internal class TypeInfo
{
	private readonly TypeUsage m_type;

	private readonly List<TypeInfo> m_immediateSubTypes;

	private readonly TypeInfo m_superType;

	private readonly RootTypeInfo m_rootType;

	internal bool IsRootType => m_rootType == null;

	internal List<TypeInfo> ImmediateSubTypes => m_immediateSubTypes;

	internal TypeInfo SuperType => m_superType;

	internal RootTypeInfo RootType => m_rootType ?? ((RootTypeInfo)this);

	internal TypeUsage Type => m_type;

	internal object TypeId { get; set; }

	internal virtual RowType FlattenedType => RootType.FlattenedType;

	internal virtual TypeUsage FlattenedTypeUsage => RootType.FlattenedTypeUsage;

	internal virtual EdmProperty EntitySetIdProperty => RootType.EntitySetIdProperty;

	internal bool HasEntitySetIdProperty => RootType.EntitySetIdProperty != null;

	internal virtual EdmProperty NullSentinelProperty => RootType.NullSentinelProperty;

	internal bool HasNullSentinelProperty => RootType.NullSentinelProperty != null;

	internal virtual EdmProperty TypeIdProperty => RootType.TypeIdProperty;

	internal bool HasTypeIdProperty => RootType.TypeIdProperty != null;

	internal virtual IEnumerable<PropertyRef> PropertyRefList => RootType.PropertyRefList;

	internal static TypeInfo Create(TypeUsage type, TypeInfo superTypeInfo, ExplicitDiscriminatorMap discriminatorMap)
	{
		if (superTypeInfo == null)
		{
			return new RootTypeInfo(type, discriminatorMap);
		}
		return new TypeInfo(type, superTypeInfo);
	}

	protected TypeInfo(TypeUsage type, TypeInfo superType)
	{
		m_type = type;
		m_immediateSubTypes = new List<TypeInfo>();
		m_superType = superType;
		if (superType != null)
		{
			superType.m_immediateSubTypes.Add(this);
			m_rootType = superType.RootType;
		}
	}

	internal EdmProperty GetNewProperty(PropertyRef propertyRef)
	{
		TryGetNewProperty(propertyRef, throwIfMissing: true, out var newProperty);
		return newProperty;
	}

	internal bool TryGetNewProperty(PropertyRef propertyRef, bool throwIfMissing, out EdmProperty newProperty)
	{
		return RootType.TryGetNewProperty(propertyRef, throwIfMissing, out newProperty);
	}

	internal IEnumerable<PropertyRef> GetKeyPropertyRefs()
	{
		RefType type = null;
		EntityTypeBase entityTypeBase = ((!TypeHelpers.TryGetEdmType<RefType>(m_type, out type)) ? TypeHelpers.GetEdmType<EntityTypeBase>(m_type) : type.ElementType);
		foreach (EdmMember keyMember in entityTypeBase.KeyMembers)
		{
			PlanCompiler.Assert(keyMember is EdmProperty, "Non-EdmProperty key members are not supported");
			yield return new SimplePropertyRef(keyMember);
		}
	}

	internal IEnumerable<PropertyRef> GetIdentityPropertyRefs()
	{
		if (HasEntitySetIdProperty)
		{
			yield return EntitySetIdPropertyRef.Instance;
		}
		foreach (PropertyRef keyPropertyRef in GetKeyPropertyRefs())
		{
			yield return keyPropertyRef;
		}
	}

	internal IEnumerable<PropertyRef> GetAllPropertyRefs()
	{
		foreach (PropertyRef propertyRef in PropertyRefList)
		{
			yield return propertyRef;
		}
	}

	internal IEnumerable<EdmProperty> GetAllProperties()
	{
		foreach (EdmProperty property in FlattenedType.Properties)
		{
			yield return property;
		}
	}

	internal List<TypeInfo> GetTypeHierarchy()
	{
		List<TypeInfo> result = new List<TypeInfo>();
		GetTypeHierarchy(result);
		return result;
	}

	private void GetTypeHierarchy(List<TypeInfo> result)
	{
		result.Add(this);
		foreach (TypeInfo immediateSubType in ImmediateSubTypes)
		{
			immediateSubType.GetTypeHierarchy(result);
		}
	}
}
