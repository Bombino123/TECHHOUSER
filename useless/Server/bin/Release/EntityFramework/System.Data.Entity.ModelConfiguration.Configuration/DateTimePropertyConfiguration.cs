using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive;

namespace System.Data.Entity.ModelConfiguration.Configuration;

public class DateTimePropertyConfiguration : PrimitivePropertyConfiguration
{
	internal new System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive.DateTimePropertyConfiguration Configuration => (System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive.DateTimePropertyConfiguration)base.Configuration;

	internal DateTimePropertyConfiguration(System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive.DateTimePropertyConfiguration configuration)
		: base(configuration)
	{
	}

	public new DateTimePropertyConfiguration IsOptional()
	{
		base.IsOptional();
		return this;
	}

	public new DateTimePropertyConfiguration IsRequired()
	{
		base.IsRequired();
		return this;
	}

	public new DateTimePropertyConfiguration HasDatabaseGeneratedOption(DatabaseGeneratedOption? databaseGeneratedOption)
	{
		base.HasDatabaseGeneratedOption(databaseGeneratedOption);
		return this;
	}

	public new DateTimePropertyConfiguration IsConcurrencyToken()
	{
		base.IsConcurrencyToken();
		return this;
	}

	public new DateTimePropertyConfiguration IsConcurrencyToken(bool? concurrencyToken)
	{
		base.IsConcurrencyToken(concurrencyToken);
		return this;
	}

	public new DateTimePropertyConfiguration HasColumnName(string columnName)
	{
		base.HasColumnName(columnName);
		return this;
	}

	public new DateTimePropertyConfiguration HasColumnAnnotation(string name, object value)
	{
		base.HasColumnAnnotation(name, value);
		return this;
	}

	public new DateTimePropertyConfiguration HasColumnType(string columnType)
	{
		base.HasColumnType(columnType);
		return this;
	}

	public new DateTimePropertyConfiguration HasColumnOrder(int? columnOrder)
	{
		base.HasColumnOrder(columnOrder);
		return this;
	}

	public DateTimePropertyConfiguration HasPrecision(byte value)
	{
		Configuration.Precision = value;
		return this;
	}
}
