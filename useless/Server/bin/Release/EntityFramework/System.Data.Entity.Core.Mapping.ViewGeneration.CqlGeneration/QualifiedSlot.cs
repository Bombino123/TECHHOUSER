using System.Data.Entity.Core.Common.CommandTrees;
using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
using System.Data.Entity.Core.Common.Utils;
using System.Data.Entity.Core.Mapping.ViewGeneration.Structures;
using System.Text;

namespace System.Data.Entity.Core.Mapping.ViewGeneration.CqlGeneration;

internal sealed class QualifiedSlot : ProjectedSlot
{
	private readonly CqlBlock m_block;

	private readonly ProjectedSlot m_slot;

	internal QualifiedSlot(CqlBlock block, ProjectedSlot slot)
	{
		m_block = block;
		m_slot = slot;
	}

	internal override ProjectedSlot DeepQualify(CqlBlock block)
	{
		return new QualifiedSlot(block, m_slot);
	}

	internal override string GetCqlFieldAlias(MemberPath outputMember)
	{
		return GetOriginalSlot().GetCqlFieldAlias(outputMember);
	}

	internal ProjectedSlot GetOriginalSlot()
	{
		ProjectedSlot slot;
		for (slot = m_slot; slot is QualifiedSlot qualifiedSlot; slot = qualifiedSlot.m_slot)
		{
		}
		return slot;
	}

	internal string GetQualifiedCqlName(MemberPath outputMember)
	{
		return CqlWriter.GetQualifiedName(m_block.CqlAlias, GetCqlFieldAlias(outputMember));
	}

	internal override StringBuilder AsEsql(StringBuilder builder, MemberPath outputMember, string blockAlias, int indentLevel)
	{
		builder.Append(GetQualifiedCqlName(outputMember));
		return builder;
	}

	internal override DbExpression AsCqt(DbExpression row, MemberPath outputMember)
	{
		return m_block.GetInput(row).Property(GetCqlFieldAlias(outputMember));
	}

	internal override void ToCompactString(StringBuilder builder)
	{
		StringUtil.FormatStringBuilder(builder, "{0} ", m_block.CqlAlias);
		m_slot.ToCompactString(builder);
	}
}
