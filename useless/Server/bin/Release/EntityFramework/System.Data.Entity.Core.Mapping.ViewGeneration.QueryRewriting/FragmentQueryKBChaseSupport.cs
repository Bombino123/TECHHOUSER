using System.Collections.Generic;
using System.Data.Entity.Core.Common.Utils;
using System.Data.Entity.Core.Common.Utils.Boolean;
using System.Data.Entity.Core.Mapping.ViewGeneration.Structures;
using System.Linq;

namespace System.Data.Entity.Core.Mapping.ViewGeneration.QueryRewriting;

internal class FragmentQueryKBChaseSupport : FragmentQueryKB
{
	private static class Normalizer
	{
		private class NonNegatedTreeVisitor : BasicVisitor<DomainConstraint<BoolLiteral, Constant>>
		{
			internal static readonly NonNegatedTreeVisitor Instance = new NonNegatedTreeVisitor();

			private NonNegatedTreeVisitor()
			{
			}

			internal override BoolExpr<DomainConstraint<BoolLiteral, Constant>> VisitNot(NotExpr<DomainConstraint<BoolLiteral, Constant>> expr)
			{
				return expr.Child.Accept(NegatedTreeVisitor.Instance);
			}

			internal override BoolExpr<DomainConstraint<BoolLiteral, Constant>> VisitTerm(TermExpr<DomainConstraint<BoolLiteral, Constant>> expression)
			{
				switch (expression.Identifier.Range.Count)
				{
				case 0:
					return FalseExpr<DomainConstraint<BoolLiteral, Constant>>.Value;
				case 1:
					return expression;
				default:
				{
					List<BoolExpr<DomainConstraint<BoolLiteral, Constant>>> list = new List<BoolExpr<DomainConstraint<BoolLiteral, Constant>>>();
					DomainVariable<BoolLiteral, Constant> variable = expression.Identifier.Variable;
					foreach (Constant item in expression.Identifier.Range)
					{
						list.Add(new DomainConstraint<BoolLiteral, Constant>(variable, new Set<Constant>(new Constant[1] { item }, Constant.EqualityComparer)));
					}
					return new OrExpr<DomainConstraint<BoolLiteral, Constant>>(list);
				}
				}
			}
		}

		private class NegatedTreeVisitor : Visitor<DomainConstraint<BoolLiteral, Constant>, BoolExpr<DomainConstraint<BoolLiteral, Constant>>>
		{
			internal static readonly NegatedTreeVisitor Instance = new NegatedTreeVisitor();

			private NegatedTreeVisitor()
			{
			}

			internal override BoolExpr<DomainConstraint<BoolLiteral, Constant>> VisitTrue(TrueExpr<DomainConstraint<BoolLiteral, Constant>> expression)
			{
				return FalseExpr<DomainConstraint<BoolLiteral, Constant>>.Value;
			}

			internal override BoolExpr<DomainConstraint<BoolLiteral, Constant>> VisitFalse(FalseExpr<DomainConstraint<BoolLiteral, Constant>> expression)
			{
				return TrueExpr<DomainConstraint<BoolLiteral, Constant>>.Value;
			}

			internal override BoolExpr<DomainConstraint<BoolLiteral, Constant>> VisitNot(NotExpr<DomainConstraint<BoolLiteral, Constant>> expression)
			{
				return expression.Child.Accept(NonNegatedTreeVisitor.Instance);
			}

			internal override BoolExpr<DomainConstraint<BoolLiteral, Constant>> VisitAnd(AndExpr<DomainConstraint<BoolLiteral, Constant>> expression)
			{
				return new OrExpr<DomainConstraint<BoolLiteral, Constant>>(expression.Children.Select((BoolExpr<DomainConstraint<BoolLiteral, Constant>> child) => child.Accept(this)));
			}

			internal override BoolExpr<DomainConstraint<BoolLiteral, Constant>> VisitOr(OrExpr<DomainConstraint<BoolLiteral, Constant>> expression)
			{
				return new AndExpr<DomainConstraint<BoolLiteral, Constant>>(expression.Children.Select((BoolExpr<DomainConstraint<BoolLiteral, Constant>> child) => child.Accept(this)));
			}

			internal override BoolExpr<DomainConstraint<BoolLiteral, Constant>> VisitTerm(TermExpr<DomainConstraint<BoolLiteral, Constant>> expression)
			{
				DomainConstraint<BoolLiteral, Constant> domainConstraint = expression.Identifier.InvertDomainConstraint();
				if (domainConstraint.Range.Count == 0)
				{
					return FalseExpr<DomainConstraint<BoolLiteral, Constant>>.Value;
				}
				List<BoolExpr<DomainConstraint<BoolLiteral, Constant>>> list = new List<BoolExpr<DomainConstraint<BoolLiteral, Constant>>>();
				DomainVariable<BoolLiteral, Constant> variable = domainConstraint.Variable;
				foreach (Constant item in domainConstraint.Range)
				{
					list.Add(new DomainConstraint<BoolLiteral, Constant>(variable, new Set<Constant>(new Constant[1] { item }, Constant.EqualityComparer)));
				}
				return new OrExpr<DomainConstraint<BoolLiteral, Constant>>(list);
			}
		}

		private class NonNegatedNnfSplitCounter : TermCounter<DomainConstraint<BoolLiteral, Constant>>
		{
			internal static readonly NonNegatedNnfSplitCounter Instance = new NonNegatedNnfSplitCounter();

			private NonNegatedNnfSplitCounter()
			{
			}

			internal override int VisitNot(NotExpr<DomainConstraint<BoolLiteral, Constant>> expr)
			{
				return expr.Child.Accept(NegatedNnfSplitCountEstimator.Instance);
			}

			internal override int VisitTerm(TermExpr<DomainConstraint<BoolLiteral, Constant>> expression)
			{
				return expression.Identifier.Range.Count;
			}
		}

		private class NegatedNnfSplitCountEstimator : TermCounter<DomainConstraint<BoolLiteral, Constant>>
		{
			internal static readonly NegatedNnfSplitCountEstimator Instance = new NegatedNnfSplitCountEstimator();

			private NegatedNnfSplitCountEstimator()
			{
			}

			internal override int VisitNot(NotExpr<DomainConstraint<BoolLiteral, Constant>> expression)
			{
				return expression.Child.Accept(NonNegatedNnfSplitCounter.Instance);
			}

			internal override int VisitTerm(TermExpr<DomainConstraint<BoolLiteral, Constant>> expression)
			{
				return expression.Identifier.Variable.Domain.Count - expression.Identifier.Range.Count;
			}
		}

		private class DnfTreeVisitor : BasicVisitor<DomainConstraint<BoolLiteral, Constant>>
		{
			internal static readonly DnfTreeVisitor Instance = new DnfTreeVisitor();

			private DnfTreeVisitor()
			{
			}

			internal override BoolExpr<DomainConstraint<BoolLiteral, Constant>> VisitNot(NotExpr<DomainConstraint<BoolLiteral, Constant>> expression)
			{
				return expression;
			}

			internal override BoolExpr<DomainConstraint<BoolLiteral, Constant>> VisitAnd(AndExpr<DomainConstraint<BoolLiteral, Constant>> expression)
			{
				BoolExpr<DomainConstraint<BoolLiteral, Constant>> boolExpr = base.VisitAnd(expression);
				if (!(boolExpr is TreeExpr<DomainConstraint<BoolLiteral, Constant>> treeExpr))
				{
					return boolExpr;
				}
				Set<BoolExpr<DomainConstraint<BoolLiteral, Constant>>> set = new Set<BoolExpr<DomainConstraint<BoolLiteral, Constant>>>();
				Set<Set<BoolExpr<DomainConstraint<BoolLiteral, Constant>>>> set2 = new Set<Set<BoolExpr<DomainConstraint<BoolLiteral, Constant>>>>();
				foreach (BoolExpr<DomainConstraint<BoolLiteral, Constant>> child in treeExpr.Children)
				{
					if (child is OrExpr<DomainConstraint<BoolLiteral, Constant>> orExpr)
					{
						set2.Add(new Set<BoolExpr<DomainConstraint<BoolLiteral, Constant>>>(orExpr.Children));
					}
					else
					{
						set.Add(child);
					}
				}
				set2.Add(new Set<BoolExpr<DomainConstraint<BoolLiteral, Constant>>>(new BoolExpr<DomainConstraint<BoolLiteral, Constant>>[1]
				{
					new AndExpr<DomainConstraint<BoolLiteral, Constant>>(set)
				}));
				IEnumerable<IEnumerable<BoolExpr<DomainConstraint<BoolLiteral, Constant>>>> seed = new IEnumerable<BoolExpr<DomainConstraint<BoolLiteral, Constant>>>[1] { Enumerable.Empty<BoolExpr<DomainConstraint<BoolLiteral, Constant>>>() };
				IEnumerable<IEnumerable<BoolExpr<DomainConstraint<BoolLiteral, Constant>>>> enumerable = set2.Aggregate(seed, (IEnumerable<IEnumerable<BoolExpr<DomainConstraint<BoolLiteral, Constant>>>> accumulator, Set<BoolExpr<DomainConstraint<BoolLiteral, Constant>>> bucket) => from accseq in accumulator
					from item in bucket
					select accseq.Concat(new BoolExpr<DomainConstraint<BoolLiteral, Constant>>[1] { item }));
				List<BoolExpr<DomainConstraint<BoolLiteral, Constant>>> list = new List<BoolExpr<DomainConstraint<BoolLiteral, Constant>>>();
				foreach (IEnumerable<BoolExpr<DomainConstraint<BoolLiteral, Constant>>> item in enumerable)
				{
					list.Add(new AndExpr<DomainConstraint<BoolLiteral, Constant>>(item));
				}
				return new OrExpr<DomainConstraint<BoolLiteral, Constant>>(list);
			}
		}

		internal static BoolExpr<DomainConstraint<BoolLiteral, Constant>> ToNnfAndSplitRange(BoolExpr<DomainConstraint<BoolLiteral, Constant>> expr)
		{
			return expr.Accept(NonNegatedTreeVisitor.Instance);
		}

		internal static int EstimateNnfAndSplitTermCount(BoolExpr<DomainConstraint<BoolLiteral, Constant>> expr)
		{
			return expr.Accept(NonNegatedNnfSplitCounter.Instance);
		}

		internal static BoolExpr<DomainConstraint<BoolLiteral, Constant>> ToDnf(BoolExpr<DomainConstraint<BoolLiteral, Constant>> expr, bool isNnf)
		{
			if (!isNnf)
			{
				expr = ToNnfAndSplitRange(expr);
			}
			return expr.Accept(DnfTreeVisitor.Instance);
		}
	}

	private class AtomicConditionRuleChase
	{
		private class NonNegatedDomainConstraintTreeVisitor : BasicVisitor<DomainConstraint<BoolLiteral, Constant>>
		{
			private readonly FragmentQueryKBChaseSupport _kb;

			internal NonNegatedDomainConstraintTreeVisitor(FragmentQueryKBChaseSupport kb)
			{
				_kb = kb;
			}

			internal override BoolExpr<DomainConstraint<BoolLiteral, Constant>> VisitTerm(TermExpr<DomainConstraint<BoolLiteral, Constant>> expression)
			{
				return _kb.Chase(expression);
			}

			internal override BoolExpr<DomainConstraint<BoolLiteral, Constant>> VisitNot(NotExpr<DomainConstraint<BoolLiteral, Constant>> expression)
			{
				return base.VisitNot(expression);
			}
		}

		private readonly NonNegatedDomainConstraintTreeVisitor _visitor;

		internal AtomicConditionRuleChase(FragmentQueryKBChaseSupport kb)
		{
			_visitor = new NonNegatedDomainConstraintTreeVisitor(kb);
		}

		internal BoolExpr<DomainConstraint<BoolLiteral, Constant>> Chase(BoolExpr<DomainConstraint<BoolLiteral, Constant>> expression)
		{
			return expression.Accept(_visitor);
		}
	}

	private Dictionary<TermExpr<DomainConstraint<BoolLiteral, Constant>>, BoolExpr<DomainConstraint<BoolLiteral, Constant>>> _implications;

	private readonly AtomicConditionRuleChase _chase;

	private Set<BoolExpr<DomainConstraint<BoolLiteral, Constant>>> _residualFacts = new Set<BoolExpr<DomainConstraint<BoolLiteral, Constant>>>();

	private int _kbSize;

	private int _residueSize = -1;

	internal Dictionary<TermExpr<DomainConstraint<BoolLiteral, Constant>>, BoolExpr<DomainConstraint<BoolLiteral, Constant>>> Implications
	{
		get
		{
			if (_implications == null)
			{
				_implications = new Dictionary<TermExpr<DomainConstraint<BoolLiteral, Constant>>, BoolExpr<DomainConstraint<BoolLiteral, Constant>>>();
				foreach (BoolExpr<DomainConstraint<BoolLiteral, Constant>> fact in base.Facts)
				{
					CacheFact(fact);
				}
			}
			return _implications;
		}
	}

	private IEnumerable<BoolExpr<DomainConstraint<BoolLiteral, Constant>>> ResidueInternal
	{
		get
		{
			if (_residueSize < 0 && _residualFacts.Count > 0)
			{
				PrepareResidue();
			}
			return _residualFacts;
		}
	}

	private int ResidueSize
	{
		get
		{
			if (_residueSize < 0)
			{
				PrepareResidue();
			}
			return _residueSize;
		}
	}

	internal FragmentQueryKBChaseSupport()
	{
		_chase = new AtomicConditionRuleChase(this);
	}

	internal override void AddFact(BoolExpr<DomainConstraint<BoolLiteral, Constant>> fact)
	{
		base.AddFact(fact);
		_kbSize += fact.CountTerms();
		if (_implications != null)
		{
			CacheFact(fact);
		}
	}

	private void CacheFact(BoolExpr<DomainConstraint<BoolLiteral, Constant>> fact)
	{
		Implication implication = fact as Implication;
		Equivalence equivalence = fact as Equivalence;
		if (implication != null)
		{
			CacheImplication(implication.Condition, implication.Implies);
		}
		else if (equivalence != null)
		{
			CacheImplication(equivalence.Left, equivalence.Right);
			CacheImplication(equivalence.Right, equivalence.Left);
		}
		else
		{
			CacheResidualFact(fact);
		}
	}

	internal BoolExpr<DomainConstraint<BoolLiteral, Constant>> Chase(TermExpr<DomainConstraint<BoolLiteral, Constant>> expression)
	{
		Implications.TryGetValue(expression, out var value);
		return new AndExpr<DomainConstraint<BoolLiteral, Constant>>(expression, value ?? TrueExpr<DomainConstraint<BoolLiteral, Constant>>.Value);
	}

	internal bool IsSatisfiable(BoolExpr<DomainConstraint<BoolLiteral, Constant>> expression)
	{
		ConversionContext<DomainConstraint<BoolLiteral, Constant>> context = IdentifierService<DomainConstraint<BoolLiteral, Constant>>.Instance.CreateConversionContext();
		Converter<DomainConstraint<BoolLiteral, Constant>> converter = new Converter<DomainConstraint<BoolLiteral, Constant>>(expression, context);
		if (converter.Vertex.IsZero())
		{
			return false;
		}
		if (base.KbExpression.ExprType == ExprType.True)
		{
			return true;
		}
		int num = expression.CountTerms() + _kbSize;
		BoolExpr<DomainConstraint<BoolLiteral, Constant>> expr = converter.Dnf.Expr;
		BoolExpr<DomainConstraint<BoolLiteral, Constant>> expr2 = ((Normalizer.EstimateNnfAndSplitTermCount(expr) > Normalizer.EstimateNnfAndSplitTermCount(expression)) ? expression : expr);
		BoolExpr<DomainConstraint<BoolLiteral, Constant>> boolExpr = _chase.Chase(Normalizer.ToNnfAndSplitRange(expr2));
		BoolExpr<DomainConstraint<BoolLiteral, Constant>> expr3;
		if (boolExpr.CountTerms() + ResidueSize > num)
		{
			expr3 = new AndExpr<DomainConstraint<BoolLiteral, Constant>>(base.KbExpression, expression);
		}
		else
		{
			expr3 = new AndExpr<DomainConstraint<BoolLiteral, Constant>>(new List<BoolExpr<DomainConstraint<BoolLiteral, Constant>>>(ResidueInternal) { boolExpr });
			context = IdentifierService<DomainConstraint<BoolLiteral, Constant>>.Instance.CreateConversionContext();
		}
		return !new Converter<DomainConstraint<BoolLiteral, Constant>>(expr3, context).Vertex.IsZero();
	}

	internal BoolExpr<DomainConstraint<BoolLiteral, Constant>> Chase(BoolExpr<DomainConstraint<BoolLiteral, Constant>> expression)
	{
		if (Implications.Count != 0)
		{
			return _chase.Chase(Normalizer.ToNnfAndSplitRange(expression));
		}
		return expression;
	}

	private void CacheImplication(BoolExpr<DomainConstraint<BoolLiteral, Constant>> condition, BoolExpr<DomainConstraint<BoolLiteral, Constant>> implies)
	{
		BoolExpr<DomainConstraint<BoolLiteral, Constant>> boolExpr = Normalizer.ToDnf(condition, isNnf: false);
		BoolExpr<DomainConstraint<BoolLiteral, Constant>> implies2 = Normalizer.ToNnfAndSplitRange(implies);
		switch (boolExpr.ExprType)
		{
		case ExprType.Or:
		{
			foreach (BoolExpr<DomainConstraint<BoolLiteral, Constant>> child in ((OrExpr<DomainConstraint<BoolLiteral, Constant>>)boolExpr).Children)
			{
				if (child.ExprType != ExprType.Term)
				{
					CacheResidualFact(new OrExpr<DomainConstraint<BoolLiteral, Constant>>(new NotExpr<DomainConstraint<BoolLiteral, Constant>>(child), implies));
				}
				else
				{
					CacheNormalizedImplication((TermExpr<DomainConstraint<BoolLiteral, Constant>>)child, implies2);
				}
			}
			break;
		}
		case ExprType.Term:
			CacheNormalizedImplication((TermExpr<DomainConstraint<BoolLiteral, Constant>>)boolExpr, implies2);
			break;
		default:
			CacheResidualFact(new OrExpr<DomainConstraint<BoolLiteral, Constant>>(new NotExpr<DomainConstraint<BoolLiteral, Constant>>(condition), implies));
			break;
		}
	}

	private void CacheNormalizedImplication(TermExpr<DomainConstraint<BoolLiteral, Constant>> condition, BoolExpr<DomainConstraint<BoolLiteral, Constant>> implies)
	{
		foreach (TermExpr<DomainConstraint<BoolLiteral, Constant>> key in Implications.Keys)
		{
			if (key.Identifier.Variable.Equals(condition.Identifier.Variable) && !key.Identifier.Range.SetEquals(condition.Identifier.Range))
			{
				CacheResidualFact(new OrExpr<DomainConstraint<BoolLiteral, Constant>>(new NotExpr<DomainConstraint<BoolLiteral, Constant>>(condition), implies));
				return;
			}
		}
		BoolExpr<DomainConstraint<BoolLiteral, Constant>> expr = new Converter<DomainConstraint<BoolLiteral, Constant>>(Chase(implies), IdentifierService<DomainConstraint<BoolLiteral, Constant>>.Instance.CreateConversionContext()).Dnf.Expr;
		FragmentQueryKBChaseSupport fragmentQueryKBChaseSupport = new FragmentQueryKBChaseSupport();
		fragmentQueryKBChaseSupport.Implications[condition] = expr;
		bool flag = true;
		foreach (TermExpr<DomainConstraint<BoolLiteral, Constant>> item in new Set<TermExpr<DomainConstraint<BoolLiteral, Constant>>>(Implications.Keys))
		{
			BoolExpr<DomainConstraint<BoolLiteral, Constant>> boolExpr = fragmentQueryKBChaseSupport.Chase(Implications[item]);
			if (item.Equals(condition))
			{
				flag = false;
				boolExpr = new AndExpr<DomainConstraint<BoolLiteral, Constant>>(boolExpr, expr);
			}
			Implications[item] = new Converter<DomainConstraint<BoolLiteral, Constant>>(boolExpr, IdentifierService<DomainConstraint<BoolLiteral, Constant>>.Instance.CreateConversionContext()).Dnf.Expr;
		}
		if (flag)
		{
			Implications[condition] = expr;
		}
		_residueSize = -1;
	}

	private void CacheResidualFact(BoolExpr<DomainConstraint<BoolLiteral, Constant>> fact)
	{
		_residualFacts.Add(fact);
		_residueSize = -1;
	}

	private void PrepareResidue()
	{
		int num = 0;
		if (Implications.Count > 0 && _residualFacts.Count > 0)
		{
			Set<BoolExpr<DomainConstraint<BoolLiteral, Constant>>> set = new Set<BoolExpr<DomainConstraint<BoolLiteral, Constant>>>();
			foreach (BoolExpr<DomainConstraint<BoolLiteral, Constant>> residualFact in _residualFacts)
			{
				BoolExpr<DomainConstraint<BoolLiteral, Constant>> expr = new Converter<DomainConstraint<BoolLiteral, Constant>>(Chase(residualFact), IdentifierService<DomainConstraint<BoolLiteral, Constant>>.Instance.CreateConversionContext()).Dnf.Expr;
				set.Add(expr);
				num = (_residueSize = num + expr.CountTerms());
			}
			_residualFacts = set;
		}
		_residueSize = num;
	}
}
