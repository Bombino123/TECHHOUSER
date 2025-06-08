using System.ComponentModel;
using System.Data.Common;
using System.Data.Entity.Core.Common;
using System.Data.Entity.Core.Common.CommandTrees;
using System.Data.Entity.Core.Common.Internal;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Core.Metadata.Edm.Provider;
using System.Data.Entity.Resources;
using System.Globalization;

namespace System.Data.Entity.Core.EntityClient;

public class EntityParameter : DbParameter, IDbDataParameter, IDataParameter
{
	private string _parameterName;

	private DbType? _dbType;

	private EdmType _edmType;

	private byte? _precision;

	private byte? _scale;

	private bool _isDirty;

	private object _value;

	private object _parent;

	private ParameterDirection _direction;

	private int? _size;

	private string _sourceColumn;

	private DataRowVersion _sourceVersion;

	private bool _sourceColumnNullMapping;

	private bool? _isNullable;

	public override string ParameterName
	{
		get
		{
			return _parameterName ?? "";
		}
		set
		{
			SetParameterNameWithValidation(value, "value");
		}
	}

	public override DbType DbType
	{
		get
		{
			if (!_dbType.HasValue)
			{
				if (_edmType != null)
				{
					return GetDbTypeFromEdm(_edmType);
				}
				if (_value == null)
				{
					return DbType.String;
				}
				try
				{
					return TypeHelpers.ConvertClrTypeToDbType(_value.GetType());
				}
				catch (ArgumentException innerException)
				{
					throw new InvalidOperationException(Strings.EntityClient_CannotDeduceDbType, innerException);
				}
			}
			return _dbType.Value;
		}
		set
		{
			PropertyChanging();
			_dbType = value;
		}
	}

	public virtual EdmType EdmType
	{
		get
		{
			return _edmType;
		}
		set
		{
			if (value != null && !Helper.IsScalarType(value))
			{
				throw new InvalidOperationException(Strings.EntityClient_EntityParameterEdmTypeNotScalar(value.FullName));
			}
			PropertyChanging();
			_edmType = value;
		}
	}

	public new virtual byte Precision
	{
		get
		{
			if (!_precision.HasValue)
			{
				return 0;
			}
			return _precision.Value;
		}
		set
		{
			PropertyChanging();
			_precision = value;
		}
	}

	public new virtual byte Scale
	{
		get
		{
			if (!_scale.HasValue)
			{
				return 0;
			}
			return _scale.Value;
		}
		set
		{
			PropertyChanging();
			_scale = value;
		}
	}

	public override object Value
	{
		get
		{
			return _value;
		}
		set
		{
			if (!_dbType.HasValue && _edmType == null)
			{
				DbType dbType = DbType.String;
				if (_value != null)
				{
					dbType = TypeHelpers.ConvertClrTypeToDbType(_value.GetType());
				}
				DbType dbType2 = DbType.String;
				if (value != null)
				{
					dbType2 = TypeHelpers.ConvertClrTypeToDbType(value.GetType());
				}
				if (dbType != dbType2)
				{
					PropertyChanging();
				}
			}
			_value = value;
		}
	}

	internal virtual bool IsDirty => _isDirty;

	internal virtual bool IsDbTypeSpecified => _dbType.HasValue;

	internal virtual bool IsDirectionSpecified => _direction != (ParameterDirection)0;

	internal virtual bool IsIsNullableSpecified => _isNullable.HasValue;

	internal virtual bool IsPrecisionSpecified => _precision.HasValue;

	internal virtual bool IsScaleSpecified => _scale.HasValue;

	internal virtual bool IsSizeSpecified => _size.HasValue;

	[RefreshProperties(RefreshProperties.All)]
	[EntityResCategory("DataCategory_Data")]
	[EntityResDescription("DbParameter_Direction")]
	public override ParameterDirection Direction
	{
		get
		{
			ParameterDirection direction = _direction;
			if (direction == (ParameterDirection)0)
			{
				return ParameterDirection.Input;
			}
			return direction;
		}
		set
		{
			if (_direction != value)
			{
				if ((uint)(value - 1) > 2u && value != ParameterDirection.ReturnValue)
				{
					string name = typeof(ParameterDirection).Name;
					string name2 = typeof(ParameterDirection).Name;
					int num = (int)value;
					throw new ArgumentOutOfRangeException(name, Strings.ADP_InvalidEnumerationValue(name2, num.ToString(CultureInfo.InvariantCulture)));
				}
				PropertyChanging();
				_direction = value;
			}
		}
	}

	public override bool IsNullable
	{
		get
		{
			if (!_isNullable.HasValue)
			{
				return true;
			}
			return _isNullable.Value;
		}
		set
		{
			_isNullable = value;
		}
	}

	[EntityResCategory("DataCategory_Data")]
	[EntityResDescription("DbParameter_Size")]
	public override int Size
	{
		get
		{
			int num = (_size.HasValue ? _size.Value : 0);
			if (num == 0)
			{
				num = ValueSize(Value);
			}
			return num;
		}
		set
		{
			if (!_size.HasValue || _size.Value != value)
			{
				if (value < -1)
				{
					throw new ArgumentException(Strings.ADP_InvalidSizeValue(value.ToString(CultureInfo.InvariantCulture)));
				}
				PropertyChanging();
				if (value == 0)
				{
					_size = null;
				}
				else
				{
					_size = value;
				}
			}
		}
	}

	[EntityResCategory("DataCategory_Update")]
	[EntityResDescription("DbParameter_SourceColumn")]
	public override string SourceColumn
	{
		get
		{
			string sourceColumn = _sourceColumn;
			if (sourceColumn == null)
			{
				return string.Empty;
			}
			return sourceColumn;
		}
		set
		{
			_sourceColumn = value;
		}
	}

	public override bool SourceColumnNullMapping
	{
		get
		{
			return _sourceColumnNullMapping;
		}
		set
		{
			_sourceColumnNullMapping = value;
		}
	}

	[EntityResCategory("DataCategory_Update")]
	[EntityResDescription("DbParameter_SourceVersion")]
	public override DataRowVersion SourceVersion
	{
		get
		{
			DataRowVersion sourceVersion = _sourceVersion;
			if (sourceVersion == (DataRowVersion)0)
			{
				return DataRowVersion.Current;
			}
			return sourceVersion;
		}
		set
		{
			switch (value)
			{
			case DataRowVersion.Original:
			case DataRowVersion.Current:
			case DataRowVersion.Proposed:
			case DataRowVersion.Default:
				_sourceVersion = value;
				return;
			}
			string name = typeof(DataRowVersion).Name;
			string name2 = typeof(DataRowVersion).Name;
			int num = (int)value;
			throw new ArgumentOutOfRangeException(name, Strings.ADP_InvalidEnumerationValue(name2, num.ToString(CultureInfo.InvariantCulture)));
		}
	}

	private bool IsTypeConsistent
	{
		get
		{
			if (_edmType != null && _dbType.HasValue)
			{
				DbType dbTypeFromEdm = GetDbTypeFromEdm(_edmType);
				if (dbTypeFromEdm == DbType.String)
				{
					if (_dbType != DbType.String && _dbType != DbType.AnsiString && dbTypeFromEdm != DbType.AnsiStringFixedLength)
					{
						return dbTypeFromEdm == DbType.StringFixedLength;
					}
					return true;
				}
				return _dbType == dbTypeFromEdm;
			}
			return true;
		}
	}

	public EntityParameter()
	{
	}

	public EntityParameter(string parameterName, DbType dbType)
	{
		SetParameterNameWithValidation(parameterName, "parameterName");
		DbType = dbType;
	}

	public EntityParameter(string parameterName, DbType dbType, int size)
	{
		SetParameterNameWithValidation(parameterName, "parameterName");
		DbType = dbType;
		Size = size;
	}

	public EntityParameter(string parameterName, DbType dbType, int size, string sourceColumn)
	{
		SetParameterNameWithValidation(parameterName, "parameterName");
		DbType = dbType;
		Size = size;
		SourceColumn = sourceColumn;
	}

	public EntityParameter(string parameterName, DbType dbType, int size, ParameterDirection direction, bool isNullable, byte precision, byte scale, string sourceColumn, DataRowVersion sourceVersion, object value)
	{
		SetParameterNameWithValidation(parameterName, "parameterName");
		DbType = dbType;
		Size = size;
		Direction = direction;
		IsNullable = isNullable;
		Precision = precision;
		Scale = scale;
		SourceColumn = sourceColumn;
		SourceVersion = sourceVersion;
		Value = value;
	}

	private EntityParameter(EntityParameter source)
		: this()
	{
		source.CloneHelper(this);
		if (_value is ICloneable cloneable)
		{
			_value = cloneable.Clone();
		}
	}

	private void SetParameterNameWithValidation(string parameterName, string argumentName)
	{
		if (!string.IsNullOrEmpty(parameterName) && !DbCommandTree.IsValidParameterName(parameterName))
		{
			throw new ArgumentException(Strings.EntityClient_InvalidParameterName(parameterName), argumentName);
		}
		PropertyChanging();
		_parameterName = parameterName;
	}

	public override void ResetDbType()
	{
		if (_dbType.HasValue || _edmType != null)
		{
			PropertyChanging();
		}
		_edmType = null;
		_dbType = null;
	}

	private void PropertyChanging()
	{
		_isDirty = true;
	}

	private static int ValueSize(object value)
	{
		return ValueSizeCore(value);
	}

	internal virtual EntityParameter Clone()
	{
		return new EntityParameter(this);
	}

	private void CloneHelper(EntityParameter destination)
	{
		destination._value = _value;
		destination._direction = _direction;
		destination._size = _size;
		destination._sourceColumn = _sourceColumn;
		destination._sourceVersion = _sourceVersion;
		destination._sourceColumnNullMapping = _sourceColumnNullMapping;
		destination._isNullable = _isNullable;
		destination._parameterName = _parameterName;
		destination._dbType = _dbType;
		destination._edmType = _edmType;
		destination._precision = _precision;
		destination._scale = _scale;
	}

	internal virtual TypeUsage GetTypeUsage()
	{
		if (!IsTypeConsistent)
		{
			throw new InvalidOperationException(Strings.EntityClient_EntityParameterInconsistentEdmType(_edmType.FullName, _parameterName));
		}
		if (_edmType != null)
		{
			return TypeUsage.Create(_edmType);
		}
		if (!DbTypeMap.TryGetModelTypeUsage(DbType, out var modelType))
		{
			if (DbType == DbType.Object && Value != null && ClrProviderManifest.Instance.TryGetPrimitiveType(Value.GetType(), out var primitiveType) && (Helper.IsSpatialType(primitiveType) || Helper.IsHierarchyIdType(primitiveType)))
			{
				return EdmProviderManifest.Instance.GetCanonicalModelTypeUsage(primitiveType.PrimitiveTypeKind);
			}
			throw new InvalidOperationException(Strings.EntityClient_UnsupportedDbType(DbType.ToString(), ParameterName));
		}
		return modelType;
	}

	internal virtual void ResetIsDirty()
	{
		_isDirty = false;
	}

	private static DbType GetDbTypeFromEdm(EdmType edmType)
	{
		PrimitiveType type = Helper.AsPrimitive(edmType);
		if (Helper.IsSpatialType(type))
		{
			return DbType.Object;
		}
		if (DbCommandDefinition.TryGetDbTypeFromPrimitiveType(type, out var dbType))
		{
			return dbType;
		}
		return DbType.AnsiString;
	}

	private void ResetSize()
	{
		if (_size.HasValue)
		{
			PropertyChanging();
			_size = null;
		}
	}

	private bool ShouldSerializeSize()
	{
		if (_size.HasValue)
		{
			return _size.Value != 0;
		}
		return false;
	}

	internal virtual void CopyTo(DbParameter destination)
	{
		CloneHelper((EntityParameter)destination);
	}

	internal virtual object CompareExchangeParent(object value, object comparand)
	{
		object parent = _parent;
		if (comparand == parent)
		{
			_parent = value;
		}
		return parent;
	}

	internal virtual void ResetParent()
	{
		_parent = null;
	}

	public override string ToString()
	{
		return ParameterName;
	}

	private static int ValueSizeCore(object value)
	{
		if (!EntityUtil.IsNull(value))
		{
			if (value is string text)
			{
				return text.Length;
			}
			if (value is byte[] array)
			{
				return array.Length;
			}
			if (value is char[] array2)
			{
				return array2.Length;
			}
			if (value is byte || value is char)
			{
				return 1;
			}
		}
		return 0;
	}
}
