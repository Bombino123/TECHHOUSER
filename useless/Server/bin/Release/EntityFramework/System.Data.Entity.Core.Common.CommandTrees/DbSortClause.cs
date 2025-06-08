namespace System.Data.Entity.Core.Common.CommandTrees;

public sealed class DbSortClause
{
	private readonly DbExpression _expr;

	private readonly bool _asc;

	private readonly string _coll;

	public bool Ascending => _asc;

	public string Collation => _coll;

	public DbExpression Expression => _expr;

	internal DbSortClause(DbExpression key, bool asc, string collation)
	{
		_expr = key;
		_asc = asc;
		_coll = collation;
	}
}
