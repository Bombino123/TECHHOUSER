using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Utilities;

namespace System.Data.Entity.Core.Common.CommandTrees;

public sealed class DbRefExpression : DbUnaryExpression
{
	private readonly EntitySet _entitySet;

	public EntitySet EntitySet => _entitySet;

	internal DbRefExpression(TypeUsage refResultType, EntitySet entitySet, DbExpression refKeys)
		: base(DbExpressionKind.Ref, refResultType, refKeys)
	{
		_entitySet = entitySet;
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
