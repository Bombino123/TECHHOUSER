using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System.Data.Entity.Core.Common.Utils.Boolean;

internal abstract class Clause<T_Identifier> : NormalFormNode<T_Identifier>
{
	private readonly Set<Literal<T_Identifier>> _literals;

	private readonly int _hashCode;

	internal Set<Literal<T_Identifier>> Literals => _literals;

	protected Clause(Set<Literal<T_Identifier>> literals, ExprType treeType)
		: base(ConvertLiteralsToExpr(literals, treeType))
	{
		_literals = literals.AsReadOnly();
		_hashCode = _literals.GetElementsHashCode();
	}

	private static BoolExpr<T_Identifier> ConvertLiteralsToExpr(Set<Literal<T_Identifier>> literals, ExprType treeType)
	{
		bool num = treeType == ExprType.And;
		IEnumerable<BoolExpr<T_Identifier>> children = literals.Select(ConvertLiteralToExpression);
		if (num)
		{
			return new AndExpr<T_Identifier>(children);
		}
		return new OrExpr<T_Identifier>(children);
	}

	private static BoolExpr<T_Identifier> ConvertLiteralToExpression(Literal<T_Identifier> literal)
	{
		return literal.Expr;
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("Clause{");
		stringBuilder.Append(_literals);
		return stringBuilder.Append("}").ToString();
	}

	public override int GetHashCode()
	{
		return _hashCode;
	}

	public override bool Equals(object obj)
	{
		return base.Equals(obj);
	}
}
