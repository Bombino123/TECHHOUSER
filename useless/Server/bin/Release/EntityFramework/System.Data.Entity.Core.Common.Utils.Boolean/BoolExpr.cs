using System.Collections.Generic;
using System.Linq;

namespace System.Data.Entity.Core.Common.Utils.Boolean;

internal abstract class BoolExpr<T_Identifier> : IEquatable<BoolExpr<T_Identifier>>
{
	internal abstract ExprType ExprType { get; }

	internal abstract T_Return Accept<T_Return>(Visitor<T_Identifier, T_Return> visitor);

	internal BoolExpr<T_Identifier> Simplify()
	{
		return IdentifierService<T_Identifier>.Instance.LocalSimplify(this);
	}

	internal BoolExpr<T_Identifier> ExpensiveSimplify(out Converter<T_Identifier> converter)
	{
		ConversionContext<T_Identifier> context = IdentifierService<T_Identifier>.Instance.CreateConversionContext();
		converter = new Converter<T_Identifier>(this, context);
		if (converter.Vertex.IsOne())
		{
			return TrueExpr<T_Identifier>.Value;
		}
		if (converter.Vertex.IsZero())
		{
			return FalseExpr<T_Identifier>.Value;
		}
		return ChooseCandidate(this, converter.Cnf.Expr, converter.Dnf.Expr);
	}

	private static BoolExpr<T_Identifier> ChooseCandidate(params BoolExpr<T_Identifier>[] candidates)
	{
		int num = 0;
		int num2 = 0;
		BoolExpr<T_Identifier> boolExpr = null;
		for (int i = 0; i < candidates.Length; i++)
		{
			BoolExpr<T_Identifier> boolExpr2 = candidates[i].Simplify();
			int num3 = boolExpr2.GetTerms().Distinct().Count();
			int num4 = boolExpr2.CountTerms();
			if (boolExpr == null || num3 < num || (num3 == num && num4 < num2))
			{
				boolExpr = boolExpr2;
				num = num3;
				num2 = num4;
			}
		}
		return boolExpr;
	}

	internal List<TermExpr<T_Identifier>> GetTerms()
	{
		return LeafVisitor<T_Identifier>.GetTerms(this);
	}

	internal int CountTerms()
	{
		return TermCounter<T_Identifier>.CountTerms(this);
	}

	public static implicit operator BoolExpr<T_Identifier>(T_Identifier value)
	{
		return new TermExpr<T_Identifier>(value);
	}

	internal virtual BoolExpr<T_Identifier> MakeNegated()
	{
		return new NotExpr<T_Identifier>(this);
	}

	public override string ToString()
	{
		return ExprType.ToString();
	}

	public bool Equals(BoolExpr<T_Identifier> other)
	{
		if (other != null && ExprType == other.ExprType)
		{
			return EquivalentTypeEquals(other);
		}
		return false;
	}

	protected abstract bool EquivalentTypeEquals(BoolExpr<T_Identifier> other);
}
