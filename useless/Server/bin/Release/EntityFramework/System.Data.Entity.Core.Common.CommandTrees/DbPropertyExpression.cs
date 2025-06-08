using System.Collections.Generic;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Utilities;

namespace System.Data.Entity.Core.Common.CommandTrees;

public class DbPropertyExpression : DbExpression
{
	private readonly EdmMember _property;

	private readonly DbExpression _instance;

	public virtual EdmMember Property => _property;

	public virtual DbExpression Instance => _instance;

	internal DbPropertyExpression()
	{
	}

	internal DbPropertyExpression(TypeUsage resultType, EdmMember property, DbExpression instance)
		: base(DbExpressionKind.Property, resultType)
	{
		_property = property;
		_instance = instance;
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

	public KeyValuePair<string, DbExpression> ToKeyValuePair()
	{
		return new KeyValuePair<string, DbExpression>(Property.Name, this);
	}

	public static implicit operator KeyValuePair<string, DbExpression>(DbPropertyExpression value)
	{
		Check.NotNull(value, "value");
		return value.ToKeyValuePair();
	}
}
