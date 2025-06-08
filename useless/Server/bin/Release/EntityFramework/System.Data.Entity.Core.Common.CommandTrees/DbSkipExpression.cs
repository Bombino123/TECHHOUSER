using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Utilities;

namespace System.Data.Entity.Core.Common.CommandTrees;

public sealed class DbSkipExpression : DbExpression
{
	private readonly DbExpressionBinding _input;

	private readonly ReadOnlyCollection<DbSortClause> _keys;

	private readonly DbExpression _count;

	public DbExpressionBinding Input => _input;

	public IList<DbSortClause> SortOrder => _keys;

	public DbExpression Count => _count;

	internal DbSkipExpression(TypeUsage resultType, DbExpressionBinding input, ReadOnlyCollection<DbSortClause> sortOrder, DbExpression count)
		: base(DbExpressionKind.Skip, resultType)
	{
		_input = input;
		_keys = sortOrder;
		_count = count;
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
