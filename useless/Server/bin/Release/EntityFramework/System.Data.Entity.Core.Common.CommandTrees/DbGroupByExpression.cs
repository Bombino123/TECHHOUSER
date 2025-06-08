using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity.Core.Common.CommandTrees.Internal;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Utilities;

namespace System.Data.Entity.Core.Common.CommandTrees;

public sealed class DbGroupByExpression : DbExpression
{
	private readonly DbGroupExpressionBinding _input;

	private readonly DbExpressionList _keys;

	private readonly ReadOnlyCollection<DbAggregate> _aggregates;

	public DbGroupExpressionBinding Input => _input;

	public IList<DbExpression> Keys => _keys;

	public IList<DbAggregate> Aggregates => _aggregates;

	internal DbGroupByExpression(TypeUsage collectionOfRowResultType, DbGroupExpressionBinding input, DbExpressionList groupKeys, ReadOnlyCollection<DbAggregate> aggregates)
		: base(DbExpressionKind.GroupBy, collectionOfRowResultType)
	{
		_input = input;
		_keys = groupKeys;
		_aggregates = aggregates;
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
