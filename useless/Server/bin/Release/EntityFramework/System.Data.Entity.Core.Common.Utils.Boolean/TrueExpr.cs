namespace System.Data.Entity.Core.Common.Utils.Boolean;

internal sealed class TrueExpr<T_Identifier> : BoolExpr<T_Identifier>
{
	private static readonly TrueExpr<T_Identifier> _value = new TrueExpr<T_Identifier>();

	internal static TrueExpr<T_Identifier> Value => _value;

	internal override ExprType ExprType => ExprType.True;

	private TrueExpr()
	{
	}

	internal override T_Return Accept<T_Return>(Visitor<T_Identifier, T_Return> visitor)
	{
		return visitor.VisitTrue(this);
	}

	internal override BoolExpr<T_Identifier> MakeNegated()
	{
		return FalseExpr<T_Identifier>.Value;
	}

	protected override bool EquivalentTypeEquals(BoolExpr<T_Identifier> other)
	{
		return this == other;
	}
}
