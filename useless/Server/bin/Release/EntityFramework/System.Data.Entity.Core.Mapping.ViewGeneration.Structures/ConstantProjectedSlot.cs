using System.Data.Entity.Core.Common.CommandTrees;
using System.Data.Entity.Core.Mapping.ViewGeneration.CqlGeneration;
using System.Text;

namespace System.Data.Entity.Core.Mapping.ViewGeneration.Structures;

internal sealed class ConstantProjectedSlot : ProjectedSlot
{
	private readonly Constant m_constant;

	internal Constant CellConstant => m_constant;

	internal ConstantProjectedSlot(Constant value)
	{
		m_constant = value;
	}

	internal override ProjectedSlot DeepQualify(CqlBlock block)
	{
		return this;
	}

	internal override StringBuilder AsEsql(StringBuilder builder, MemberPath outputMember, string blockAlias, int indentLevel)
	{
		return m_constant.AsEsql(builder, outputMember, blockAlias);
	}

	internal override DbExpression AsCqt(DbExpression row, MemberPath outputMember)
	{
		return m_constant.AsCqt(row, outputMember);
	}

	protected override bool IsEqualTo(ProjectedSlot right)
	{
		if (!(right is ConstantProjectedSlot constantProjectedSlot))
		{
			return false;
		}
		return Constant.EqualityComparer.Equals(m_constant, constantProjectedSlot.m_constant);
	}

	protected override int GetHash()
	{
		return Constant.EqualityComparer.GetHashCode(m_constant);
	}

	internal override void ToCompactString(StringBuilder builder)
	{
		m_constant.ToCompactString(builder);
	}
}
