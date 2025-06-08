using System.Data.Entity.Core.Query.InternalTrees;

namespace System.Data.Entity.Core.Query.PlanCompiler;

internal class RelPropertyRef : PropertyRef
{
	private readonly RelProperty m_property;

	internal RelProperty Property => m_property;

	internal RelPropertyRef(RelProperty property)
	{
		m_property = property;
	}

	public override bool Equals(object obj)
	{
		if (obj is RelPropertyRef relPropertyRef)
		{
			return m_property.Equals(relPropertyRef.m_property);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return m_property.GetHashCode();
	}

	public override string ToString()
	{
		return m_property.ToString();
	}
}
