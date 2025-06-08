using System.Collections.Generic;
using System.Data.Entity.Core.Common.CommandTrees;
using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
using System.Data.Entity.Core.Common.Utils;
using System.Data.Entity.Core.Mapping.ViewGeneration.Structures;
using System.Text;

namespace System.Data.Entity.Core.Mapping.ViewGeneration.CqlGeneration;

internal sealed class JoinCqlBlock : CqlBlock
{
	internal sealed class OnClause : InternalBase
	{
		private sealed class SingleClause : InternalBase
		{
			private readonly QualifiedSlot m_leftSlot;

			private readonly MemberPath m_leftSlotOutputMember;

			private readonly QualifiedSlot m_rightSlot;

			private readonly MemberPath m_rightSlotOutputMember;

			internal SingleClause(QualifiedSlot leftSlot, MemberPath leftSlotOutputMember, QualifiedSlot rightSlot, MemberPath rightSlotOutputMember)
			{
				m_leftSlot = leftSlot;
				m_leftSlotOutputMember = leftSlotOutputMember;
				m_rightSlot = rightSlot;
				m_rightSlotOutputMember = rightSlotOutputMember;
			}

			internal StringBuilder AsEsql(StringBuilder builder)
			{
				builder.Append(m_leftSlot.GetQualifiedCqlName(m_leftSlotOutputMember)).Append(" = ").Append(m_rightSlot.GetQualifiedCqlName(m_rightSlotOutputMember));
				return builder;
			}

			internal DbExpression AsCqt(DbExpression leftRow, DbExpression rightRow)
			{
				return m_leftSlot.AsCqt(leftRow, m_leftSlotOutputMember).Equal(m_rightSlot.AsCqt(rightRow, m_rightSlotOutputMember));
			}

			internal override void ToCompactString(StringBuilder builder)
			{
				m_leftSlot.ToCompactString(builder);
				builder.Append(" = ");
				m_rightSlot.ToCompactString(builder);
			}
		}

		private readonly List<SingleClause> m_singleClauses;

		internal OnClause()
		{
			m_singleClauses = new List<SingleClause>();
		}

		internal void Add(QualifiedSlot leftSlot, MemberPath leftSlotOutputMember, QualifiedSlot rightSlot, MemberPath rightSlotOutputMember)
		{
			SingleClause item = new SingleClause(leftSlot, leftSlotOutputMember, rightSlot, rightSlotOutputMember);
			m_singleClauses.Add(item);
		}

		internal StringBuilder AsEsql(StringBuilder builder)
		{
			bool flag = true;
			foreach (SingleClause singleClause in m_singleClauses)
			{
				if (!flag)
				{
					builder.Append(" AND ");
				}
				singleClause.AsEsql(builder);
				flag = false;
			}
			return builder;
		}

		internal DbExpression AsCqt(DbExpression leftRow, DbExpression rightRow)
		{
			DbExpression dbExpression = m_singleClauses[0].AsCqt(leftRow, rightRow);
			for (int i = 1; i < m_singleClauses.Count; i++)
			{
				dbExpression = dbExpression.And(m_singleClauses[i].AsCqt(leftRow, rightRow));
			}
			return dbExpression;
		}

		internal override void ToCompactString(StringBuilder builder)
		{
			builder.Append("ON ");
			StringUtil.ToSeparatedString(builder, m_singleClauses, " AND ");
		}
	}

	private readonly CellTreeOpType m_opType;

	private readonly List<OnClause> m_onClauses;

	internal JoinCqlBlock(CellTreeOpType opType, SlotInfo[] slotInfos, List<CqlBlock> children, List<OnClause> onClauses, CqlIdentifiers identifiers, int blockAliasNum)
		: base(slotInfos, children, BoolExpression.True, identifiers, blockAliasNum)
	{
		m_opType = opType;
		m_onClauses = onClauses;
	}

	internal override StringBuilder AsEsql(StringBuilder builder, bool isTopLevel, int indentLevel)
	{
		StringUtil.IndentNewLine(builder, indentLevel);
		builder.Append("SELECT ");
		GenerateProjectionEsql(builder, null, addNewLineAfterEachSlot: false, indentLevel, isTopLevel);
		StringUtil.IndentNewLine(builder, indentLevel);
		builder.Append("FROM ");
		int num = 0;
		foreach (CqlBlock child in base.Children)
		{
			if (num > 0)
			{
				StringUtil.IndentNewLine(builder, indentLevel + 1);
				builder.Append(OpCellTreeNode.OpToEsql(m_opType));
			}
			builder.Append(" (");
			child.AsEsql(builder, isTopLevel: false, indentLevel + 1);
			builder.Append(") AS ").Append(child.CqlAlias);
			if (num > 0)
			{
				StringUtil.IndentNewLine(builder, indentLevel + 1);
				builder.Append("ON ");
				m_onClauses[num - 1].AsEsql(builder);
			}
			num++;
		}
		return builder;
	}

	internal override DbExpression AsCqt(bool isTopLevel)
	{
		CqlBlock cqlBlock = base.Children[0];
		DbExpression dbExpression = cqlBlock.AsCqt(isTopLevel: false);
		List<string> list = new List<string>();
		for (int i = 1; i < base.Children.Count; i++)
		{
			CqlBlock cqlBlock2 = base.Children[i];
			DbExpression right = cqlBlock2.AsCqt(isTopLevel: false);
			Func<DbExpression, DbExpression, DbExpression> joinCondition = m_onClauses[i - 1].AsCqt;
			DbJoinExpression dbJoinExpression;
			switch (m_opType)
			{
			case CellTreeOpType.FOJ:
				dbJoinExpression = dbExpression.FullOuterJoin(right, joinCondition);
				break;
			case CellTreeOpType.IJ:
				dbJoinExpression = dbExpression.InnerJoin(right, joinCondition);
				break;
			case CellTreeOpType.LOJ:
				dbJoinExpression = dbExpression.LeftOuterJoin(right, joinCondition);
				break;
			default:
				return null;
			}
			if (i == 1)
			{
				cqlBlock.SetJoinTreeContext(list, dbJoinExpression.Left.VariableName);
			}
			else
			{
				list.Add(dbJoinExpression.Left.VariableName);
			}
			cqlBlock2.SetJoinTreeContext(list, dbJoinExpression.Right.VariableName);
			dbExpression = dbJoinExpression;
		}
		return dbExpression.Select((DbExpression row) => GenerateProjectionCqt(row, isTopLevel: false));
	}
}
