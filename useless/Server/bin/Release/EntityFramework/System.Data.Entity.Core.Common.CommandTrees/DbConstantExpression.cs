using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Utilities;

namespace System.Data.Entity.Core.Common.CommandTrees;

public class DbConstantExpression : DbExpression
{
	private readonly bool _shouldCloneValue;

	private readonly object _value;

	public virtual object Value
	{
		get
		{
			if (_shouldCloneValue)
			{
				return ((byte[])_value).Clone();
			}
			return _value;
		}
	}

	internal DbConstantExpression()
	{
	}

	internal DbConstantExpression(TypeUsage resultType, object value)
		: base(DbExpressionKind.Constant, resultType)
	{
		_shouldCloneValue = TypeHelpers.TryGetEdmType<PrimitiveType>(resultType, out var type) && type.PrimitiveTypeKind == PrimitiveTypeKind.Binary;
		if (_shouldCloneValue)
		{
			_value = ((byte[])value).Clone();
		}
		else
		{
			_value = value;
		}
	}

	internal object GetValue()
	{
		return _value;
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
