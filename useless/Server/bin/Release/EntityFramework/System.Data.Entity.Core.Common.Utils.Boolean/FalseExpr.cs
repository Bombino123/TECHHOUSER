namespace System.Data.Entity.Core.Common.Utils.Boolean;

internal sealed class FalseExpr<T_Identifier> : BoolExpr<T_Identifier>
{
	private static readonly FalseExpr<T_Identifier> _value = new FalseExpr<T_Identifier>();

	internal static FalseExpr<T_Identifier> Value => _value;

	internal override ExprType ExprType => ExprType.False;

	private FalseExpr()
	{
	}

	internal override T_Return Accept<T_Return>(Visitor<T_Identifier, T_Return> visitor)
	{
		return visitor.VisitFalse(this);
	}

	internal override BoolExpr<T_Identifier> MakeNegated()
	{
		return TrueExpr<T_Identifier>.Value;
	}

	protected override bool EquivalentTypeEquals(BoolExpr<T_Identifier> other)
	{
		return this == other;
	}
}
