using System.ComponentModel;
using System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive;

namespace System.Data.Entity.ModelConfiguration.Configuration;

public abstract class LengthColumnConfiguration : PrimitiveColumnConfiguration
{
	internal new System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive.LengthPropertyConfiguration Configuration => (System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive.LengthPropertyConfiguration)base.Configuration;

	internal LengthColumnConfiguration(System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive.LengthPropertyConfiguration configuration)
		: base(configuration)
	{
	}

	public LengthColumnConfiguration IsMaxLength()
	{
		Configuration.IsMaxLength = true;
		Configuration.MaxLength = null;
		return this;
	}

	public LengthColumnConfiguration HasMaxLength(int? value)
	{
		Configuration.MaxLength = value;
		Configuration.IsMaxLength = null;
		return this;
	}

	public LengthColumnConfiguration IsFixedLength()
	{
		Configuration.IsFixedLength = true;
		return this;
	}

	public LengthColumnConfiguration IsVariableLength()
	{
		Configuration.IsFixedLength = false;
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
