using System.Data.Entity.Core.Metadata.Edm;

namespace System.Data.Entity.Core.Common.CommandTrees;

public abstract class DbUnaryExpression : DbExpression
{
	private readonly DbExpression _argument;

	public virtual DbExpression Argument => _argument;

	internal DbUnaryExpression()
	{
	}

	internal DbUnaryExpression(DbExpressionKind kind, TypeUsage resultType, DbExpression argument)
		: base(kind, resultType)
	{
		_argument = argument;
	}
}
