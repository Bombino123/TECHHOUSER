using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive;

namespace System.Data.Entity.ModelConfiguration.Configuration;

public class StringPropertyConfiguration : LengthPropertyConfiguration
{
	internal new System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive.StringPropertyConfiguration Configuration => (System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive.StringPropertyConfiguration)base.Configuration;

	internal StringPropertyConfiguration(System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive.StringPropertyConfiguration configuration)
		: base(configuration)
	{
	}

	public new StringPropertyConfiguration IsMaxLength()
	{
		base.IsMaxLength();
		return this;
	}

	public new StringPropertyConfiguration HasMaxLength(int? value)
	{
		base.HasMaxLength(value);
		return this;
	}

	public new StringPropertyConfiguration IsFixedLength()
	{
		base.IsFixedLength();
		return this;
	}

	public new StringPropertyConfiguration IsVariableLength()
	{
		base.IsVariableLength();
		return this;
	}

	public new StringPropertyConfiguration IsOptional()
	{
		base.IsOptional();
		return this;
	}

	public new StringPropertyConfiguration IsRequired()
	{
		base.IsRequired();
		return this;
	}

	public new StringPropertyConfiguration HasDatabaseGeneratedOption(DatabaseGeneratedOption? databaseGeneratedOption)
	{
		base.HasDatabaseGeneratedOption(databaseGeneratedOption);
		return this;
	}

	public new StringPropertyConfiguration IsConcurrencyToken()
	{
		base.IsConcurrencyToken();
		return this;
	}

	public new StringPropertyConfiguration IsConcurrencyToken(bool? concurrencyToken)
	{
		base.IsConcurrencyToken(concurrencyToken);
		return this;
	}

	public new StringPropertyConfiguration HasColumnName(string columnName)
	{
		base.HasColumnName(columnName);
		return this;
	}

	public new StringPropertyConfiguration HasColumnAnnotation(string name, object value)
	{
		base.HasColumnAnnotation(name, value);
		return this;
	}

	public new StringPropertyConfiguration HasColumnType(string columnType)
	{
		base.HasColumnType(columnType);
		return this;
	}

	public new StringPropertyConfiguration HasColumnOrder(int? columnOrder)
	{
		base.HasColumnOrder(columnOrder);
		return this;
	}

	public StringPropertyConfiguration IsUnicode()
	{
		IsUnicode(true);
		return this;
	}

	public StringPropertyConfiguration IsUnicode(bool? unicode)
	{
		Configuration.IsUnicode = unicode;
		return this;
	}
}
