using System.Collections.Generic;
using System.Data.Entity.Core.Common.CommandTrees.Internal;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Utilities;

namespace System.Data.Entity.Core.Common.CommandTrees;

public class DbInExpression : DbExpression
{
	private readonly DbExpression _item;

	private readonly DbExpressionList _list;

	public DbExpression Item => _item;

	public IList<DbExpression> List => _list;

	internal DbInExpression(TypeUsage booleanResultType, DbExpression item, DbExpressionList list)
		: base(DbExpressionKind.In, booleanResultType)
	{
		_item = item;
		_list = list;
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
