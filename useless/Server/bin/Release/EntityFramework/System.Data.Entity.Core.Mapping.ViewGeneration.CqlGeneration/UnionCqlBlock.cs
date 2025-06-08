using System.Collections.Generic;
using System.Data.Entity.Core.Common.CommandTrees;
using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
using System.Data.Entity.Core.Common.Utils;
using System.Data.Entity.Core.Mapping.ViewGeneration.Structures;
using System.Text;

namespace System.Data.Entity.Core.Mapping.ViewGeneration.CqlGeneration;

internal sealed class UnionCqlBlock : CqlBlock
{
	internal UnionCqlBlock(SlotInfo[] slotInfos, List<CqlBlock> children, CqlIdentifiers identifiers, int blockAliasNum)
		: base(slotInfos, children, BoolExpression.True, identifiers, blockAliasNum)
	{
	}

	internal override StringBuilder AsEsql(StringBuilder builder, bool isTopLevel, int indentLevel)
	{
		bool flag = true;
		foreach (CqlBlock child in base.Children)
		{
			if (!flag)
			{
				StringUtil.IndentNewLine(builder, indentLevel + 1);
				builder.Append(OpCellTreeNode.OpToEsql(CellTreeOpType.Union));
			}
			flag = false;
			builder.Append(" (");
			child.AsEsql(builder, isTopLevel, indentLevel + 1);
			builder.Append(')');
		}
		return builder;
	}

	internal override DbExpression AsCqt(bool isTopLevel)
	{
		DbExpression dbExpression = base.Children[0].AsCqt(isTopLevel);
		for (int i = 1; i < base.Children.Count; i++)
		{
			dbExpression = dbExpression.UnionAll(base.Children[i].AsCqt(isTopLevel));
		}
		return dbExpression;
	}
}
