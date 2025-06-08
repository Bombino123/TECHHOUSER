namespace System.Data.Entity.Core.Query.PlanCompiler;

internal class NestedPropertyRef : PropertyRef
{
	private readonly PropertyRef m_inner;

	private readonly PropertyRef m_outer;

	internal PropertyRef OuterProperty => m_outer;

	internal PropertyRef InnerProperty => m_inner;

	internal NestedPropertyRef(PropertyRef innerProperty, PropertyRef outerProperty)
	{
		PlanCompiler.Assert(!(innerProperty is NestedPropertyRef), "innerProperty cannot be a NestedPropertyRef");
		m_inner = innerProperty;
		m_outer = outerProperty;
	}

	public override bool Equals(object obj)
	{
		if (obj is NestedPropertyRef nestedPropertyRef && m_inner.Equals(nestedPropertyRef.m_inner))
		{
			return m_outer.Equals(nestedPropertyRef.m_outer);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return m_inner.GetHashCode() ^ m_outer.GetHashCode();
	}

	public override string ToString()
	{
		return m_inner?.ToString() + "." + m_outer;
	}
}
