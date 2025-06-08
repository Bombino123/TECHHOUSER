using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Utilities;

namespace System.Data.Entity.Core.Common.CommandTrees;

public sealed class DbRelationshipNavigationExpression : DbExpression
{
	private readonly RelationshipType _relation;

	private readonly RelationshipEndMember _fromRole;

	private readonly RelationshipEndMember _toRole;

	private readonly DbExpression _from;

	public RelationshipType Relationship => _relation;

	public RelationshipEndMember NavigateFrom => _fromRole;

	public RelationshipEndMember NavigateTo => _toRole;

	public DbExpression NavigationSource => _from;

	internal DbRelationshipNavigationExpression(TypeUsage resultType, RelationshipType relType, RelationshipEndMember fromEnd, RelationshipEndMember toEnd, DbExpression navigateFrom)
		: base(DbExpressionKind.RelationshipNavigation, resultType)
	{
		_relation = relType;
		_fromRole = fromEnd;
		_toRole = toEnd;
		_from = navigateFrom;
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
