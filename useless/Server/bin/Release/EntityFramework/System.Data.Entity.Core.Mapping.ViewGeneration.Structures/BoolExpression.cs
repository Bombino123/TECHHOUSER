using System.Collections.Generic;
using System.Data.Entity.Core.Common.CommandTrees;
using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
using System.Data.Entity.Core.Common.Utils;
using System.Data.Entity.Core.Common.Utils.Boolean;
using System.Data.Entity.Resources;
using System.Linq;
using System.Text;

namespace System.Data.Entity.Core.Mapping.ViewGeneration.Structures;

internal class BoolExpression : InternalBase
{
	private class CopyVisitor : BasicVisitor<DomainConstraint<BoolLiteral, Constant>>
	{
	}

	private class BoolComparer : IEqualityComparer<BoolExpression>
	{
		public bool Equals(BoolExpression left, BoolExpression right)
		{
			if (left == right)
			{
				return true;
			}
			if (left == null || right == null)
			{
				return false;
			}
			return left.m_tree.Equals(right.m_tree);
		}

		public int GetHashCode(BoolExpression expression)
		{
			return expression.m_tree.GetHashCode();
		}
	}

	private class FixRangeVisitor : BasicVisitor<DomainConstraint<BoolLiteral, Constant>>
	{
		private readonly MemberDomainMap m_memberDomainMap;

		private FixRangeVisitor(MemberDomainMap memberDomainMap)
		{
			m_memberDomainMap = memberDomainMap;
		}

		internal static BoolExpr<DomainConstraint<BoolLiteral, Constant>> FixRange(BoolExpr<DomainConstraint<BoolLiteral, Constant>> expression, MemberDomainMap memberDomainMap)
		{
			FixRangeVisitor visitor = new FixRangeVisitor(memberDomainMap);
			return expression.Accept(visitor);
		}

		internal override BoolExpr<DomainConstraint<BoolLiteral, Constant>> VisitTerm(TermExpr<DomainConstraint<BoolLiteral, Constant>> expression)
		{
			return GetBoolLiteral(expression).FixRange(expression.Identifier.Range, m_memberDomainMap);
		}
	}

	private class IsFinalVisitor : Visitor<DomainConstraint<BoolLiteral, Constant>, bool>
	{
		internal static bool IsFinal(BoolExpr<DomainConstraint<BoolLiteral, Constant>> expression)
		{
			IsFinalVisitor visitor = new IsFinalVisitor();
			return expression.Accept(visitor);
		}

		internal override bool VisitTrue(TrueExpr<DomainConstraint<BoolLiteral, Constant>> expression)
		{
			return true;
		}

		internal override bool VisitFalse(FalseExpr<DomainConstraint<BoolLiteral, Constant>> expression)
		{
			return true;
		}

		internal override bool VisitTerm(TermExpr<DomainConstraint<BoolLiteral, Constant>> expression)
		{
			if (GetBoolLiteral(expression) is MemberRestriction memberRestriction)
			{
				return memberRestriction.IsComplete;
			}
			return true;
		}

		internal override bool VisitNot(NotExpr<DomainConstraint<BoolLiteral, Constant>> expression)
		{
			return expression.Child.Accept(this);
		}

		internal override bool VisitAnd(AndExpr<DomainConstraint<BoolLiteral, Constant>> expression)
		{
			return VisitAndOr(expression);
		}

		internal override bool VisitOr(OrExpr<DomainConstraint<BoolLiteral, Constant>> expression)
		{
			return VisitAndOr(expression);
		}

		private bool VisitAndOr(TreeExpr<DomainConstraint<BoolLiteral, Constant>> expression)
		{
			bool flag = true;
			bool result = true;
			foreach (BoolExpr<DomainConstraint<BoolLiteral, Constant>> child in expression.Children)
			{
				if (!(child is FalseExpr<DomainConstraint<BoolLiteral, Constant>>) && !(child is TrueExpr<DomainConstraint<BoolLiteral, Constant>>))
				{
					bool flag2 = child.Accept(this);
					if (flag)
					{
						result = flag2;
					}
					flag = false;
				}
			}
			return result;
		}
	}

	private class RemapBoolVisitor : BasicVisitor<DomainConstraint<BoolLiteral, Constant>>
	{
		private readonly Dictionary<MemberPath, MemberPath> m_remap;

		private readonly MemberDomainMap m_memberDomainMap;

		private RemapBoolVisitor(MemberDomainMap memberDomainMap, Dictionary<MemberPath, MemberPath> remap)
		{
			m_remap = remap;
			m_memberDomainMap = memberDomainMap;
		}

		internal static BoolExpr<DomainConstraint<BoolLiteral, Constant>> RemapExtentTreeNodes(BoolExpr<DomainConstraint<BoolLiteral, Constant>> expression, MemberDomainMap memberDomainMap, Dictionary<MemberPath, MemberPath> remap)
		{
			RemapBoolVisitor visitor = new RemapBoolVisitor(memberDomainMap, remap);
			return expression.Accept(visitor);
		}

		internal override BoolExpr<DomainConstraint<BoolLiteral, Constant>> VisitTerm(TermExpr<DomainConstraint<BoolLiteral, Constant>> expression)
		{
			return GetBoolLiteral(expression).RemapBool(m_remap).GetDomainBoolExpression(m_memberDomainMap);
		}
	}

	private class RequiredSlotsVisitor : BasicVisitor<DomainConstraint<BoolLiteral, Constant>>
	{
		private readonly MemberProjectionIndex m_projectedSlotMap;

		private readonly bool[] m_requiredSlots;

		private RequiredSlotsVisitor(MemberProjectionIndex projectedSlotMap, bool[] requiredSlots)
		{
			m_projectedSlotMap = projectedSlotMap;
			m_requiredSlots = requiredSlots;
		}

		internal static void GetRequiredSlots(BoolExpr<DomainConstraint<BoolLiteral, Constant>> expression, MemberProjectionIndex projectedSlotMap, bool[] requiredSlots)
		{
			RequiredSlotsVisitor visitor = new RequiredSlotsVisitor(projectedSlotMap, requiredSlots);
			expression.Accept(visitor);
		}

		internal override BoolExpr<DomainConstraint<BoolLiteral, Constant>> VisitTerm(TermExpr<DomainConstraint<BoolLiteral, Constant>> expression)
		{
			GetBoolLiteral(expression).GetRequiredSlots(m_projectedSlotMap, m_requiredSlots);
			return expression;
		}
	}

	private sealed class AsEsqlVisitor : AsCqlVisitor<StringBuilder>
	{
		private readonly StringBuilder m_builder;

		private readonly string m_blockAlias;

		internal static StringBuilder AsEsql(BoolExpr<DomainConstraint<BoolLiteral, Constant>> expression, StringBuilder builder, string blockAlias)
		{
			AsEsqlVisitor visitor = new AsEsqlVisitor(builder, blockAlias);
			return expression.Accept(visitor);
		}

		private AsEsqlVisitor(StringBuilder builder, string blockAlias)
		{
			m_builder = builder;
			m_blockAlias = blockAlias;
		}

		internal override StringBuilder VisitTrue(TrueExpr<DomainConstraint<BoolLiteral, Constant>> expression)
		{
			m_builder.Append("True");
			return m_builder;
		}

		internal override StringBuilder VisitFalse(FalseExpr<DomainConstraint<BoolLiteral, Constant>> expression)
		{
			m_builder.Append("False");
			return m_builder;
		}

		protected override StringBuilder BooleanLiteralAsCql(BoolLiteral literal, bool skipIsNotNull)
		{
			return literal.AsEsql(m_builder, m_blockAlias, skipIsNotNull);
		}

		protected override StringBuilder NotExprAsCql(NotExpr<DomainConstraint<BoolLiteral, Constant>> expression)
		{
			m_builder.Append("NOT(");
			expression.Child.Accept(this);
			m_builder.Append(")");
			return m_builder;
		}

		internal override StringBuilder VisitAnd(AndExpr<DomainConstraint<BoolLiteral, Constant>> expression)
		{
			return VisitAndOr(expression, ExprType.And);
		}

		internal override StringBuilder VisitOr(OrExpr<DomainConstraint<BoolLiteral, Constant>> expression)
		{
			return VisitAndOr(expression, ExprType.Or);
		}

		private StringBuilder VisitAndOr(TreeExpr<DomainConstraint<BoolLiteral, Constant>> expression, ExprType kind)
		{
			m_builder.Append('(');
			bool flag = true;
			foreach (BoolExpr<DomainConstraint<BoolLiteral, Constant>> child in expression.Children)
			{
				if (!flag)
				{
					if (kind == ExprType.And)
					{
						m_builder.Append(" AND ");
					}
					else
					{
						m_builder.Append(" OR ");
					}
				}
				flag = false;
				child.Accept(this);
			}
			m_builder.Append(')');
			return m_builder;
		}
	}

	private sealed class AsCqtVisitor : AsCqlVisitor<DbExpression>
	{
		private readonly DbExpression m_row;

		internal static DbExpression AsCqt(BoolExpr<DomainConstraint<BoolLiteral, Constant>> expression, DbExpression row)
		{
			AsCqtVisitor visitor = new AsCqtVisitor(row);
			return expression.Accept(visitor);
		}

		private AsCqtVisitor(DbExpression row)
		{
			m_row = row;
		}

		internal override DbExpression VisitTrue(TrueExpr<DomainConstraint<BoolLiteral, Constant>> expression)
		{
			return DbExpressionBuilder.True;
		}

		internal override DbExpression VisitFalse(FalseExpr<DomainConstraint<BoolLiteral, Constant>> expression)
		{
			return DbExpressionBuilder.False;
		}

		protected override DbExpression BooleanLiteralAsCql(BoolLiteral literal, bool skipIsNotNull)
		{
			return literal.AsCqt(m_row, skipIsNotNull);
		}

		protected override DbExpression NotExprAsCql(NotExpr<DomainConstraint<BoolLiteral, Constant>> expression)
		{
			return expression.Child.Accept(this).Not();
		}

		internal override DbExpression VisitAnd(AndExpr<DomainConstraint<BoolLiteral, Constant>> expression)
		{
			return VisitAndOr(expression, DbExpressionBuilder.And);
		}

		internal override DbExpression VisitOr(OrExpr<DomainConstraint<BoolLiteral, Constant>> expression)
		{
			return VisitAndOr(expression, DbExpressionBuilder.Or);
		}

		private DbExpression VisitAndOr(TreeExpr<DomainConstraint<BoolLiteral, Constant>> expression, Func<DbExpression, DbExpression, DbExpression> op)
		{
			DbExpression dbExpression = null;
			foreach (BoolExpr<DomainConstraint<BoolLiteral, Constant>> child in expression.Children)
			{
				dbExpression = ((dbExpression != null) ? op(dbExpression, child.Accept(this)) : child.Accept(this));
			}
			return dbExpression;
		}
	}

	private abstract class AsCqlVisitor<T_Return> : Visitor<DomainConstraint<BoolLiteral, Constant>, T_Return>
	{
		private bool m_skipIsNotNull;

		protected AsCqlVisitor()
		{
			m_skipIsNotNull = true;
		}

		internal override T_Return VisitTerm(TermExpr<DomainConstraint<BoolLiteral, Constant>> expression)
		{
			BoolLiteral boolLiteral = GetBoolLiteral(expression);
			return BooleanLiteralAsCql(boolLiteral, m_skipIsNotNull);
		}

		protected abstract T_Return BooleanLiteralAsCql(BoolLiteral literal, bool skipIsNotNull);

		internal override T_Return VisitNot(NotExpr<DomainConstraint<BoolLiteral, Constant>> expression)
		{
			m_skipIsNotNull = false;
			return NotExprAsCql(expression);
		}

		protected abstract T_Return NotExprAsCql(NotExpr<DomainConstraint<BoolLiteral, Constant>> expression);
	}

	private class AsUserStringVisitor : Visitor<DomainConstraint<BoolLiteral, Constant>, StringBuilder>
	{
		private readonly StringBuilder m_builder;

		private readonly string m_blockAlias;

		private bool m_skipIsNotNull;

		private AsUserStringVisitor(StringBuilder builder, string blockAlias)
		{
			m_builder = builder;
			m_blockAlias = blockAlias;
			m_skipIsNotNull = true;
		}

		internal static StringBuilder AsUserString(BoolExpr<DomainConstraint<BoolLiteral, Constant>> expression, StringBuilder builder, string blockAlias)
		{
			AsUserStringVisitor visitor = new AsUserStringVisitor(builder, blockAlias);
			return expression.Accept(visitor);
		}

		internal override StringBuilder VisitTrue(TrueExpr<DomainConstraint<BoolLiteral, Constant>> expression)
		{
			m_builder.Append("True");
			return m_builder;
		}

		internal override StringBuilder VisitFalse(FalseExpr<DomainConstraint<BoolLiteral, Constant>> expression)
		{
			m_builder.Append("False");
			return m_builder;
		}

		internal override StringBuilder VisitTerm(TermExpr<DomainConstraint<BoolLiteral, Constant>> expression)
		{
			BoolLiteral boolLiteral = GetBoolLiteral(expression);
			if (boolLiteral is ScalarRestriction || boolLiteral is TypeRestriction)
			{
				return boolLiteral.AsUserString(m_builder, Strings.ViewGen_EntityInstanceToken, m_skipIsNotNull);
			}
			return boolLiteral.AsUserString(m_builder, m_blockAlias, m_skipIsNotNull);
		}

		internal override StringBuilder VisitNot(NotExpr<DomainConstraint<BoolLiteral, Constant>> expression)
		{
			m_skipIsNotNull = false;
			if (expression.Child is TermExpr<DomainConstraint<BoolLiteral, Constant>> term)
			{
				return GetBoolLiteral(term).AsNegatedUserString(m_builder, m_blockAlias, m_skipIsNotNull);
			}
			m_builder.Append("NOT(");
			expression.Child.Accept(this);
			m_builder.Append(")");
			return m_builder;
		}

		internal override StringBuilder VisitAnd(AndExpr<DomainConstraint<BoolLiteral, Constant>> expression)
		{
			return VisitAndOr(expression, ExprType.And);
		}

		internal override StringBuilder VisitOr(OrExpr<DomainConstraint<BoolLiteral, Constant>> expression)
		{
			return VisitAndOr(expression, ExprType.Or);
		}

		private StringBuilder VisitAndOr(TreeExpr<DomainConstraint<BoolLiteral, Constant>> expression, ExprType kind)
		{
			m_builder.Append('(');
			bool flag = true;
			foreach (BoolExpr<DomainConstraint<BoolLiteral, Constant>> child in expression.Children)
			{
				if (!flag)
				{
					if (kind == ExprType.And)
					{
						m_builder.Append(" AND ");
					}
					else
					{
						m_builder.Append(" OR ");
					}
				}
				flag = false;
				child.Accept(this);
			}
			m_builder.Append(')');
			return m_builder;
		}
	}

	private class TermVisitor : Visitor<DomainConstraint<BoolLiteral, Constant>, IEnumerable<TermExpr<DomainConstraint<BoolLiteral, Constant>>>>
	{
		private TermVisitor(bool allowAllOperators)
		{
		}

		internal static IEnumerable<TermExpr<DomainConstraint<BoolLiteral, Constant>>> GetTerms(BoolExpr<DomainConstraint<BoolLiteral, Constant>> expression, bool allowAllOperators)
		{
			TermVisitor visitor = new TermVisitor(allowAllOperators);
			return expression.Accept(visitor);
		}

		internal override IEnumerable<TermExpr<DomainConstraint<BoolLiteral, Constant>>> VisitTrue(TrueExpr<DomainConstraint<BoolLiteral, Constant>> expression)
		{
			yield break;
		}

		internal override IEnumerable<TermExpr<DomainConstraint<BoolLiteral, Constant>>> VisitFalse(FalseExpr<DomainConstraint<BoolLiteral, Constant>> expression)
		{
			yield break;
		}

		internal override IEnumerable<TermExpr<DomainConstraint<BoolLiteral, Constant>>> VisitTerm(TermExpr<DomainConstraint<BoolLiteral, Constant>> expression)
		{
			yield return expression;
		}

		internal override IEnumerable<TermExpr<DomainConstraint<BoolLiteral, Constant>>> VisitNot(NotExpr<DomainConstraint<BoolLiteral, Constant>> expression)
		{
			return VisitTreeNode(expression);
		}

		private IEnumerable<TermExpr<DomainConstraint<BoolLiteral, Constant>>> VisitTreeNode(TreeExpr<DomainConstraint<BoolLiteral, Constant>> expression)
		{
			foreach (BoolExpr<DomainConstraint<BoolLiteral, Constant>> child in expression.Children)
			{
				foreach (TermExpr<DomainConstraint<BoolLiteral, Constant>> item in child.Accept(this))
				{
					yield return item;
				}
			}
		}

		internal override IEnumerable<TermExpr<DomainConstraint<BoolLiteral, Constant>>> VisitAnd(AndExpr<DomainConstraint<BoolLiteral, Constant>> expression)
		{
			return VisitTreeNode(expression);
		}

		internal override IEnumerable<TermExpr<DomainConstraint<BoolLiteral, Constant>>> VisitOr(OrExpr<DomainConstraint<BoolLiteral, Constant>> expression)
		{
			return VisitTreeNode(expression);
		}
	}

	private class CompactStringVisitor : Visitor<DomainConstraint<BoolLiteral, Constant>, StringBuilder>
	{
		private StringBuilder m_builder;

		private CompactStringVisitor(StringBuilder builder)
		{
			m_builder = builder;
		}

		internal static StringBuilder ToBuilder(BoolExpr<DomainConstraint<BoolLiteral, Constant>> expression, StringBuilder builder)
		{
			CompactStringVisitor visitor = new CompactStringVisitor(builder);
			return expression.Accept(visitor);
		}

		internal override StringBuilder VisitTrue(TrueExpr<DomainConstraint<BoolLiteral, Constant>> expression)
		{
			m_builder.Append("True");
			return m_builder;
		}

		internal override StringBuilder VisitFalse(FalseExpr<DomainConstraint<BoolLiteral, Constant>> expression)
		{
			m_builder.Append("False");
			return m_builder;
		}

		internal override StringBuilder VisitTerm(TermExpr<DomainConstraint<BoolLiteral, Constant>> expression)
		{
			GetBoolLiteral(expression).ToCompactString(m_builder);
			return m_builder;
		}

		internal override StringBuilder VisitNot(NotExpr<DomainConstraint<BoolLiteral, Constant>> expression)
		{
			m_builder.Append("NOT(");
			expression.Child.Accept(this);
			m_builder.Append(")");
			return m_builder;
		}

		internal override StringBuilder VisitAnd(AndExpr<DomainConstraint<BoolLiteral, Constant>> expression)
		{
			return VisitAndOr(expression, "AND");
		}

		internal override StringBuilder VisitOr(OrExpr<DomainConstraint<BoolLiteral, Constant>> expression)
		{
			return VisitAndOr(expression, "OR");
		}

		private StringBuilder VisitAndOr(TreeExpr<DomainConstraint<BoolLiteral, Constant>> expression, string opAsString)
		{
			List<string> list = new List<string>();
			StringBuilder builder = m_builder;
			foreach (BoolExpr<DomainConstraint<BoolLiteral, Constant>> child in expression.Children)
			{
				m_builder = new StringBuilder();
				child.Accept(this);
				list.Add(m_builder.ToString());
			}
			m_builder = builder;
			m_builder.Append('(');
			StringUtil.ToSeparatedStringSorted(m_builder, list, " " + opAsString + " ");
			m_builder.Append(')');
			return m_builder;
		}
	}

	private BoolExpr<DomainConstraint<BoolLiteral, Constant>> m_tree;

	private readonly MemberDomainMap m_memberDomainMap;

	private Converter<DomainConstraint<BoolLiteral, Constant>> m_converter;

	internal static readonly IEqualityComparer<BoolExpression> EqualityComparer = new BoolComparer();

	internal static readonly BoolExpression True = new BoolExpression(isTrue: true);

	internal static readonly BoolExpression False = new BoolExpression(isTrue: false);

	private static readonly CopyVisitor _copyVisitorInstance = new CopyVisitor();

	internal IEnumerable<BoolExpression> Atoms
	{
		get
		{
			IEnumerable<TermExpr<DomainConstraint<BoolLiteral, Constant>>> terms = TermVisitor.GetTerms(m_tree, allowAllOperators: false);
			foreach (TermExpr<DomainConstraint<BoolLiteral, Constant>> item in terms)
			{
				yield return new BoolExpression(item, m_memberDomainMap);
			}
		}
	}

	internal BoolLiteral AsLiteral
	{
		get
		{
			if (!(m_tree is TermExpr<DomainConstraint<BoolLiteral, Constant>> term))
			{
				return null;
			}
			return GetBoolLiteral(term);
		}
	}

	internal bool IsTrue => m_tree.ExprType == ExprType.True;

	internal bool IsFalse => m_tree.ExprType == ExprType.False;

	internal BoolExpr<DomainConstraint<BoolLiteral, Constant>> Tree => m_tree;

	internal IEnumerable<DomainConstraint<BoolLiteral, Constant>> VariableConstraints => LeafVisitor<DomainConstraint<BoolLiteral, Constant>>.GetLeaves(m_tree);

	internal IEnumerable<DomainVariable<BoolLiteral, Constant>> Variables => VariableConstraints.Select((DomainConstraint<BoolLiteral, Constant> domainConstraint) => domainConstraint.Variable);

	internal IEnumerable<MemberRestriction> MemberRestrictions
	{
		get
		{
			foreach (DomainVariable<BoolLiteral, Constant> variable in Variables)
			{
				if (variable.Identifier is MemberRestriction memberRestriction)
				{
					yield return memberRestriction;
				}
			}
		}
	}

	internal bool RepresentsAllTypeConditions => MemberRestrictions.All((MemberRestriction var) => var is TypeRestriction);

	internal static BoolExpression CreateLiteral(BoolLiteral literal, MemberDomainMap memberDomainMap)
	{
		return new BoolExpression(literal.GetDomainBoolExpression(memberDomainMap), memberDomainMap);
	}

	internal BoolExpression Create(BoolLiteral literal)
	{
		return new BoolExpression(literal.GetDomainBoolExpression(m_memberDomainMap), m_memberDomainMap);
	}

	internal static BoolExpression CreateNot(BoolExpression expression)
	{
		return new BoolExpression(ExprType.Not, new BoolExpression[1] { expression });
	}

	internal static BoolExpression CreateAnd(params BoolExpression[] children)
	{
		return new BoolExpression(ExprType.And, children);
	}

	internal static BoolExpression CreateOr(params BoolExpression[] children)
	{
		return new BoolExpression(ExprType.Or, children);
	}

	internal static BoolExpression CreateAndNot(BoolExpression e1, BoolExpression e2)
	{
		return CreateAnd(e1, CreateNot(e2));
	}

	internal BoolExpression Create(BoolExpr<DomainConstraint<BoolLiteral, Constant>> expression)
	{
		return new BoolExpression(expression, m_memberDomainMap);
	}

	private BoolExpression(bool isTrue)
	{
		if (isTrue)
		{
			m_tree = TrueExpr<DomainConstraint<BoolLiteral, Constant>>.Value;
		}
		else
		{
			m_tree = FalseExpr<DomainConstraint<BoolLiteral, Constant>>.Value;
		}
	}

	private BoolExpression(ExprType opType, IEnumerable<BoolExpression> children)
	{
		List<BoolExpression> list = new List<BoolExpression>(children);
		foreach (BoolExpression child in children)
		{
			if (child.m_memberDomainMap != null)
			{
				m_memberDomainMap = child.m_memberDomainMap;
				break;
			}
		}
		switch (opType)
		{
		case ExprType.And:
			m_tree = new AndExpr<DomainConstraint<BoolLiteral, Constant>>(ToBoolExprList(list));
			break;
		case ExprType.Or:
			m_tree = new OrExpr<DomainConstraint<BoolLiteral, Constant>>(ToBoolExprList(list));
			break;
		case ExprType.Not:
			m_tree = new NotExpr<DomainConstraint<BoolLiteral, Constant>>(list[0].m_tree);
			break;
		}
	}

	internal BoolExpression(BoolExpr<DomainConstraint<BoolLiteral, Constant>> expr, MemberDomainMap memberDomainMap)
	{
		m_tree = expr;
		m_memberDomainMap = memberDomainMap;
	}

	internal static BoolLiteral GetBoolLiteral(TermExpr<DomainConstraint<BoolLiteral, Constant>> term)
	{
		return term.Identifier.Variable.Identifier;
	}

	internal bool IsAlwaysTrue()
	{
		InitializeConverter();
		return m_converter.Vertex.IsOne();
	}

	internal bool IsSatisfiable()
	{
		return !IsUnsatisfiable();
	}

	internal bool IsUnsatisfiable()
	{
		InitializeConverter();
		return m_converter.Vertex.IsZero();
	}

	private static IEnumerable<BoolExpr<DomainConstraint<BoolLiteral, Constant>>> ToBoolExprList(IEnumerable<BoolExpression> nodes)
	{
		foreach (BoolExpression node in nodes)
		{
			yield return node.m_tree;
		}
	}

	internal BoolExpression RemapLiterals(Dictionary<BoolLiteral, BoolLiteral> remap)
	{
		BoolLiteral value;
		BooleanExpressionTermRewriter<DomainConstraint<BoolLiteral, Constant>, DomainConstraint<BoolLiteral, Constant>> visitor = new BooleanExpressionTermRewriter<DomainConstraint<BoolLiteral, Constant>, DomainConstraint<BoolLiteral, Constant>>((TermExpr<DomainConstraint<BoolLiteral, Constant>> term) => (!remap.TryGetValue(GetBoolLiteral(term), out value)) ? term : value.GetDomainBoolExpression(m_memberDomainMap));
		return new BoolExpression(m_tree.Accept(visitor), m_memberDomainMap);
	}

	internal virtual void GetRequiredSlots(MemberProjectionIndex projectedSlotMap, bool[] requiredSlots)
	{
		RequiredSlotsVisitor.GetRequiredSlots(m_tree, projectedSlotMap, requiredSlots);
	}

	internal StringBuilder AsEsql(StringBuilder builder, string blockAlias)
	{
		return AsEsqlVisitor.AsEsql(m_tree, builder, blockAlias);
	}

	internal DbExpression AsCqt(DbExpression row)
	{
		return AsCqtVisitor.AsCqt(m_tree, row);
	}

	internal StringBuilder AsUserString(StringBuilder builder, string blockAlias, bool writeRoundtrippingMessage)
	{
		if (writeRoundtrippingMessage)
		{
			builder.AppendLine(Strings.Viewgen_ConfigurationErrorMsg(blockAlias));
			builder.Append("  ");
		}
		return AsUserStringVisitor.AsUserString(m_tree, builder, blockAlias);
	}

	internal override void ToCompactString(StringBuilder builder)
	{
		CompactStringVisitor.ToBuilder(m_tree, builder);
	}

	internal BoolExpression RemapBool(Dictionary<MemberPath, MemberPath> remap)
	{
		return new BoolExpression(RemapBoolVisitor.RemapExtentTreeNodes(m_tree, m_memberDomainMap, remap), m_memberDomainMap);
	}

	internal static List<BoolExpression> AddConjunctionToBools(List<BoolExpression> bools, BoolExpression conjunct)
	{
		List<BoolExpression> list = new List<BoolExpression>();
		foreach (BoolExpression @bool in bools)
		{
			if (@bool == null)
			{
				list.Add(null);
				continue;
			}
			list.Add(CreateAnd(@bool, conjunct));
		}
		return list;
	}

	private void InitializeConverter()
	{
		if (m_converter == null)
		{
			m_converter = new Converter<DomainConstraint<BoolLiteral, Constant>>(m_tree, IdentifierService<DomainConstraint<BoolLiteral, Constant>>.Instance.CreateConversionContext());
		}
	}

	internal BoolExpression MakeCopy()
	{
		return Create(m_tree.Accept(_copyVisitorInstance));
	}

	internal void ExpensiveSimplify()
	{
		if (!IsFinal())
		{
			m_tree = m_tree.Simplify();
			return;
		}
		InitializeConverter();
		m_tree = m_tree.ExpensiveSimplify(out m_converter);
		FixDomainMap(m_memberDomainMap);
	}

	internal void FixDomainMap(MemberDomainMap domainMap)
	{
		m_tree = FixRangeVisitor.FixRange(m_tree, domainMap);
	}

	private bool IsFinal()
	{
		if (m_memberDomainMap != null)
		{
			return IsFinalVisitor.IsFinal(m_tree);
		}
		return false;
	}
}
