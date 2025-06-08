using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity.Core.Common.CommandTrees;
using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
using System.Data.Entity.Core.Common.Utils;
using System.Data.Entity.Core.Mapping.ViewGeneration.Structures;
using System.Linq;
using System.Text;

namespace System.Data.Entity.Core.Mapping.ViewGeneration.CqlGeneration;

internal abstract class CqlBlock : InternalBase
{
	private sealed class JoinTreeContext
	{
		private readonly IList<string> m_parentQualifiers;

		private readonly int m_indexInParentQualifiers;

		private readonly string m_leafQualifier;

		internal JoinTreeContext(IList<string> parentQualifiers, string leafQualifier)
		{
			m_parentQualifiers = parentQualifiers;
			m_indexInParentQualifiers = parentQualifiers.Count;
			m_leafQualifier = leafQualifier;
		}

		internal DbExpression FindInput(DbExpression row)
		{
			DbExpression instance = row;
			for (int num = m_parentQualifiers.Count - 1; num >= m_indexInParentQualifiers; num--)
			{
				instance = instance.Property(m_parentQualifiers[num]);
			}
			return instance.Property(m_leafQualifier);
		}
	}

	private ReadOnlyCollection<SlotInfo> m_slots;

	private readonly ReadOnlyCollection<CqlBlock> m_children;

	private readonly BoolExpression m_whereClause;

	private readonly string m_blockAlias;

	private JoinTreeContext m_joinTreeContext;

	internal ReadOnlyCollection<SlotInfo> Slots
	{
		get
		{
			return m_slots;
		}
		set
		{
			m_slots = value;
		}
	}

	protected ReadOnlyCollection<CqlBlock> Children => m_children;

	protected BoolExpression WhereClause => m_whereClause;

	internal string CqlAlias => m_blockAlias;

	protected CqlBlock(SlotInfo[] slotInfos, List<CqlBlock> children, BoolExpression whereClause, CqlIdentifiers identifiers, int blockAliasNum)
	{
		m_slots = new ReadOnlyCollection<SlotInfo>(slotInfos);
		m_children = new ReadOnlyCollection<CqlBlock>(children);
		m_whereClause = whereClause;
		m_blockAlias = identifiers.GetBlockAlias(blockAliasNum);
	}

	internal abstract StringBuilder AsEsql(StringBuilder builder, bool isTopLevel, int indentLevel);

	internal abstract DbExpression AsCqt(bool isTopLevel);

	internal QualifiedSlot QualifySlotWithBlockAlias(int slotNum)
	{
		SlotInfo slotInfo = m_slots[slotNum];
		return new QualifiedSlot(this, slotInfo.SlotValue);
	}

	internal ProjectedSlot SlotValue(int slotNum)
	{
		return m_slots[slotNum].SlotValue;
	}

	internal MemberPath MemberPath(int slotNum)
	{
		return m_slots[slotNum].OutputMember;
	}

	internal bool IsProjected(int slotNum)
	{
		return m_slots[slotNum].IsProjected;
	}

	protected void GenerateProjectionEsql(StringBuilder builder, string blockAlias, bool addNewLineAfterEachSlot, int indentLevel, bool isTopLevel)
	{
		bool flag = true;
		foreach (SlotInfo slot in Slots)
		{
			if (slot.IsRequiredByParent)
			{
				if (!flag)
				{
					builder.Append(", ");
				}
				if (addNewLineAfterEachSlot)
				{
					StringUtil.IndentNewLine(builder, indentLevel + 1);
				}
				slot.AsEsql(builder, blockAlias, indentLevel);
				if (!isTopLevel && (!(slot.SlotValue is QualifiedSlot) || slot.IsEnforcedNotNull))
				{
					builder.Append(" AS ").Append(slot.CqlFieldAlias);
				}
				flag = false;
			}
		}
		if (addNewLineAfterEachSlot)
		{
			StringUtil.IndentNewLine(builder, indentLevel);
		}
	}

	protected DbExpression GenerateProjectionCqt(DbExpression row, bool isTopLevel)
	{
		if (isTopLevel)
		{
			return Slots.Where((SlotInfo slot) => slot.IsRequiredByParent).Single().AsCqt(row);
		}
		return DbExpressionBuilder.NewRow(from slot in Slots
			where slot.IsRequiredByParent
			select new KeyValuePair<string, DbExpression>(slot.CqlFieldAlias, slot.AsCqt(row)));
	}

	internal void SetJoinTreeContext(IList<string> parentQualifiers, string leafQualifier)
	{
		m_joinTreeContext = new JoinTreeContext(parentQualifiers, leafQualifier);
	}

	internal DbExpression GetInput(DbExpression row)
	{
		if (m_joinTreeContext == null)
		{
			return row;
		}
		return m_joinTreeContext.FindInput(row);
	}

	internal override void ToCompactString(StringBuilder builder)
	{
		for (int i = 0; i < m_slots.Count; i++)
		{
			StringUtil.FormatStringBuilder(builder, "{0}: ", i);
			m_slots[i].ToCompactString(builder);
			builder.Append(' ');
		}
		m_whereClause.ToCompactString(builder);
	}
}
