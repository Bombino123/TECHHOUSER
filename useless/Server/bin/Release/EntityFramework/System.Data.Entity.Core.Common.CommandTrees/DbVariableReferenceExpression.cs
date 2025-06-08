using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Utilities;

namespace System.Data.Entity.Core.Common.CommandTrees;

public class DbVariableReferenceExpression : DbExpression
{
	private readonly string _name;

	public virtual string VariableName => _name;

	internal DbVariableReferenceExpression()
	{
	}

	internal DbVariableReferenceExpression(TypeUsage type, string name)
		: base(DbExpressionKind.VariableReference, type)
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
