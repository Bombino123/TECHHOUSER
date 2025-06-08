using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System.Data.Entity.Core.Common.Utils.Boolean;

internal abstract class Sentence<T_Identifier, T_Clause> : NormalFormNode<T_Identifier> where T_Clause : Clause<T_Identifier>, IEquatable<T_Clause>
{
	private readonly Set<T_Clause> _clauses;

	protected Sentence(Set<T_Clause> clauses, ExprType treeType)
		: base(ConvertClausesToExpr(clauses, treeType))
	{
		_clauses = clauses.AsReadOnly();
	}

	private static BoolExpr<T_Identifier> ConvertClausesToExpr(Set<T_Clause> clauses, ExprType treeType)
	{
		bool num = treeType == ExprType.And;
		IEnumerable<BoolExpr<T_Identifier>> children = clauses.Select(NormalFormNode<T_Identifier>.ExprSelector);
		if (num)
		{
			return new AndExpr<T_Identifier>(children);
		}
		return new OrExpr<T_Identifier>(children);
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("Sentence{");
		stringBuilder.Append(_clauses);
		return stringBuilder.Append("}").ToString();
	}
}
