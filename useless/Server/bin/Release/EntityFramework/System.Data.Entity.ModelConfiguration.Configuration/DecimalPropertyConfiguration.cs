using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive;

namespace System.Data.Entity.ModelConfiguration.Configuration;

public class DecimalPropertyConfiguration : PrimitivePropertyConfiguration
{
	internal new System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive.DecimalPropertyConfiguration Configuration => (System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive.DecimalPropertyConfiguration)base.Configuration;

	internal DecimalPropertyConfiguration(System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive.DecimalPropertyConfiguration configuration)
		: base(configuration)
	{
	}

	public new DecimalPropertyConfiguration IsOptional()
	{
		base.IsOptional();
		return this;
	}

	public new DecimalPropertyConfiguration IsRequired()
	{
		base.IsRequired();
		return this;
	}

	public new DecimalPropertyConfiguration HasDatabaseGeneratedOption(DatabaseGeneratedOption? databaseGeneratedOption)
	{
		base.HasDatabaseGeneratedOption(databaseGeneratedOption);
		return this;
	}

	public new DecimalPropertyConfiguration IsConcurrencyToken()
	{
		base.IsConcurrencyToken();
		return this;
	}

	public new DecimalPropertyConfiguration IsConcurrencyToken(bool? concurrencyToken)
	{
		base.IsConcurrencyToken(concurrencyToken);
		return this;
	}

	public new DecimalPropertyConfiguration HasColumnName(string columnName)
	{
		base.HasColumnName(columnName);
		return this;
	}

	public new DecimalPropertyConfiguration HasColumnAnnotation(string name, object value)
	{
		base.HasColumnAnnotation(name, value);
		return this;
	}

	public new DecimalPropertyConfiguration HasColumnType(string columnType)
	{
		base.HasColumnType(columnType);
		return this;
	}

	public new DecimalPropertyConfiguration HasColumnOrder(int? columnOrder)
	{
		base.HasColumnOrder(columnOrder);
		return this;
	}

	public DecimalPropertyConfiguration HasPrecision(byte precision, byte scale)
	{
		Configuration.Precision = precision;
		Configuration.Scale = scale;
		return this;
	}
}
