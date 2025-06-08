using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Utilities;

namespace System.Data.Entity.Core.Common.CommandTrees;

public sealed class DbElementExpression : DbUnaryExpression
{
	private readonly bool _singlePropertyUnwrapped;

	internal bool IsSinglePropertyUnwrapped => _singlePropertyUnwrapped;

	internal DbElementExpression(TypeUsage resultType, DbExpression argument)
		: base(DbExpressionKind.Element, resultType, argument)
	{
		_singlePropertyUnwrapped = false;
	}

	internal DbElementExpression(TypeUsage resultType, DbExpression argument, bool unwrapSingleProperty)
		: base(DbExpressionKind.Element, resultType, argument)
	{
		_singlePropertyUnwrapped = unwrapSingleProperty;
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
