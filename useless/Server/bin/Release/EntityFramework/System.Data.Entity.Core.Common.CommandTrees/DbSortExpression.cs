using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Utilities;

namespace System.Data.Entity.Core.Common.CommandTrees;

public sealed class DbSortExpression : DbExpression
{
	private readonly DbExpressionBinding _input;

	private readonly ReadOnlyCollection<DbSortClause> _keys;

	public DbExpressionBinding Input => _input;

	public IList<DbSortClause> SortOrder => _keys;

	internal DbSortExpression(TypeUsage resultType, DbExpressionBinding input, ReadOnlyCollection<DbSortClause> sortOrder)
		: base(DbExpressionKind.Sort, resultType)
	{
		_input = input;
		_keys = sortOrder;
	}

	public override void Accept(DbExpressionVisitor visitor)
	{
		Check.NotNull(visitor, "visitor");
		visitor.Visit(this);
	}

	public override TResultType Accept<TResultType>(DbExpressionVisitor<TResultType> visitor)
	{
		Check.NotNull(visitor, "visitor");
		return visitor.Visit(this);
	}
}
