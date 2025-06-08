using System.Data.Entity.Core.Common.CommandTrees;
using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
using System.Data.Entity.Core.Common.Utils;
using System.Data.Entity.Core.Mapping.ViewGeneration.Structures;
using System.Text;

namespace System.Data.Entity.Core.Mapping.ViewGeneration.CqlGeneration;

internal sealed class BooleanProjectedSlot : ProjectedSlot
{
	private readonly BoolExpression m_expr;

	private readonly CellIdBoolean m_originalCell;

	internal BooleanProjectedSlot(BoolExpression expr, CqlIdentifiers identifiers, int originalCellNum)
	{
		m_expr = expr;
		m_originalCell = new CellIdBoolean(identifiers, originalCellNum);
	}

	internal override string GetCqlFieldAlias(MemberPath outputMember)
	{
		return m_originalCell.SlotName;
	}

	internal override StringBuilder AsEsql(StringBuilder builder, MemberPath outputMember, string blockAlias, int indentLevel)
	{
		if (m_expr.IsTrue || m_expr.IsFalse)
		{
			m_expr.AsEsql(builder, blockAlias);
		}
		else
		{
			builder.Append("CASE WHEN ");
			m_expr.AsEsql(builder, blockAlias);
			builder.Append(" THEN True ELSE False END");
		}
		return builder;
	}

	internal override DbExpression AsCqt(DbExpression row, MemberPath outputMember)
	{
		if (m_expr.IsTrue || m_expr.IsFalse)
		{
			return m_expr.AsCqt(row);
		}
		return DbExpressionBuilder.Case(new DbExpression[1] { m_expr.AsCqt(row) }, new DbExpression[1] { DbExpressionBuilder.True }, DbExpressionBuilder.False);
	}

	internal override void ToCompactString(StringBuilder builder)
	{
		StringUtil.FormatStringBuilder(builder, "<{0}, ", m_originalCell.SlotName);
		m_expr.ToCompactString(builder);
		builder.Append('>');
	}
}
