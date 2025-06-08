using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Utilities;

namespace System.Data.Entity.Core.Common.CommandTrees;

public sealed class DbIsOfExpression : DbUnaryExpression
{
	private readonly TypeUsage _ofType;

	public TypeUsage OfType => _ofType;

	internal DbIsOfExpression(DbExpressionKind isOfKind, TypeUsage booleanResultType, DbExpression argument, TypeUsage isOfType)
		: base(isOfKind, booleanResultType, argument)
	{
		_ofType = isOfType;
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
