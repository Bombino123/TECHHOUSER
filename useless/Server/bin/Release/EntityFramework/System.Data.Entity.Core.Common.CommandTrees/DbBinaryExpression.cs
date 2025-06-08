using System.Data.Entity.Core.Metadata.Edm;

namespace System.Data.Entity.Core.Common.CommandTrees;

public abstract class DbBinaryExpression : DbExpression
{
	private readonly DbExpression _left;

	private readonly DbExpression _right;

	public virtual DbExpression Left => _left;

	public virtual DbExpression Right => _right;

	internal DbBinaryExpression()
	{
	}

	internal DbBinaryExpression(DbExpressionKind kind, TypeUsage type, DbExpression left, DbExpression right)
		: base(kind, type)
	{
		_left = left;
		_right = right;
	}
}
