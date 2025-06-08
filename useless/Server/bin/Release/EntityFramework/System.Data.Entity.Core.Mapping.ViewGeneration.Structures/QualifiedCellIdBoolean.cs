using System.Data.Entity.Core.Common.CommandTrees;
using System.Data.Entity.Core.Mapping.ViewGeneration.CqlGeneration;
using System.Text;

namespace System.Data.Entity.Core.Mapping.ViewGeneration.Structures;

internal sealed class QualifiedCellIdBoolean : CellIdBoolean
{
	private readonly CqlBlock m_block;

	internal QualifiedCellIdBoolean(CqlBlock block, CqlIdentifiers identifiers, int originalCellNum)
		: base(identifiers, originalCellNum)
	{
		m_block = block;
	}

	internal override StringBuilder AsEsql(StringBuilder builder, string blockAlias, bool skipIsNotNull)
	{
		return base.AsEsql(builder, m_block.CqlAlias, skipIsNotNull);
	}

	internal override DbExpression AsCqt(DbExpression row, bool skipIsNotNull)
	{
		return base.AsCqt(m_block.GetInput(row), skipIsNotNull);
	}
}
