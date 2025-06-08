using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Utilities;

namespace System.Data.Entity.Core.Common.CommandTrees;

public sealed class DbProjectExpression : DbExpression
{
	private readonly DbExpressionBinding _input;

	private readonly DbExpression _projection;

	public DbExpressionBinding Input => _input;

	public DbExpression Projection => _projection;

	internal DbProjectExpression(TypeUsage resultType, DbExpressionBinding input, DbExpression projection)
		: base(DbExpressionKind.Project, resultType)
	{
		_input = input;
		_projection = projection;
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
