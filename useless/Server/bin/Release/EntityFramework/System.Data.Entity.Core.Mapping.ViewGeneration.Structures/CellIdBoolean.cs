using System.Collections.Generic;
using System.Data.Entity.Core.Common.CommandTrees;
using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
using System.Data.Entity.Core.Mapping.ViewGeneration.CqlGeneration;
using System.Text;

namespace System.Data.Entity.Core.Mapping.ViewGeneration.Structures;

internal class CellIdBoolean : TrueFalseLiteral
{
	private readonly int m_index;

	private readonly string m_slotName;

	internal string SlotName => m_slotName;

	internal CellIdBoolean(CqlIdentifiers identifiers, int index)
	{
		m_index = index;
		m_slotName = identifiers.GetFromVariable(index);
	}

	internal override StringBuilder AsEsql(StringBuilder builder, string blockAlias, bool skipIsNotNull)
	{
		string qualifiedName = CqlWriter.GetQualifiedName(blockAlias, SlotName);
		builder.Append(qualifiedName);
		return builder;
	}

	internal override DbExpression AsCqt(DbExpression row, bool skipIsNotNull)
	{
		return row.Property(SlotName);
	}

	internal override StringBuilder AsUserString(StringBuilder builder, string blockAlias, bool skipIsNotNull)
	{
		return AsEsql(builder, blockAlias, skipIsNotNull);
	}

	internal override StringBuilder AsNegatedUserString(StringBuilder builder, string blockAlias, bool skipIsNotNull)
	{
		builder.Append("NOT(");
		builder = AsUserString(builder, blockAlias, skipIsNotNull);
		builder.Append(")");
		return builder;
	}

	internal override void GetRequiredSlots(MemberProjectionIndex projectedSlotMap, bool[] requiredSlots)
	{
		int numBoolSlots = requiredSlots.Length - projectedSlotMap.Count;
		int num = projectedSlotMap.BoolIndexToSlot(m_index, numBoolSlots);
		requiredSlots[num] = true;
	}

	protected override bool IsEqualTo(BoolLiteral right)
	{
		if (!(right is CellIdBoolean cellIdBoolean))
		{
			return false;
		}
		return m_index == cellIdBoolean.m_index;
	}

	public override int GetHashCode()
	{
		int index = m_index;
		return index.GetHashCode();
	}

	internal override BoolLiteral RemapBool(Dictionary<MemberPath, MemberPath> remap)
	{
		return this;
	}

	internal override void ToCompactString(StringBuilder builder)
	{
		builder.Append(SlotName);
	}
}
