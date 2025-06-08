namespace System.Data.Entity.Core.Common.Utils.Boolean;

internal sealed class DnfClause<T_Identifier> : Clause<T_Identifier>, IEquatable<DnfClause<T_Identifier>>
{
	internal DnfClause(Set<Literal<T_Identifier>> literals)
		: base(literals, ExprType.And)
	{
	}

	public bool Equals(DnfClause<T_Identifier> other)
	{
		return other?.Literals.SetEquals(base.Literals) ?? false;
	}
}
