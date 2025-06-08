namespace System.Data.Entity.Core.Query.InternalTrees;

internal class SortKey
{
	private readonly bool m_asc;

	private readonly string m_collation;

	internal Var Var { get; set; }

	internal bool AscendingSort => m_asc;

	internal string Collation => m_collation;

	internal SortKey(Var v, bool asc, string collation)
	{
		Var = v;
		m_asc = asc;
		m_collation = collation;
	}
}
