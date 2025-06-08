using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Utilities;

namespace System.Data.Entity.Core.Common.CommandTrees;

public sealed class DbApplyExpression : DbExpression
{
	private readonly DbExpressionBinding _input;

	private readonly DbExpressionBinding _apply;

	public DbExpressionBinding Apply => _apply;

	public DbExpressionBinding Input => _input;

	internal DbApplyExpression(DbExpressionKind applyKind, TypeUsage resultRowCollectionTypeUsage, DbExpressionBinding input, DbExpressionBinding apply)
		: base(applyKind, resultRowCollectionTypeUsage)
	{
		_input = input;
		_apply = apply;
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
