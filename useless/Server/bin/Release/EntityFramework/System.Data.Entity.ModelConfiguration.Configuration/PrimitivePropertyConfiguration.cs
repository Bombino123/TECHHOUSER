using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive;
using System.Data.Entity.Utilities;

namespace System.Data.Entity.ModelConfiguration.Configuration;

public class PrimitivePropertyConfiguration
{
	private readonly System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive.PrimitivePropertyConfiguration _configuration;

	internal System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive.PrimitivePropertyConfiguration Configuration => _configuration;

	internal PrimitivePropertyConfiguration(System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive.PrimitivePropertyConfiguration configuration)
	{
		_configuration = configuration;
	}

	public PrimitivePropertyConfiguration IsOptional()
	{
		Configuration.IsNullable = true;
		return this;
	}

	public PrimitivePropertyConfiguration IsRequired()
	{
		Configuration.IsNullable = false;
		return this;
	}

	public PrimitivePropertyConfiguration HasDatabaseGeneratedOption(DatabaseGeneratedOption? databaseGeneratedOption)
	{
		if (databaseGeneratedOption.HasValue && !Enum.IsDefined(typeof(DatabaseGeneratedOption), databaseGeneratedOption))
		{
			throw new ArgumentOutOfRangeException("databaseGeneratedOption");
		}
		Configuration.DatabaseGeneratedOption = databaseGeneratedOption;
		return this;
	}

	public PrimitivePropertyConfiguration IsConcurrencyToken()
	{
		IsConcurrencyToken(true);
		return this;
	}

	public PrimitivePropertyConfiguration IsConcurrencyToken(bool? concurrencyToken)
	{
		Configuration.ConcurrencyMode = ((!concurrencyToken.HasValue) ? null : new ConcurrencyMode?(concurrencyToken.Value ? ConcurrencyMode.Fixed : ConcurrencyMode.None));
		return this;
	}

	public PrimitivePropertyConfiguration HasColumnType(string columnType)
	{
		Configuration.ColumnType = columnType;
		return this;
	}

	public PrimitivePropertyConfiguration HasColumnName(string columnName)
	{
		Configuration.ColumnName = columnName;
		return this;
	}

	public PrimitivePropertyConfiguration HasColumnAnnotation(string name, object value)
	{
		Check.NotEmpty(name, "name");
		Configuration.SetAnnotation(name, value);
		return this;
	}

	public PrimitivePropertyConfiguration HasParameterName(string parameterName)
	{
		Configuration.ParameterName = parameterName;
		return this;
	}

	public PrimitivePropertyConfiguration HasColumnOrder(int? columnOrder)
	{
		if (columnOrder.HasValue && columnOrder.Value < 0)
		{
			throw new ArgumentOutOfRangeException("columnOrder");
		}
		Configuration.ColumnOrder = columnOrder;
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
