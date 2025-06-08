using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Utilities;

namespace System.Data.Entity.Core.Common.CommandTrees;

public class DbParameterReferenceExpression : DbExpression
{
	private readonly string _name;

	public virtual string ParameterName => _name;

	internal DbParameterReferenceExpression()
	{
	}

	internal DbParameterReferenceExpression(TypeUsage type, string name)
		: base(DbExpressionKind.ParameterReference, type, forceNullable: false)
	{
		_name = name;
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
