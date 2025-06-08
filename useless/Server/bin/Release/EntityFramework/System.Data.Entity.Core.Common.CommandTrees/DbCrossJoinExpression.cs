using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Utilities;

namespace System.Data.Entity.Core.Common.CommandTrees;

public sealed class DbCrossJoinExpression : DbExpression
{
	private readonly ReadOnlyCollection<DbExpressionBinding> _inputs;

	public IList<DbExpressionBinding> Inputs => _inputs;

	internal DbCrossJoinExpression(TypeUsage collectionOfRowResultType, ReadOnlyCollection<DbExpressionBinding> inputs)
		: base(DbExpressionKind.CrossJoin, collectionOfRowResultType)
	{
		_inputs = inputs;
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
