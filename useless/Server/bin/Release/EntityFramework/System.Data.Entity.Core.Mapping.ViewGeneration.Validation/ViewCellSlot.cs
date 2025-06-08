using System.Collections.Generic;
using System.Data.Entity.Core.Common.CommandTrees;
using System.Data.Entity.Core.Common.Utils;
using System.Data.Entity.Core.Mapping.ViewGeneration.Structures;
using System.Text;

namespace System.Data.Entity.Core.Mapping.ViewGeneration.Validation;

internal class ViewCellSlot : ProjectedSlot
{
	private readonly int m_slotNum;

	private readonly MemberProjectedSlot m_cSlot;

	private readonly MemberProjectedSlot m_sSlot;

	internal MemberProjectedSlot CSlot => m_cSlot;

	internal MemberProjectedSlot SSlot => m_sSlot;

	internal ViewCellSlot(int slotNum, MemberProjectedSlot cSlot, MemberProjectedSlot sSlot)
	{
		m_slotNum = slotNum;
		m_cSlot = cSlot;
		m_sSlot = sSlot;
	}

	protected override bool IsEqualTo(ProjectedSlot right)
	{
		if (!(right is ViewCellSlot viewCellSlot))
		{
			return false;
		}
		if (m_slotNum == viewCellSlot.m_slotNum && ProjectedSlot.EqualityComparer.Equals(m_cSlot, viewCellSlot.m_cSlot))
		{
			return ProjectedSlot.EqualityComparer.Equals(m_sSlot, viewCellSlot.m_sSlot);
		}
		return false;
	}

	protected override int GetHash()
	{
		return ProjectedSlot.EqualityComparer.GetHashCode(m_cSlot) ^ ProjectedSlot.EqualityComparer.GetHashCode(m_sSlot) ^ m_slotNum;
	}

	internal static string SlotsToUserString(IEnumerable<ViewCellSlot> slots, bool isFromCside)
	{
		StringBuilder stringBuilder = new StringBuilder();
		bool flag = true;
		foreach (ViewCellSlot slot in slots)
		{
			if (!flag)
			{
				stringBuilder.Append(", ");
			}
			stringBuilder.Append(SlotToUserString(slot, isFromCside));
			flag = false;
		}
		return stringBuilder.ToString();
	}

	internal static string SlotToUserString(ViewCellSlot slot, bool isFromCside)
	{
		MemberProjectedSlot memberProjectedSlot = (isFromCside ? slot.CSlot : slot.SSlot);
		return StringUtil.FormatInvariant("{0}", memberProjectedSlot);
	}

	internal override string GetCqlFieldAlias(MemberPath outputMember)
	{
		return null;
	}

	internal override StringBuilder AsEsql(StringBuilder builder, MemberPath outputMember, string blockAlias, int indentLevel)
	{
		return null;
	}

	internal override DbExpression AsCqt(DbExpression row, MemberPath outputMember)
	{
		return null;
	}

	internal override void ToCompactString(StringBuilder builder)
	{
		builder.Append('<');
		StringUtil.FormatStringBuilder(builder, "{0}", m_slotNum);
		builder.Append(':');
		m_cSlot.ToCompactString(builder);
		builder.Append('-');
		m_sSlot.ToCompactString(builder);
		builder.Append('>');
	}
}
