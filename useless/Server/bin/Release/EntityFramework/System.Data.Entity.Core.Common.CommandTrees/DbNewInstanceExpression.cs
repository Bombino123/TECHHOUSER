using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity.Core.Common.CommandTrees.Internal;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Utilities;

namespace System.Data.Entity.Core.Common.CommandTrees;

public sealed class DbNewInstanceExpression : DbExpression
{
	private readonly DbExpressionList _elements;

	private readonly ReadOnlyCollection<DbRelatedEntityRef> _relatedEntityRefs;

	public IList<DbExpression> Arguments => _elements;

	internal bool HasRelatedEntityReferences => _relatedEntityRefs != null;

	internal ReadOnlyCollection<DbRelatedEntityRef> RelatedEntityReferences => _relatedEntityRefs;

	internal DbNewInstanceExpression(TypeUsage type, DbExpressionList args)
		: base(DbExpressionKind.NewInstance, type)
	{
		_elements = args;
	}

	internal DbNewInstanceExpression(TypeUsage resultType, DbExpressionList attributeValues, ReadOnlyCollection<DbRelatedEntityRef> relationships)
		: this(resultType, attributeValues)
	{
		_relatedEntityRefs = ((relationships.Count > 0) ? relationships : null);
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
