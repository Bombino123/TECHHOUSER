using System.Collections.Generic;
using System.Text;

namespace System.Data.Entity.Core.Common.Utils.Boolean;

internal class KnowledgeBase<T_Identifier>
{
	protected class Implication : OrExpr<T_Identifier>
	{
		private readonly BoolExpr<T_Identifier> _condition;

		private readonly BoolExpr<T_Identifier> _implies;

		internal BoolExpr<T_Identifier> Condition => _condition;

		internal BoolExpr<T_Identifier> Implies => _implies;

		internal Implication(BoolExpr<T_Identifier> condition, BoolExpr<T_Identifier> implies)
			: base(new BoolExpr<T_Identifier>[2]
			{
				condition.MakeNegated(),
				implies
			})
		{
			_condition = condition;
			_implies = implies;
		}

		public override string ToString()
		{
			return StringUtil.FormatInvariant("{0} --> {1}", _condition, _implies);
		}
	}

	protected class Equivalence : AndExpr<T_Identifier>
	{
		private readonly BoolExpr<T_Identifier> _left;

		private readonly BoolExpr<T_Identifier> _right;

		internal BoolExpr<T_Identifier> Left => _left;

		internal BoolExpr<T_Identifier> Right => _right;

		internal Equivalence(BoolExpr<T_Identifier> left, BoolExpr<T_Identifier> right)
			: base(new BoolExpr<T_Identifier>[2]
			{
				new Implication(left, right),
				new Implication(right, left)
			})
		{
			_left = left;
			_right = right;
		}

		public override string ToString()
		{
			return StringUtil.FormatInvariant("{0} <--> {1}", _left, _right);
		}
	}

	private readonly List<BoolExpr<T_Identifier>> _facts;

	private Vertex _knowledge;

	private readonly ConversionContext<T_Identifier> _context;

	protected IEnumerable<BoolExpr<T_Identifier>> Facts => _facts;

	internal KnowledgeBase()
	{
		_facts = new List<BoolExpr<T_Identifier>>();
		_knowledge = Vertex.One;
		_context = IdentifierService<T_Identifier>.Instance.CreateConversionContext();
	}

	internal void AddKnowledgeBase(KnowledgeBase<T_Identifier> kb)
	{
		foreach (BoolExpr<T_Identifier> fact in kb._facts)
		{
			AddFact(fact);
		}
	}

	internal virtual void AddFact(BoolExpr<T_Identifier> fact)
	{
		_facts.Add(fact);
		Vertex vertex = new Converter<T_Identifier>(fact, _context).Vertex;
		_knowledge = _context.Solver.And(_knowledge, vertex);
	}

	internal void AddImplication(BoolExpr<T_Identifier> condition, BoolExpr<T_Identifier> implies)
	{
		AddFact(new Implication(condition, implies));
	}

	internal void AddEquivalence(BoolExpr<T_Identifier> left, BoolExpr<T_Identifier> right)
	{
		AddFact(new Equivalence(left, right));
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine("Facts:");
		foreach (BoolExpr<T_Identifier> fact in _facts)
		{
			stringBuilder.Append("\t").AppendLine(fact.ToString());
		}
		return stringBuilder.ToString();
	}
}
