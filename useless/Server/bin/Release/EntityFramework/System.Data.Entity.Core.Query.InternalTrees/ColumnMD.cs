using System.Data.Entity.Core.Metadata.Edm;

namespace System.Data.Entity.Core.Query.InternalTrees;

internal class ColumnMD
{
	private readonly string m_name;

	private readonly TypeUsage m_type;

	private readonly EdmMember m_property;

	internal string Name => m_name;

	internal TypeUsage Type => m_type;

	internal bool IsNullable
	{
		get
		{
			if (m_property != null)
			{
				return TypeSemantics.IsNullable(m_property);
			}
			return true;
		}
	}

	internal ColumnMD(string name, TypeUsage type)
	{
		m_name = name;
		m_type = type;
	}

	internal ColumnMD(EdmMember property)
		: this(property.Name, property.TypeUsage)
	{
		m_property = property;
	}

	public override string ToString()
	{
		return m_name;
	}
}
