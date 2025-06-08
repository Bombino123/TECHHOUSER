using System.Data.Entity.Core.Common.CommandTrees;
using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
using System.Data.Entity.Core.Common.Utils;
using System.Data.Entity.Core.Mapping.ViewGeneration.Structures;
using System.Text;

namespace System.Data.Entity.Core.Mapping.ViewGeneration.CqlGeneration;

internal sealed class SlotInfo : InternalBase
{
	private bool m_isRequiredByParent;

	private readonly bool m_isProjected;

	private readonly ProjectedSlot m_slotValue;

	private readonly MemberPath m_outputMember;

	private readonly bool m_enforceNotNull;

	internal bool IsRequiredByParent => m_isRequiredByParent;

	internal bool IsProjected => m_isProjected;

	internal MemberPath OutputMember => m_outputMember;

	internal ProjectedSlot SlotValue => m_slotValue;

	internal string CqlFieldAlias
	{
		get
		{
			if (m_slotValue == null)
			{
				return null;
			}
			return m_slotValue.GetCqlFieldAlias(m_outputMember);
		}
	}

	internal bool IsEnforcedNotNull => m_enforceNotNull;

	internal SlotInfo(bool isRequiredByParent, bool isProjected, ProjectedSlot slotValue, MemberPath outputMember)
		: this(isRequiredByParent, isProjected, slotValue, outputMember, enforceNotNull: false)
	{
	}

	internal SlotInfo(bool isRequiredByParent, bool isProjected, ProjectedSlot slotValue, MemberPath outputMember, bool enforceNotNull)
	{
		m_isRequiredByParent = isRequiredByParent;
		m_isProjected = isProjected;
		m_slotValue = slotValue;
		m_outputMember = outputMember;
		m_enforceNotNull = enforceNotNull;
	}

	internal void ResetIsRequiredByParent()
	{
		m_isRequiredByParent = false;
	}

	internal StringBuilder AsEsql(StringBuilder builder, string blockAlias, int indentLevel)
	{
		if (m_enforceNotNull)
		{
			builder.Append('(');
			m_slotValue.AsEsql(builder, m_outputMember, blockAlias, indentLevel);
			builder.Append(" AND ");
			m_slotValue.AsEsql(builder, m_outputMember, blockAlias, indentLevel);
			builder.Append(" IS NOT NULL)");
		}
		else
		{
			m_slotValue.AsEsql(builder, m_outputMember, blockAlias, indentLevel);
		}
		return builder;
	}

	internal DbExpression AsCqt(DbExpression row)
	{
		DbExpression dbExpression = m_slotValue.AsCqt(row, m_outputMember);
		if (m_enforceNotNull)
		{
			dbExpression = dbExpression.And(dbExpression.IsNull().Not());
		}
		return dbExpression;
	}

	internal override void ToCompactString(StringBuilder builder)
	{
		if (m_slotValue != null)
		{
			builder.Append(CqlFieldAlias);
		}
	}
}
