using System.Collections.Generic;
using System.Data.Entity.Core.Common.CommandTrees;
using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
using System.Data.Entity.Core.Common.Utils;
using System.Data.Entity.Core.Mapping.ViewGeneration.Structures;
using System.Text;

namespace System.Data.Entity.Core.Mapping.ViewGeneration.CqlGeneration;

internal sealed class CaseCqlBlock : CqlBlock
{
	private readonly SlotInfo m_caseSlotInfo;

	internal CaseCqlBlock(SlotInfo[] slots, int caseSlot, CqlBlock child, BoolExpression whereClause, CqlIdentifiers identifiers, int blockAliasNum)
		: base(slots, new List<CqlBlock>(new CqlBlock[1] { child }), whereClause, identifiers, blockAliasNum)
	{
		m_caseSlotInfo = slots[caseSlot];
	}

	internal override StringBuilder AsEsql(StringBuilder builder, bool isTopLevel, int indentLevel)
	{
		StringUtil.IndentNewLine(builder, indentLevel);
		builder.Append("SELECT ");
		if (isTopLevel)
		{
			builder.Append("VALUE ");
		}
		builder.Append("-- Constructing ").Append(m_caseSlotInfo.OutputMember.LeafName);
		CqlBlock cqlBlock = base.Children[0];
		GenerateProjectionEsql(builder, cqlBlock.CqlAlias, addNewLineAfterEachSlot: true, indentLevel, isTopLevel);
		builder.Append("FROM (");
		cqlBlock.AsEsql(builder, isTopLevel: false, indentLevel + 1);
		StringUtil.IndentNewLine(builder, indentLevel);
		builder.Append(") AS ").Append(cqlBlock.CqlAlias);
		if (!BoolExpression.EqualityComparer.Equals(base.WhereClause, BoolExpression.True))
		{
			StringUtil.IndentNewLine(builder, indentLevel);
			builder.Append("WHERE ");
			base.WhereClause.AsEsql(builder, cqlBlock.CqlAlias);
		}
		return builder;
	}

	internal override DbExpression AsCqt(bool isTopLevel)
	{
		DbExpression source = base.Children[0].AsCqt(isTopLevel: false);
		if (!BoolExpression.EqualityComparer.Equals(base.WhereClause, BoolExpression.True))
		{
			source = source.Where((DbExpression row) => base.WhereClause.AsCqt(row));
		}
		return source.Select((DbExpression row) => GenerateProjectionCqt(row, isTopLevel));
	}
}
