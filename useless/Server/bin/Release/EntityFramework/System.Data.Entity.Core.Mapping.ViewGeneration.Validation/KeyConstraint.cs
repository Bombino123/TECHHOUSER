using System.Collections.Generic;
using System.Data.Entity.Core.Common.Utils;
using System.Text;

namespace System.Data.Entity.Core.Mapping.ViewGeneration.Validation;

internal class KeyConstraint<TCellRelation, TSlot> : InternalBase where TCellRelation : CellRelation
{
	private readonly TCellRelation m_relation;

	private readonly Set<TSlot> m_keySlots;

	protected TCellRelation CellRelation => m_relation;

	protected Set<TSlot> KeySlots => m_keySlots;

	internal KeyConstraint(TCellRelation relation, IEnumerable<TSlot> keySlots, IEqualityComparer<TSlot> comparer)
	{
		m_relation = relation;
		m_keySlots = new Set<TSlot>(keySlots, comparer).MakeReadOnly();
	}

	internal override void ToCompactString(StringBuilder builder)
	{
		StringUtil.FormatStringBuilder(builder, "Key (V{0}) - ", m_relation.CellNumber);
		StringUtil.ToSeparatedStringSorted(builder, KeySlots, ", ");
	}
}
