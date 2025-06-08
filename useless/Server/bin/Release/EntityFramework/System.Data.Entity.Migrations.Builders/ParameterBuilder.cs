using System.ComponentModel;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Migrations.Model;
using System.Data.Entity.Spatial;

namespace System.Data.Entity.Migrations.Builders;

public class ParameterBuilder
{
	public ParameterModel Binary(int? maxLength = null, bool? fixedLength = null, byte[] defaultValue = null, string defaultValueSql = null, string name = null, string storeType = null, bool outParameter = false)
	{
		return BuildParameter(PrimitiveTypeKind.Binary, defaultValue, defaultValueSql, maxLength, null, null, null, fixedLength, name, storeType, outParameter);
	}

	public ParameterModel Boolean(bool? defaultValue = null, string defaultValueSql = null, string name = null, string storeType = null, bool outParameter = false)
	{
		return BuildParameter(PrimitiveTypeKind.Boolean, defaultValue, defaultValueSql, null, null, null, null, null, name, storeType, outParameter);
	}

	public ParameterModel Byte(byte? defaultValue = null, string defaultValueSql = null, string name = null, string storeType = null, bool outParameter = false)
	{
		return BuildParameter(PrimitiveTypeKind.Byte, defaultValue, defaultValueSql, null, null, null, null, null, name, storeType, outParameter);
	}

	public ParameterModel DateTime(byte? precision = null, DateTime? defaultValue = null, string defaultValueSql = null, string name = null, string storeType = null, bool outParameter = false)
	{
		return BuildParameter(PrimitiveTypeKind.DateTime, defaultValue, defaultValueSql, null, precision, null, null, null, name, storeType, outParameter);
	}

	public ParameterModel Decimal(byte? precision = null, byte? scale = null, decimal? defaultValue = null, string defaultValueSql = null, string name = null, string storeType = null, bool outParameter = false)
	{
		return BuildParameter(PrimitiveTypeKind.Decimal, defaultValue, defaultValueSql, null, precision, scale, null, null, name, storeType, outParameter);
	}

	public ParameterModel Double(double? defaultValue = null, string defaultValueSql = null, string name = null, string storeType = null, bool outParameter = false)
	{
		return BuildParameter(PrimitiveTypeKind.Double, defaultValue, defaultValueSql, null, null, null, null, null, name, storeType, outParameter);
	}

	public ParameterModel Guid(Guid? defaultValue = null, string defaultValueSql = null, string name = null, string storeType = null, bool outParameter = false)
	{
		return BuildParameter(PrimitiveTypeKind.Guid, defaultValue, defaultValueSql, null, null, null, null, null, name, storeType, outParameter);
	}

	public ParameterModel Single(float? defaultValue = null, string defaultValueSql = null, string name = null, string storeType = null, bool outParameter = false)
	{
		return BuildParameter(PrimitiveTypeKind.Single, defaultValue, defaultValueSql, null, null, null, null, null, name, storeType, outParameter);
	}

	public ParameterModel Short(short? defaultValue = null, string defaultValueSql = null, string name = null, string storeType = null, bool outParameter = false)
	{
		return BuildParameter(PrimitiveTypeKind.Int16, defaultValue, defaultValueSql, null, null, null, null, null, name, storeType, outParameter);
	}

	public ParameterModel Int(int? defaultValue = null, string defaultValueSql = null, string name = null, string storeType = null, bool outParameter = false)
	{
		return BuildParameter(PrimitiveTypeKind.Int32, defaultValue, defaultValueSql, null, null, null, null, null, name, storeType, outParameter);
	}

	public ParameterModel Long(long? defaultValue = null, string defaultValueSql = null, string name = null, string storeType = null, bool outParameter = false)
	{
		return BuildParameter(PrimitiveTypeKind.Int64, defaultValue, defaultValueSql, null, null, null, null, null, name, storeType, outParameter);
	}

	public ParameterModel String(int? maxLength = null, bool? fixedLength = null, bool? unicode = null, string defaultValue = null, string defaultValueSql = null, string name = null, string storeType = null, bool outParameter = false)
	{
		return BuildParameter(PrimitiveTypeKind.String, defaultValue, defaultValueSql, maxLength, null, null, unicode, fixedLength, name, storeType, outParameter);
	}

	public ParameterModel Time(byte? precision = null, TimeSpan? defaultValue = null, string defaultValueSql = null, string name = null, string storeType = null, bool outParameter = false)
	{
		return BuildParameter(PrimitiveTypeKind.Time, defaultValue, defaultValueSql, null, precision, null, null, null, name, storeType, outParameter);
	}

	public ParameterModel DateTimeOffset(byte? precision = null, DateTimeOffset? defaultValue = null, string defaultValueSql = null, string name = null, string storeType = null, bool outParameter = false)
	{
		return BuildParameter(PrimitiveTypeKind.DateTimeOffset, defaultValue, defaultValueSql, null, precision, null, null, null, name, storeType, outParameter);
	}

	public ParameterModel Geography(DbGeography defaultValue = null, string defaultValueSql = null, string name = null, string storeType = null, bool outParameter = false)
	{
		return BuildParameter(PrimitiveTypeKind.Geography, defaultValue, defaultValueSql, null, null, null, null, null, name, storeType, outParameter);
	}

	public ParameterModel Geometry(DbGeometry defaultValue = null, string defaultValueSql = null, string name = null, string storeType = null, bool outParameter = false)
	{
		return BuildParameter(PrimitiveTypeKind.Geometry, defaultValue, defaultValueSql, null, null, null, null, null, name, storeType, outParameter);
	}

	private static ParameterModel BuildParameter(PrimitiveTypeKind primitiveTypeKind, object defaultValue, string defaultValueSql = null, int? maxLength = null, byte? precision = null, byte? scale = null, bool? unicode = null, bool? fixedLength = null, string name = null, string storeType = null, bool outParameter = false)
	{
		return new ParameterModel(primitiveTypeKind)
		{
			MaxLength = maxLength,
			Precision = precision,
			Scale = scale,
			IsUnicode = unicode,
			IsFixedLength = fixedLength,
			DefaultValue = defaultValue,
			DefaultValueSql = defaultValueSql,
			Name = name,
			StoreType = storeType,
			IsOutParameter = outParameter
		};
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public override string ToString()
	{
		return base.ToString();
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public override bool Equals(object obj)
	{
		return base.Equals(obj);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public override int GetHashCode()
	{
		return base.GetHashCode();
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public new Type GetType()
	{
		return base.GetType();
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected new object MemberwiseClone()
	{
		return base.MemberwiseClone();
	}
}
