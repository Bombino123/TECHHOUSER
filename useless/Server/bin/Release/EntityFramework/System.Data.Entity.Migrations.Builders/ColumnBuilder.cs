using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Hierarchy;
using System.Data.Entity.Infrastructure.Annotations;
using System.Data.Entity.Migrations.Model;
using System.Data.Entity.Spatial;

namespace System.Data.Entity.Migrations.Builders;

public class ColumnBuilder
{
	public ColumnModel Binary(bool? nullable = null, int? maxLength = null, bool? fixedLength = null, byte[] defaultValue = null, string defaultValueSql = null, bool timestamp = false, string name = null, string storeType = null, IDictionary<string, AnnotationValues> annotations = null)
	{
		return BuildColumn(PrimitiveTypeKind.Binary, nullable, defaultValue, defaultValueSql, maxLength, null, null, null, fixedLength, identity: false, timestamp, name, storeType, annotations);
	}

	public ColumnModel Boolean(bool? nullable = null, bool? defaultValue = null, string defaultValueSql = null, string name = null, string storeType = null, IDictionary<string, AnnotationValues> annotations = null)
	{
		return BuildColumn(PrimitiveTypeKind.Boolean, nullable, defaultValue, defaultValueSql, null, null, null, null, null, identity: false, timestamp: false, name, storeType, annotations);
	}

	public ColumnModel Byte(bool? nullable = null, bool identity = false, byte? defaultValue = null, string defaultValueSql = null, string name = null, string storeType = null, IDictionary<string, AnnotationValues> annotations = null)
	{
		return BuildColumn(PrimitiveTypeKind.Byte, nullable, defaultValue, defaultValueSql, null, null, null, null, null, identity, timestamp: false, name, storeType, annotations);
	}

	public ColumnModel DateTime(bool? nullable = null, byte? precision = null, DateTime? defaultValue = null, string defaultValueSql = null, string name = null, string storeType = null, IDictionary<string, AnnotationValues> annotations = null)
	{
		return BuildColumn(PrimitiveTypeKind.DateTime, nullable, defaultValue, defaultValueSql, null, precision, null, null, null, identity: false, timestamp: false, name, storeType, annotations);
	}

	public ColumnModel Decimal(bool? nullable = null, byte? precision = null, byte? scale = null, decimal? defaultValue = null, string defaultValueSql = null, string name = null, string storeType = null, bool identity = false, IDictionary<string, AnnotationValues> annotations = null)
	{
		return BuildColumn(PrimitiveTypeKind.Decimal, nullable, defaultValue, defaultValueSql, null, precision, scale, null, null, identity, timestamp: false, name, storeType, annotations);
	}

	public ColumnModel Double(bool? nullable = null, double? defaultValue = null, string defaultValueSql = null, string name = null, string storeType = null, IDictionary<string, AnnotationValues> annotations = null)
	{
		return BuildColumn(PrimitiveTypeKind.Double, nullable, defaultValue, defaultValueSql, null, null, null, null, null, identity: false, timestamp: false, name, storeType, annotations);
	}

	public ColumnModel Guid(bool? nullable = null, bool identity = false, Guid? defaultValue = null, string defaultValueSql = null, string name = null, string storeType = null, IDictionary<string, AnnotationValues> annotations = null)
	{
		return BuildColumn(PrimitiveTypeKind.Guid, nullable, defaultValue, defaultValueSql, null, null, null, null, null, identity, timestamp: false, name, storeType, annotations);
	}

	public ColumnModel Single(bool? nullable = null, float? defaultValue = null, string defaultValueSql = null, string name = null, string storeType = null, IDictionary<string, AnnotationValues> annotations = null)
	{
		return BuildColumn(PrimitiveTypeKind.Single, nullable, defaultValue, defaultValueSql, null, null, null, null, null, identity: false, timestamp: false, name, storeType, annotations);
	}

	public ColumnModel Short(bool? nullable = null, bool identity = false, short? defaultValue = null, string defaultValueSql = null, string name = null, string storeType = null, IDictionary<string, AnnotationValues> annotations = null)
	{
		return BuildColumn(PrimitiveTypeKind.Int16, nullable, defaultValue, defaultValueSql, null, null, null, null, null, identity, timestamp: false, name, storeType, annotations);
	}

	public ColumnModel Int(bool? nullable = null, bool identity = false, int? defaultValue = null, string defaultValueSql = null, string name = null, string storeType = null, IDictionary<string, AnnotationValues> annotations = null)
	{
		return BuildColumn(PrimitiveTypeKind.Int32, nullable, defaultValue, defaultValueSql, null, null, null, null, null, identity, timestamp: false, name, storeType, annotations);
	}

	public ColumnModel Long(bool? nullable = null, bool identity = false, long? defaultValue = null, string defaultValueSql = null, string name = null, string storeType = null, IDictionary<string, AnnotationValues> annotations = null)
	{
		return BuildColumn(PrimitiveTypeKind.Int64, nullable, defaultValue, defaultValueSql, null, null, null, null, null, identity, timestamp: false, name, storeType, annotations);
	}

	public ColumnModel String(bool? nullable = null, int? maxLength = null, bool? fixedLength = null, bool? unicode = null, string defaultValue = null, string defaultValueSql = null, string name = null, string storeType = null, IDictionary<string, AnnotationValues> annotations = null)
	{
		return BuildColumn(PrimitiveTypeKind.String, nullable, defaultValue, defaultValueSql, maxLength, null, null, unicode, fixedLength, identity: false, timestamp: false, name, storeType, annotations);
	}

	public ColumnModel Time(bool? nullable = null, byte? precision = null, TimeSpan? defaultValue = null, string defaultValueSql = null, string name = null, string storeType = null, IDictionary<string, AnnotationValues> annotations = null)
	{
		return BuildColumn(PrimitiveTypeKind.Time, nullable, defaultValue, defaultValueSql, null, precision, null, null, null, identity: false, timestamp: false, name, storeType, annotations);
	}

	public ColumnModel DateTimeOffset(bool? nullable = null, byte? precision = null, DateTimeOffset? defaultValue = null, string defaultValueSql = null, string name = null, string storeType = null, IDictionary<string, AnnotationValues> annotations = null)
	{
		return BuildColumn(PrimitiveTypeKind.DateTimeOffset, nullable, defaultValue, defaultValueSql, null, precision, null, null, null, identity: false, timestamp: false, name, storeType, annotations);
	}

	public ColumnModel HierarchyId(bool? nullable = null, HierarchyId defaultValue = null, string defaultValueSql = null, string name = null, string storeType = null, IDictionary<string, AnnotationValues> annotations = null)
	{
		return BuildColumn(PrimitiveTypeKind.HierarchyId, nullable, defaultValue, defaultValueSql, null, null, null, null, null, identity: false, timestamp: false, name, storeType, annotations);
	}

	public ColumnModel Geography(bool? nullable = null, DbGeography defaultValue = null, string defaultValueSql = null, string name = null, string storeType = null, IDictionary<string, AnnotationValues> annotations = null)
	{
		return BuildColumn(PrimitiveTypeKind.Geography, nullable, defaultValue, defaultValueSql, null, null, null, null, null, identity: false, timestamp: false, name, storeType, annotations);
	}

	public ColumnModel Geometry(bool? nullable = null, DbGeometry defaultValue = null, string defaultValueSql = null, string name = null, string storeType = null, IDictionary<string, AnnotationValues> annotations = null)
	{
		return BuildColumn(PrimitiveTypeKind.Geometry, nullable, defaultValue, defaultValueSql, null, null, null, null, null, identity: false, timestamp: false, name, storeType, annotations);
	}

	private static ColumnModel BuildColumn(PrimitiveTypeKind primitiveTypeKind, bool? nullable, object defaultValue, string defaultValueSql = null, int? maxLength = null, byte? precision = null, byte? scale = null, bool? unicode = null, bool? fixedLength = null, bool identity = false, bool timestamp = false, string name = null, string storeType = null, IDictionary<string, AnnotationValues> annotations = null)
	{
		return new ColumnModel(primitiveTypeKind)
		{
			IsNullable = nullable,
			MaxLength = maxLength,
			Precision = precision,
			Scale = scale,
			IsUnicode = unicode,
			IsFixedLength = fixedLength,
			IsIdentity = identity,
			DefaultValue = defaultValue,
			DefaultValueSql = defaultValueSql,
			IsTimestamp = timestamp,
			Name = name,
			StoreType = storeType,
			Annotations = annotations
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
