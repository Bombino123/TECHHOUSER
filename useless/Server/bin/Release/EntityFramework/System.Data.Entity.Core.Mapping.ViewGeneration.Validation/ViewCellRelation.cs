using System.Collections.Generic;
using System.Data.Entity.Core.Mapping.ViewGeneration.Structures;
using System.Text;

namespace System.Data.Entity.Core.Mapping.ViewGeneration.Validation;

internal class ViewCellRelation : CellRelation
{
	private readonly Cell m_cell;

	private readonly List<ViewCellSlot> m_slots;

	internal Cell Cell => m_cell;

	internal ViewCellRelation(Cell cell, List<ViewCellSlot> slots, int cellNumber)
		: base(cellNumber)
	{
		m_cell = cell;
		m_slots = slots;
		m_cell.CQuery.CreateBasicCellRelation(this);
		m_cell.SQuery.CreateBasicCellRelation(this);
	}

	internal ViewCellSlot LookupViewSlot(MemberProjectedSlot slot)
	{
		foreach (ViewCellSlot slot2 in m_slots)
		{
			if (ProjectedSlot.EqualityComparer.Equals(slot, slot2.CSlot) || ProjectedSlot.EqualityComparer.Equals(slot, slot2.SSlot))
			{
				return slot2;
			}
		}
		return null;
	}

	protected override int GetHash()
	{
		return m_cell.GetHashCode();
	}

	internal override void ToCompactString(StringBuilder builder)
	{
		builder.Append("ViewRel[");
		m_cell.ToCompactString(builder);
		builder.Append(']');
	}
}
