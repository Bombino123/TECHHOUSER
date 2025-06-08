using System.Collections.Generic;
using System.Data.Entity.Core.Mapping.ViewGeneration.Structures;

namespace System.Data.Entity.Core.Mapping.ViewGeneration.Validation;

internal class BasicKeyConstraint : KeyConstraint<BasicCellRelation, MemberProjectedSlot>
{
	internal BasicKeyConstraint(BasicCellRelation relation, IEnumerable<MemberProjectedSlot> keySlots)
		: base(relation, keySlots, (IEqualityComparer<MemberProjectedSlot>)ProjectedSlot.EqualityComparer)
	{
	}

	internal ViewKeyConstraint Propagate()
	{
		ViewCellRelation viewCellRelation = base.CellRelation.ViewCellRelation;
		List<ViewCellSlot> list = new List<ViewCellSlot>();
		foreach (MemberProjectedSlot keySlot in base.KeySlots)
		{
			ViewCellSlot viewCellSlot = viewCellRelation.LookupViewSlot(keySlot);
			if (viewCellSlot == null)
			{
				return null;
			}
			list.Add(viewCellSlot);
		}
		return new ViewKeyConstraint(viewCellRelation, list);
	}
}
