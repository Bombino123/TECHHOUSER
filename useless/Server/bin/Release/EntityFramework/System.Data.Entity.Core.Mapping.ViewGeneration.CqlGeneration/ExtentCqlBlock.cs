using System.Collections.Generic;
using System.Data.Entity.Core.Common.CommandTrees;
using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
using System.Data.Entity.Core.Common.Utils;
using System.Data.Entity.Core.Mapping.ViewGeneration.Structures;
using System.Data.Entity.Core.Metadata.Edm;
using System.Text;

namespace System.Data.Entity.Core.Mapping.ViewGeneration.CqlGeneration;

internal sealed class ExtentCqlBlock : CqlBlock
{
	private readonly EntitySetBase m_extent;

	private readonly string m_nodeTableAlias;

	private readonly CellQuery.SelectDistinct m_selectDistinct;

	private static readonly List<CqlBlock> _emptyChildren = new List<CqlBlock>();

	internal ExtentCqlBlock(EntitySetBase extent, CellQuery.SelectDistinct selectDistinct, SlotInfo[] slots, BoolExpression whereClause, CqlIdentifiers identifiers, int blockAliasNum)
		: base(slots, _emptyChildren, whereClause, identifiers, blockAliasNum)
	{
		m_extent = extent;
		m_nodeTableAlias = identifiers.GetBlockAlias();
		m_selectDistinct = selectDistinct;
	}

	internal override StringBuilder AsEsql(StringBuilder builder, bool isTopLevel, int indentLevel)
	{
		StringUtil.IndentNewLine(builder, indentLevel);
		builder.Append("SELECT ");
		if (m_selectDistinct == CellQuery.SelectDistinct.Yes)
		{
			builder.Append("DISTINCT ");
		}
		GenerateProjectionEsql(builder, m_nodeTableAlias, addNewLineAfterEachSlot: true, indentLevel, isTopLevel);
		builder.Append("FROM ");
		CqlWriter.AppendEscapedQualifiedName(builder, m_extent.EntityContainer.Name, m_extent.Name);
		builder.Append(" AS ").Append(m_nodeTableAlias);
		if (!BoolExpression.EqualityComparer.Equals(base.WhereClause, BoolExpression.True))
		{
			StringUtil.IndentNewLine(builder, indentLevel);
			builder.Append("WHERE ");
			base.WhereClause.AsEsql(builder, m_nodeTableAlias);
		}
		return builder;
	}

	internal override DbExpression AsCqt(bool isTopLevel)
	{
		DbExpression source = m_extent.Scan();
		if (!BoolExpression.EqualityComparer.Equals(base.WhereClause, BoolExpression.True))
		{
			source = source.Where((DbExpression row) => base.WhereClause.AsCqt(row));
		}
		source = source.Select((DbExpression row) => GenerateProjectionCqt(row, isTopLevel));
		if (m_selectDistinct == CellQuery.SelectDistinct.Yes)
		{
			source = source.Distinct();
		}
		return source;
	}
}
