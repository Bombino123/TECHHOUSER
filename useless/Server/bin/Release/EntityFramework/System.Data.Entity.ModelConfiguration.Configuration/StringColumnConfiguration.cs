using System.ComponentModel;
using System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive;

namespace System.Data.Entity.ModelConfiguration.Configuration;

public class StringColumnConfiguration : LengthColumnConfiguration
{
	internal new System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive.StringPropertyConfiguration Configuration => (System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive.StringPropertyConfiguration)base.Configuration;

	internal StringColumnConfiguration(System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive.StringPropertyConfiguration configuration)
		: base(configuration)
	{
	}

	public new StringColumnConfiguration IsMaxLength()
	{
		base.IsMaxLength();
		return this;
	}

	public new StringColumnConfiguration HasMaxLength(int? value)
	{
		base.HasMaxLength(value);
		return this;
	}

	public new StringColumnConfiguration IsFixedLength()
	{
		base.IsFixedLength();
		return this;
	}

	public new StringColumnConfiguration IsVariableLength()
	{
		base.IsVariableLength();
		return this;
	}

	public new StringColumnConfiguration IsOptional()
	{
		base.IsOptional();
		return this;
	}

	public new StringColumnConfiguration IsRequired()
	{
		base.IsRequired();
		return this;
	}

	public new StringColumnConfiguration HasColumnType(string columnType)
	{
		base.HasColumnType(columnType);
		return this;
	}

	public new StringColumnConfiguration HasColumnOrder(int? columnOrder)
	{
		base.HasColumnOrder(columnOrder);
		return this;
	}

	public StringColumnConfiguration IsUnicode()
	{
		IsUnicode(true);
		return this;
	}

	public StringColumnConfiguration IsUnicode(bool? unicode)
	{
		Configuration.IsUnicode = unicode;
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
