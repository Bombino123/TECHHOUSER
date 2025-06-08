using System.ComponentModel;
using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Resources;
using System.Data.Entity.Spatial;
using System.Globalization;

namespace System.Data.Entity.Core.Common.CommandTrees;

public abstract class DbExpression
{
	private readonly TypeUsage _type;

	private readonly DbExpressionKind _kind;

	public virtual TypeUsage ResultType => _type;

	public virtual DbExpressionKind ExpressionKind => _kind;

	internal DbExpression()
	{
	}

	internal DbExpression(DbExpressionKind kind, TypeUsage type, bool forceNullable = true)
	{
		CheckExpressionKind(kind);
		_kind = kind;
		if (forceNullable && !TypeSemantics.IsNullable(type))
		{
			type = type.ShallowCopy(new FacetValues
			{
				Nullable = true
			});
		}
		_type = type;
	}

	public abstract void Accept(DbExpressionVisitor visitor);

	public abstract TResultType Accept<TResultType>(DbExpressionVisitor<TResultType> visitor);

	[EditorBrowsable(EditorBrowsableState.Never)]
	public override bool Equals(object obj)
	{
		return base.Equals(obj);
	}

	public override int GetHashCode()
	{
		return base.GetHashCode();
	}

	public static DbExpression FromBinary(byte[] value)
	{
		if (value == null)
		{
			return DbExpressionBuilder.CreatePrimitiveNullExpression(PrimitiveTypeKind.Binary);
		}
		return DbExpressionBuilder.Constant(value);
	}

	public static implicit operator DbExpression(byte[] value)
	{
		return FromBinary(value);
	}

	public static DbExpression FromBoolean(bool? value)
	{
		if (!value.HasValue)
		{
			return DbExpressionBuilder.CreatePrimitiveNullExpression(PrimitiveTypeKind.Boolean);
		}
		if (!value.Value)
		{
			return DbExpressionBuilder.False;
		}
		return DbExpressionBuilder.True;
	}

	public static implicit operator DbExpression(bool? value)
	{
		return FromBoolean(value);
	}

	public static DbExpression FromByte(byte? value)
	{
		if (!value.HasValue)
		{
			return DbExpressionBuilder.CreatePrimitiveNullExpression(PrimitiveTypeKind.Byte);
		}
		return DbExpressionBuilder.Constant(value.Value);
	}

	public static implicit operator DbExpression(byte? value)
	{
		return FromByte(value);
	}

	public static DbExpression FromDateTime(DateTime? value)
	{
		if (!value.HasValue)
		{
			return DbExpressionBuilder.CreatePrimitiveNullExpression(PrimitiveTypeKind.DateTime);
		}
		return DbExpressionBuilder.Constant(value.Value);
	}

	public static implicit operator DbExpression(DateTime? value)
	{
		return FromDateTime(value);
	}

	public static DbExpression FromDateTimeOffset(DateTimeOffset? value)
	{
		if (!value.HasValue)
		{
			return DbExpressionBuilder.CreatePrimitiveNullExpression(PrimitiveTypeKind.DateTimeOffset);
		}
		return DbExpressionBuilder.Constant(value.Value);
	}

	public static implicit operator DbExpression(DateTimeOffset? value)
	{
		return FromDateTimeOffset(value);
	}

	public static DbExpression FromDecimal(decimal? value)
	{
		if (!value.HasValue)
		{
			return DbExpressionBuilder.CreatePrimitiveNullExpression(PrimitiveTypeKind.Decimal);
		}
		return DbExpressionBuilder.Constant(value.Value);
	}

	public static implicit operator DbExpression(decimal? value)
	{
		return FromDecimal(value);
	}

	public static DbExpression FromDouble(double? value)
	{
		if (!value.HasValue)
		{
			return DbExpressionBuilder.CreatePrimitiveNullExpression(PrimitiveTypeKind.Double);
		}
		return DbExpressionBuilder.Constant(value.Value);
	}

	public static implicit operator DbExpression(double? value)
	{
		return FromDouble(value);
	}

	public static DbExpression FromGeography(DbGeography value)
	{
		if (value == null)
		{
			return DbExpressionBuilder.CreatePrimitiveNullExpression(PrimitiveTypeKind.Geography);
		}
		return DbExpressionBuilder.Constant(value);
	}

	public static implicit operator DbExpression(DbGeography value)
	{
		return FromGeography(value);
	}

	public static DbExpression FromGeometry(DbGeometry value)
	{
		if (value == null)
		{
			return DbExpressionBuilder.CreatePrimitiveNullExpression(PrimitiveTypeKind.Geometry);
		}
		return DbExpressionBuilder.Constant(value);
	}

	public static implicit operator DbExpression(DbGeometry value)
	{
		return FromGeometry(value);
	}

	public static DbExpression FromGuid(Guid? value)
	{
		if (!value.HasValue)
		{
			return DbExpressionBuilder.CreatePrimitiveNullExpression(PrimitiveTypeKind.Guid);
		}
		return DbExpressionBuilder.Constant(value.Value);
	}

	public static implicit operator DbExpression(Guid? value)
	{
		return FromGuid(value);
	}

	public static DbExpression FromInt16(short? value)
	{
		if (!value.HasValue)
		{
			return DbExpressionBuilder.CreatePrimitiveNullExpression(PrimitiveTypeKind.Int16);
		}
		return DbExpressionBuilder.Constant(value.Value);
	}

	public static implicit operator DbExpression(short? value)
	{
		return FromInt16(value);
	}

	public static DbExpression FromInt32(int? value)
	{
		if (!value.HasValue)
		{
			return DbExpressionBuilder.CreatePrimitiveNullExpression(PrimitiveTypeKind.Int32);
		}
		return DbExpressionBuilder.Constant(value.Value);
	}

	public static implicit operator DbExpression(int? value)
	{
		return FromInt32(value);
	}

	public static DbExpression FromInt64(long? value)
	{
		if (!value.HasValue)
		{
			return DbExpressionBuilder.CreatePrimitiveNullExpression(PrimitiveTypeKind.Int64);
		}
		return DbExpressionBuilder.Constant(value.Value);
	}

	public static implicit operator DbExpression(long? value)
	{
		return FromInt64(value);
	}

	public static DbExpression FromSingle(float? value)
	{
		if (!value.HasValue)
		{
			return DbExpressionBuilder.CreatePrimitiveNullExpression(PrimitiveTypeKind.Single);
		}
		return DbExpressionBuilder.Constant(value.Value);
	}

	public static implicit operator DbExpression(float? value)
	{
		return FromSingle(value);
	}

	public static DbExpression FromString(string value)
	{
		if (value == null)
		{
			return DbExpressionBuilder.CreatePrimitiveNullExpression(PrimitiveTypeKind.String);
		}
		return DbExpressionBuilder.Constant(value);
	}

	public static implicit operator DbExpression(string value)
	{
		return FromString(value);
	}

	internal static void CheckExpressionKind(DbExpressionKind kind)
	{
		if (kind < DbExpressionKind.All || DbExpressionKindHelper.Last < kind)
		{
			string name = typeof(DbExpressionKind).Name;
			int num = (int)kind;
			throw new ArgumentOutOfRangeException(name, Strings.ADP_InvalidEnumerationValue(name, num.ToString(CultureInfo.InvariantCulture)));
		}
	}
}
