using System.Collections.Generic;
using System.Data.Entity.Core.Common.CommandTrees;
using System.Data.Entity.Core.Mapping.ViewGeneration.CqlGeneration;
using System.Text;

namespace System.Data.Entity.Core.Mapping.ViewGeneration.Structures;

internal sealed class CaseStatementProjectedSlot : ProjectedSlot
{
	private readonly CaseStatement m_caseStatement;

	private readonly IEnumerable<WithRelationship> m_withRelationships;

	internal CaseStatementProjectedSlot(CaseStatement statement, IEnumerable<WithRelationship> withRelationships)
	{
		m_caseStatement = statement;
		m_withRelationships = withRelationships;
	}

	internal override ProjectedSlot DeepQualify(CqlBlock block)
	{
		return new CaseStatementProjectedSlot(m_caseStatement.DeepQualify(block), null);
	}

	internal override StringBuilder AsEsql(StringBuilder builder, MemberPath outputMember, string blockAlias, int indentLevel)
	{
		m_caseStatement.AsEsql(builder, m_withRelationships, blockAlias, indentLevel);
		return builder;
	}

	internal override DbExpression AsCqt(DbExpression row, MemberPath outputMember)
	{
		return m_caseStatement.AsCqt(row, m_withRelationships);
	}

	internal override void ToCompactString(StringBuilder builder)
	{
		m_caseStatement.ToCompactString(builder);
	}
}
