namespace System.Data.Entity.Core.Common.Utils.Boolean;

internal sealed class CnfSentence<T_Identifier> : Sentence<T_Identifier, CnfClause<T_Identifier>>
{
	internal CnfSentence(Set<CnfClause<T_Identifier>> clauses)
		: base(clauses, ExprType.And)
	{
	}
}
