using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive;
using System.Data.Entity.ModelConfiguration.Configuration.Types;
using System.Data.Entity.Resources;
using System.Data.Entity.Utilities;
using System.Reflection;

namespace System.Data.Entity.ModelConfiguration.Configuration;

public class ConventionPrimitivePropertyConfiguration
{
	private readonly PropertyInfo _propertyInfo;

	private readonly Func<System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive.PrimitivePropertyConfiguration> _configuration;

	private readonly Lazy<System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive.BinaryPropertyConfiguration> _binaryConfiguration;

	private readonly Lazy<System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive.DateTimePropertyConfiguration> _dateTimeConfiguration;

	private readonly Lazy<System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive.DecimalPropertyConfiguration> _decimalConfiguration;

	private readonly Lazy<System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive.LengthPropertyConfiguration> _lengthConfiguration;

	private readonly Lazy<System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive.StringPropertyConfiguration> _stringConfiguration;

	public virtual PropertyInfo ClrPropertyInfo => _propertyInfo;

	internal Func<System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive.PrimitivePropertyConfiguration> Configuration => _configuration;

	internal ConventionPrimitivePropertyConfiguration(PropertyInfo propertyInfo, Func<System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive.PrimitivePropertyConfiguration> configuration)
	{
		_propertyInfo = propertyInfo;
		_configuration = configuration;
		_binaryConfiguration = new Lazy<System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive.BinaryPropertyConfiguration>(() => _configuration() as System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive.BinaryPropertyConfiguration);
		_dateTimeConfiguration = new Lazy<System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive.DateTimePropertyConfiguration>(() => _configuration() as System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive.DateTimePropertyConfiguration);
		_decimalConfiguration = new Lazy<System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive.DecimalPropertyConfiguration>(() => _configuration() as System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive.DecimalPropertyConfiguration);
		_lengthConfiguration = new Lazy<System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive.LengthPropertyConfiguration>(() => _configuration() as System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive.LengthPropertyConfiguration);
		_stringConfiguration = new Lazy<System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive.StringPropertyConfiguration>(() => _configuration() as System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive.StringPropertyConfiguration);
	}

	public virtual ConventionPrimitivePropertyConfiguration HasColumnName(string columnName)
	{
		Check.NotEmpty(columnName, "columnName");
		if (_configuration() != null && _configuration().ColumnName == null)
		{
			_configuration().ColumnName = columnName;
		}
		return this;
	}

	public virtual ConventionPrimitivePropertyConfiguration HasColumnAnnotation(string name, object value)
	{
		Check.NotEmpty(name, "name");
		if (_configuration() != null && !_configuration().Annotations.ContainsKey(name))
		{
			_configuration().SetAnnotation(name, value);
		}
		return this;
	}

	public virtual ConventionPrimitivePropertyConfiguration HasParameterName(string parameterName)
	{
		Check.NotEmpty(parameterName, "parameterName");
		if (_configuration() != null && _configuration().ParameterName == null)
		{
			_configuration().ParameterName = parameterName;
		}
		return this;
	}

	public virtual ConventionPrimitivePropertyConfiguration HasColumnOrder(int columnOrder)
	{
		if (columnOrder < 0)
		{
			throw new ArgumentOutOfRangeException("columnOrder");
		}
		if (_configuration() != null && !_configuration().ColumnOrder.HasValue)
		{
			_configuration().ColumnOrder = columnOrder;
		}
		return this;
	}

	public virtual ConventionPrimitivePropertyConfiguration HasColumnType(string columnType)
	{
		Check.NotEmpty(columnType, "columnType");
		if (_configuration() != null && _configuration().ColumnType == null)
		{
			_configuration().ColumnType = columnType;
		}
		return this;
	}

	public virtual ConventionPrimitivePropertyConfiguration IsConcurrencyToken()
	{
		return IsConcurrencyToken(concurrencyToken: true);
	}

	public virtual ConventionPrimitivePropertyConfiguration IsConcurrencyToken(bool concurrencyToken)
	{
		if (_configuration() != null && !_configuration().ConcurrencyMode.HasValue)
		{
			_configuration().ConcurrencyMode = (concurrencyToken ? ConcurrencyMode.Fixed : ConcurrencyMode.None);
		}
		return this;
	}

	public virtual ConventionPrimitivePropertyConfiguration HasDatabaseGeneratedOption(DatabaseGeneratedOption databaseGeneratedOption)
	{
		if (!Enum.IsDefined(typeof(DatabaseGeneratedOption), databaseGeneratedOption))
		{
			throw new ArgumentOutOfRangeException("databaseGeneratedOption");
		}
		if (_configuration() != null && !_configuration().DatabaseGeneratedOption.HasValue)
		{
			_configuration().DatabaseGeneratedOption = databaseGeneratedOption;
		}
		return this;
	}

	public virtual ConventionPrimitivePropertyConfiguration IsOptional()
	{
		if (_configuration() != null && !_configuration().IsNullable.HasValue)
		{
			if (!_propertyInfo.PropertyType.IsNullable())
			{
				throw new InvalidOperationException(Strings.LightweightPrimitivePropertyConfiguration_NonNullableProperty(_propertyInfo.DeclaringType?.ToString() + "." + _propertyInfo.Name, _propertyInfo.PropertyType.Name));
			}
			_configuration().IsNullable = true;
		}
		return this;
	}

	public virtual ConventionPrimitivePropertyConfiguration IsRequired()
	{
		if (_configuration() != null && !_configuration().IsNullable.HasValue)
		{
			_configuration().IsNullable = false;
		}
		return this;
	}

	public virtual ConventionPrimitivePropertyConfiguration IsUnicode()
	{
		return IsUnicode(unicode: true);
	}

	public virtual ConventionPrimitivePropertyConfiguration IsUnicode(bool unicode)
	{
		if (_configuration() != null)
		{
			if (_stringConfiguration.Value == null)
			{
				throw new InvalidOperationException(Strings.LightweightPrimitivePropertyConfiguration_IsUnicodeNonString(_propertyInfo.Name));
			}
			if (!_stringConfiguration.Value.IsUnicode.HasValue)
			{
				_stringConfiguration.Value.IsUnicode = unicode;
			}
		}
		return this;
	}

	public virtual ConventionPrimitivePropertyConfiguration IsFixedLength()
	{
		if (_configuration() != null)
		{
			if (_lengthConfiguration.Value == null)
			{
				throw new InvalidOperationException(Strings.LightweightPrimitivePropertyConfiguration_NonLength(_propertyInfo.Name));
			}
			if (!_lengthConfiguration.Value.IsFixedLength.HasValue)
			{
				_lengthConfiguration.Value.IsFixedLength = true;
			}
		}
		return this;
	}

	public virtual ConventionPrimitivePropertyConfiguration IsVariableLength()
	{
		if (_configuration() != null)
		{
			if (_lengthConfiguration.Value == null)
			{
				throw new InvalidOperationException(Strings.LightweightPrimitivePropertyConfiguration_NonLength(_propertyInfo.Name));
			}
			if (!_lengthConfiguration.Value.IsFixedLength.HasValue)
			{
				_lengthConfiguration.Value.IsFixedLength = false;
			}
		}
		return this;
	}

	public virtual ConventionPrimitivePropertyConfiguration HasMaxLength(int maxLength)
	{
		if (maxLength < 1)
		{
			throw new ArgumentOutOfRangeException("maxLength");
		}
		if (_configuration() != null)
		{
			if (_lengthConfiguration.Value == null)
			{
				throw new InvalidOperationException(Strings.LightweightPrimitivePropertyConfiguration_NonLength(_propertyInfo.Name));
			}
			if (!_lengthConfiguration.Value.MaxLength.HasValue && !_lengthConfiguration.Value.IsMaxLength.HasValue)
			{
				_lengthConfiguration.Value.MaxLength = maxLength;
				if (!_lengthConfiguration.Value.IsFixedLength.HasValue)
				{
					_lengthConfiguration.Value.IsFixedLength = false;
				}
			}
		}
		return this;
	}

	public virtual ConventionPrimitivePropertyConfiguration IsMaxLength()
	{
		if (_configuration() != null)
		{
			if (_lengthConfiguration.Value == null)
			{
				throw new InvalidOperationException(Strings.LightweightPrimitivePropertyConfiguration_NonLength(_propertyInfo.Name));
			}
			if (!_lengthConfiguration.Value.IsMaxLength.HasValue && !_lengthConfiguration.Value.MaxLength.HasValue)
			{
				_lengthConfiguration.Value.IsMaxLength = true;
			}
		}
		return this;
	}

	public virtual ConventionPrimitivePropertyConfiguration HasPrecision(byte value)
	{
		if (_configuration() != null)
		{
			if (_dateTimeConfiguration.Value == null)
			{
				if (_decimalConfiguration.Value != null)
				{
					throw new InvalidOperationException(Strings.LightweightPrimitivePropertyConfiguration_DecimalNoScale(_propertyInfo.Name));
				}
				throw new InvalidOperationException(Strings.LightweightPrimitivePropertyConfiguration_HasPrecisionNonDateTime(_propertyInfo.Name));
			}
			if (!_dateTimeConfiguration.Value.Precision.HasValue)
			{
				_dateTimeConfiguration.Value.Precision = value;
			}
		}
		return this;
	}

	public virtual ConventionPrimitivePropertyConfiguration HasPrecision(byte precision, byte scale)
	{
		if (_configuration() != null)
		{
			if (_decimalConfiguration.Value == null)
			{
				if (_dateTimeConfiguration.Value != null)
				{
					throw new InvalidOperationException(Strings.LightweightPrimitivePropertyConfiguration_DateTimeScale(_propertyInfo.Name));
				}
				throw new InvalidOperationException(Strings.LightweightPrimitivePropertyConfiguration_HasPrecisionNonDecimal(_propertyInfo.Name));
			}
			if (!_decimalConfiguration.Value.Precision.HasValue && !_decimalConfiguration.Value.Scale.HasValue)
			{
				_decimalConfiguration.Value.Precision = precision;
				_decimalConfiguration.Value.Scale = scale;
			}
		}
		return this;
	}

	public virtual ConventionPrimitivePropertyConfiguration IsRowVersion()
	{
		if (_configuration() != null)
		{
			if (_binaryConfiguration.Value == null)
			{
				throw new InvalidOperationException(Strings.LightweightPrimitivePropertyConfiguration_IsRowVersionNonBinary(_propertyInfo.Name));
			}
			if (!_binaryConfiguration.Value.IsRowVersion.HasValue)
			{
				_binaryConfiguration.Value.IsRowVersion = true;
			}
		}
		return this;
	}

	public virtual ConventionPrimitivePropertyConfiguration IsKey()
	{
		if (_configuration() != null && _configuration().TypeConfiguration is EntityTypeConfiguration { IsKeyConfigured: false } entityTypeConfiguration)
		{
			entityTypeConfiguration.Key(ClrPropertyInfo);
		}
		return this;
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
}
