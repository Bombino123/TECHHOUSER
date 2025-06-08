namespace System.Data.Entity.Core.Common.Utils.Boolean;

internal sealed class Literal<T_Identifier> : NormalFormNode<T_Identifier>, IEquatable<Literal<T_Identifier>>
{
	private readonly TermExpr<T_Identifier> _term;

	private readonly bool _isTermPositive;

	internal TermExpr<T_Identifier> Term => _term;

	internal bool IsTermPositive => _isTermPositive;

	internal Literal(TermExpr<T_Identifier> term, bool isTermPositive)
		: base(isTermPositive ? ((BoolExpr<T_Identifier>)term) : ((BoolExpr<T_Identifier>)new NotExpr<T_Identifier>(term)))
	{
		_term = term;
		_isTermPositive = isTermPositive;
	}

	internal Literal<T_Identifier> MakeNegated()
	{
		return IdentifierService<T_Identifier>.Instance.NegateLiteral(this);
	}

	public override string ToString()
	{
		return StringUtil.FormatInvariant("{0}{1}", _isTermPositive ? string.Empty : "!", _term);
	}

	public override bool Equals(object obj)
	{
		return Equals(obj as Literal<T_Identifier>);
	}

	public bool Equals(Literal<T_Identifier> other)
	{
		if (other != null && other._isTermPositive == _isTermPositive)
		{
			return other._term.Equals(_term);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return _term.GetHashCode();
	}
}
