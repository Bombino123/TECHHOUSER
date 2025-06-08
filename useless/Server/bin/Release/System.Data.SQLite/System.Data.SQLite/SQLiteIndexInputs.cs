namespace System.Data.SQLite;

public sealed class SQLiteIndexInputs
{
	private SQLiteIndexConstraint[] constraints;

	private SQLiteIndexOrderBy[] orderBys;

	public SQLiteIndexConstraint[] Constraints => constraints;

	public SQLiteIndexOrderBy[] OrderBys => orderBys;

	internal SQLiteIndexInputs(int nConstraint, int nOrderBy)
	{
		constraints = new SQLiteIndexConstraint[nConstraint];
		orderBys = new SQLiteIndexOrderBy[nOrderBy];
	}
}
