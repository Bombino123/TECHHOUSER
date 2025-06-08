namespace System.Data.Entity.Core.Common.Utils.Boolean;

internal sealed class CnfClause<T_Identifier> : Clause<T_Identifier>, IEquatable<CnfClause<T_Identifier>>
{
	internal CnfClause(Set<Literal<T_Identifier>> literals)
		: base(literals, ExprType.Or)
	{
	}

	public bool Equals(CnfClause<T_Identifier> other)
	{
		return other?.Literals.SetEquals(base.Literals) ?? false;
	}
}
