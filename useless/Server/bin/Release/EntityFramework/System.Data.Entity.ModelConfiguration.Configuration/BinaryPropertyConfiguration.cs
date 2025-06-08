using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive;

namespace System.Data.Entity.ModelConfiguration.Configuration;

public class BinaryPropertyConfiguration : LengthPropertyConfiguration
{
	internal new System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive.BinaryPropertyConfiguration Configuration => (System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive.BinaryPropertyConfiguration)base.Configuration;

	internal BinaryPropertyConfiguration(System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive.BinaryPropertyConfiguration configuration)
		: base(configuration)
	{
	}

	public new BinaryPropertyConfiguration IsMaxLength()
	{
		base.IsMaxLength();
		return this;
	}

	public new BinaryPropertyConfiguration HasMaxLength(int? value)
	{
		base.HasMaxLength(value);
		return this;
	}

	public new BinaryPropertyConfiguration IsFixedLength()
	{
		base.IsFixedLength();
		return this;
	}

	public new BinaryPropertyConfiguration IsVariableLength()
	{
		base.IsVariableLength();
		return this;
	}

	public new BinaryPropertyConfiguration IsOptional()
	{
		base.IsOptional();
		return this;
	}

	public new BinaryPropertyConfiguration IsRequired()
	{
		base.IsRequired();
		return this;
	}

	public new BinaryPropertyConfiguration HasDatabaseGeneratedOption(DatabaseGeneratedOption? databaseGeneratedOption)
	{
		base.HasDatabaseGeneratedOption(databaseGeneratedOption);
		return this;
	}

	public new BinaryPropertyConfiguration IsConcurrencyToken()
	{
		base.IsConcurrencyToken();
		return this;
	}

	public new BinaryPropertyConfiguration IsConcurrencyToken(bool? concurrencyToken)
	{
		base.IsConcurrencyToken(concurrencyToken);
		return this;
	}

	public new BinaryPropertyConfiguration HasColumnName(string columnName)
	{
		base.HasColumnName(columnName);
		return this;
	}

	public new BinaryPropertyConfiguration HasColumnAnnotation(string name, object value)
	{
		base.HasColumnAnnotation(name, value);
		return this;
	}

	public new BinaryPropertyConfiguration HasColumnType(string columnType)
	{
		base.HasColumnType(columnType);
		return this;
	}

	public new BinaryPropertyConfiguration HasColumnOrder(int? columnOrder)
	{
		base.HasColumnOrder(columnOrder);
		return this;
	}

	public BinaryPropertyConfiguration IsRowVersion()
	{
		Configuration.IsRowVersion = true;
		return this;
	}
}
