using System.Collections.Generic;

namespace System.Data.Entity.Core.Common.Utils.Boolean;

internal sealed class TermExpr<T_Identifier> : BoolExpr<T_Identifier>, IEquatable<TermExpr<T_Identifier>>
{
	private readonly T_Identifier _identifier;

	private readonly IEqualityComparer<T_Identifier> _comparer;

	internal T_Identifier Identifier => _identifier;

	internal override ExprType ExprType => ExprType.Term;

	internal TermExpr(IEqualityComparer<T_Identifier> comparer, T_Identifier identifier)
	{
		_identifier = identifier;
		if (comparer == null)
		{
			_comparer = EqualityComparer<T_Identifier>.Default;
		}
		else
		{
			_comparer = comparer;
		}
	}

	internal TermExpr(T_Identifier identifier)
		: this((IEqualityComparer<T_Identifier>)null, identifier)
	{
	}

	public override bool Equals(object obj)
	{
		return Equals(obj as TermExpr<T_Identifier>);
	}

	public bool Equals(TermExpr<T_Identifier> other)
	{
		return _comparer.Equals(_identifier, other._identifier);
	}

	protected override bool EquivalentTypeEquals(BoolExpr<T_Identifier> other)
	{
		return _comparer.Equals(_identifier, ((TermExpr<T_Identifier>)other)._identifier);
	}

	public override int GetHashCode()
	{
		return _comparer.GetHashCode(_identifier);
	}

	public override string ToString()
	{
		return StringUtil.FormatInvariant("{0}", _identifier);
	}

	internal override T_Return Accept<T_Return>(Visitor<T_Identifier, T_Return> visitor)
	{
		return visitor.VisitTerm(this);
	}

	internal override BoolExpr<T_Identifier> MakeNegated()
	{
		Literal<T_Identifier> literal = new Literal<T_Identifier>(this, isTermPositive: true).MakeNegated();
		if (literal.IsTermPositive)
		{
			return literal.Term;
		}
		return new NotExpr<T_Identifier>(literal.Term);
	}
}
