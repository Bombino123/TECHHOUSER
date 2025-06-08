using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Core.Query.InternalTrees;

namespace System.Data.Entity.Core.Query.PlanCompiler;

internal class SimplePropertyRef : PropertyRef
{
	private readonly EdmMember m_property;

	internal EdmMember Property => m_property;

	internal SimplePropertyRef(EdmMember property)
	{
		m_property = property;
	}

	public override bool Equals(object obj)
	{
		if (obj is SimplePropertyRef simplePropertyRef && Command.EqualTypes(m_property.DeclaringType, simplePropertyRef.m_property.DeclaringType))
		{
			return simplePropertyRef.m_property.Name.Equals(m_property.Name);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return m_property.Name.GetHashCode();
	}

	public override string ToString()
	{
		return m_property.Name;
	}
}
