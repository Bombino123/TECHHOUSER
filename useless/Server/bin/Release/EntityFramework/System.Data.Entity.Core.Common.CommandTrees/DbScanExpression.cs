using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Utilities;

namespace System.Data.Entity.Core.Common.CommandTrees;

public class DbScanExpression : DbExpression
{
	private readonly EntitySetBase _targetSet;

	public virtual EntitySetBase Target => _targetSet;

	internal DbScanExpression()
	{
	}

	internal DbScanExpression(TypeUsage collectionOfEntityType, EntitySetBase entitySet)
		: base(DbExpressionKind.Scan, collectionOfEntityType)
	{
		_targetSet = entitySet;
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
