using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Utilities;

namespace System.Data.Entity.Core.Common.CommandTrees;

public sealed class DbOfTypeExpression : DbUnaryExpression
{
	private readonly TypeUsage _ofType;

	public TypeUsage OfType => _ofType;

	internal DbOfTypeExpression(DbExpressionKind ofTypeKind, TypeUsage collectionResultType, DbExpression argument, TypeUsage type)
		: base(ofTypeKind, collectionResultType, argument)
	{
		_ofType = type;
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
