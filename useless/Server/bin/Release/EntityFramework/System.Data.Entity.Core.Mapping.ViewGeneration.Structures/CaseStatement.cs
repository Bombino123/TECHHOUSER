using System.Collections.Generic;
using System.Data.Entity.Core.Common.CommandTrees;
using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
using System.Data.Entity.Core.Common.Utils;
using System.Data.Entity.Core.Mapping.ViewGeneration.CqlGeneration;
using System.Data.Entity.Core.Metadata.Edm;
using System.Linq;
using System.Text;

namespace System.Data.Entity.Core.Mapping.ViewGeneration.Structures;

internal sealed class CaseStatement : InternalBase
{
	internal sealed class WhenThen : InternalBase
	{
		private readonly BoolExpression m_condition;

		private readonly ProjectedSlot m_value;

		internal BoolExpression Condition => m_condition;

		internal ProjectedSlot Value => m_value;

		internal WhenThen(BoolExpression condition, ProjectedSlot value)
		{
			m_condition = condition;
			m_value = value;
		}

		internal WhenThen ReplaceWithQualifiedSlot(CqlBlock block)
		{
			ProjectedSlot value = m_value.DeepQualify(block);
			return new WhenThen(m_condition, value);
		}

		internal override void ToCompactString(StringBuilder builder)
		{
			builder.Append("WHEN ");
			m_condition.ToCompactString(builder);
			builder.Append("THEN ");
			m_value.ToCompactString(builder);
		}
	}

	private readonly MemberPath m_memberPath;

	private List<WhenThen> m_clauses;

	private ProjectedSlot m_elseValue;

	private bool m_simplified;

	internal MemberPath MemberPath => m_memberPath;

	internal List<WhenThen> Clauses => m_clauses;

	internal ProjectedSlot ElseValue => m_elseValue;

	internal bool DependsOnMemberValue
	{
		get
		{
			if (m_elseValue is MemberProjectedSlot)
			{
				return true;
			}
			foreach (WhenThen clause in m_clauses)
			{
				if (clause.Value is MemberProjectedSlot)
				{
					return true;
				}
			}
			return false;
		}
	}

	internal IEnumerable<EdmType> InstantiatedTypes
	{
		get
		{
			foreach (WhenThen clause in m_clauses)
			{
				if (TryGetInstantiatedType(clause.Value, out var type))
				{
					yield return type;
				}
			}
			if (TryGetInstantiatedType(m_elseValue, out var type2))
			{
				yield return type2;
			}
		}
	}

	internal CaseStatement(MemberPath memberPath)
	{
		m_memberPath = memberPath;
		m_clauses = new List<WhenThen>();
	}

	internal CaseStatement DeepQualify(CqlBlock block)
	{
		CaseStatement caseStatement = new CaseStatement(m_memberPath);
		foreach (WhenThen clause in m_clauses)
		{
			WhenThen item = clause.ReplaceWithQualifiedSlot(block);
			caseStatement.m_clauses.Add(item);
		}
		if (m_elseValue != null)
		{
			caseStatement.m_elseValue = m_elseValue.DeepQualify(block);
		}
		caseStatement.m_simplified = m_simplified;
		return caseStatement;
	}

	internal void AddWhenThen(BoolExpression condition, ProjectedSlot value)
	{
		condition.ExpensiveSimplify();
		m_clauses.Add(new WhenThen(condition, value));
	}

	private static bool TryGetInstantiatedType(ProjectedSlot slot, out EdmType type)
	{
		type = null;
		if (slot is ConstantProjectedSlot { CellConstant: TypeConstant cellConstant })
		{
			type = cellConstant.EdmType;
			return true;
		}
		return false;
	}

	internal void Simplify()
	{
		if (m_simplified)
		{
			return;
		}
		List<WhenThen> list = new List<WhenThen>();
		bool flag = false;
		foreach (WhenThen clause in m_clauses)
		{
			if (clause.Value is ConstantProjectedSlot constantProjectedSlot && (constantProjectedSlot.CellConstant.IsNull() || constantProjectedSlot.CellConstant.IsUndefined()))
			{
				flag = true;
				continue;
			}
			list.Add(clause);
			if (clause.Condition.IsTrue)
			{
				break;
			}
		}
		if (flag && list.Count == 0)
		{
			m_elseValue = new ConstantProjectedSlot(Constant.Null);
		}
		if (list.Count > 0 && !flag)
		{
			int index = list.Count - 1;
			m_elseValue = list[index].Value;
			list.RemoveAt(index);
		}
		m_clauses = list;
		m_simplified = true;
	}

	internal StringBuilder AsEsql(StringBuilder builder, IEnumerable<WithRelationship> withRelationships, string blockAlias, int indentLevel)
	{
		if (Clauses.Count == 0)
		{
			CaseSlotValueAsEsql(builder, ElseValue, MemberPath, blockAlias, withRelationships, indentLevel);
			return builder;
		}
		builder.Append("CASE");
		foreach (WhenThen clause in Clauses)
		{
			StringUtil.IndentNewLine(builder, indentLevel + 2);
			builder.Append("WHEN ");
			clause.Condition.AsEsql(builder, blockAlias);
			builder.Append(" THEN ");
			CaseSlotValueAsEsql(builder, clause.Value, MemberPath, blockAlias, withRelationships, indentLevel + 2);
		}
		if (ElseValue != null)
		{
			StringUtil.IndentNewLine(builder, indentLevel + 2);
			builder.Append("ELSE ");
			CaseSlotValueAsEsql(builder, ElseValue, MemberPath, blockAlias, withRelationships, indentLevel + 2);
		}
		StringUtil.IndentNewLine(builder, indentLevel + 1);
		builder.Append("END");
		return builder;
	}

	internal DbExpression AsCqt(DbExpression row, IEnumerable<WithRelationship> withRelationships)
	{
		List<DbExpression> list = new List<DbExpression>();
		List<DbExpression> list2 = new List<DbExpression>();
		foreach (WhenThen clause in Clauses)
		{
			list.Add(clause.Condition.AsCqt(row));
			list2.Add(CaseSlotValueAsCqt(row, clause.Value, MemberPath, withRelationships));
		}
		DbExpression dbExpression = ((ElseValue != null) ? CaseSlotValueAsCqt(row, ElseValue, MemberPath, withRelationships) : Constant.Null.AsCqt(row, MemberPath));
		if (Clauses.Count > 0)
		{
			return DbExpressionBuilder.Case(list, list2, dbExpression);
		}
		return dbExpression;
	}

	private static StringBuilder CaseSlotValueAsEsql(StringBuilder builder, ProjectedSlot slot, MemberPath outputMember, string blockAlias, IEnumerable<WithRelationship> withRelationships, int indentLevel)
	{
		slot.AsEsql(builder, outputMember, blockAlias, 1);
		WithRelationshipsClauseAsEsql(builder, withRelationships, blockAlias, indentLevel, slot);
		return builder;
	}

	private static void WithRelationshipsClauseAsEsql(StringBuilder builder, IEnumerable<WithRelationship> withRelationships, string blockAlias, int indentLevel, ProjectedSlot slot)
	{
		bool first = true;
		WithRelationshipsClauseAsCql(delegate(WithRelationship withRelationship)
		{
			if (first)
			{
				builder.Append(" WITH ");
				first = false;
			}
			withRelationship.AsEsql(builder, blockAlias, indentLevel);
		}, withRelationships, slot);
	}

	private static DbExpression CaseSlotValueAsCqt(DbExpression row, ProjectedSlot slot, MemberPath outputMember, IEnumerable<WithRelationship> withRelationships)
	{
		DbExpression slotValueExpr = slot.AsCqt(row, outputMember);
		return WithRelationshipsClauseAsCqt(row, slotValueExpr, withRelationships, slot);
	}

	private static DbExpression WithRelationshipsClauseAsCqt(DbExpression row, DbExpression slotValueExpr, IEnumerable<WithRelationship> withRelationships, ProjectedSlot slot)
	{
		List<DbRelatedEntityRef> relatedEntityRefs = new List<DbRelatedEntityRef>();
		WithRelationshipsClauseAsCql(delegate(WithRelationship withRelationship)
		{
			relatedEntityRefs.Add(withRelationship.AsCqt(row));
		}, withRelationships, slot);
		if (relatedEntityRefs.Count > 0)
		{
			DbNewInstanceExpression dbNewInstanceExpression = slotValueExpr as DbNewInstanceExpression;
			return DbExpressionBuilder.CreateNewEntityWithRelationshipsExpression((EntityType)dbNewInstanceExpression.ResultType.EdmType, dbNewInstanceExpression.Arguments, relatedEntityRefs);
		}
		return slotValueExpr;
	}

	private static void WithRelationshipsClauseAsCql(Action<WithRelationship> emitWithRelationship, IEnumerable<WithRelationship> withRelationships, ProjectedSlot slot)
	{
		if (withRelationships == null || withRelationships.Count() <= 0)
		{
			return;
		}
		EdmType edmType = ((slot as ConstantProjectedSlot).CellConstant as TypeConstant).EdmType;
		foreach (WithRelationship withRelationship in withRelationships)
		{
			if (withRelationship.FromEndEntityType.IsAssignableFrom(edmType))
			{
				emitWithRelationship(withRelationship);
			}
		}
	}

	internal override void ToCompactString(StringBuilder builder)
	{
		builder.AppendLine("CASE");
		foreach (WhenThen clause in m_clauses)
		{
			builder.Append(" WHEN ");
			clause.Condition.ToCompactString(builder);
			builder.Append(" THEN ");
			clause.Value.ToCompactString(builder);
			builder.AppendLine();
		}
		if (m_elseValue != null)
		{
			builder.Append(" ELSE ");
			m_elseValue.ToCompactString(builder);
			builder.AppendLine();
		}
		builder.Append(" END AS ");
		m_memberPath.ToCompactString(builder);
	}
}
