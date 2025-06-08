using System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive;

namespace System.Data.Entity.ModelConfiguration.Configuration;

public abstract class LengthPropertyConfiguration : PrimitivePropertyConfiguration
{
	internal new System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive.LengthPropertyConfiguration Configuration => (System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive.LengthPropertyConfiguration)base.Configuration;

	internal LengthPropertyConfiguration(System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive.LengthPropertyConfiguration configuration)
		: base(configuration)
	{
	}

	public LengthPropertyConfiguration IsMaxLength()
	{
		Configuration.IsMaxLength = true;
		Configuration.MaxLength = null;
		return this;
	}

	public LengthPropertyConfiguration HasMaxLength(int? value)
	{
		Configuration.MaxLength = value;
		Configuration.IsMaxLength = null;
		Configuration.IsFixedLength = Configuration.IsFixedLength.GetValueOrDefault();
		return this;
	}

	public LengthPropertyConfiguration IsFixedLength()
	{
		Configuration.IsFixedLength = true;
		return this;
	}

	public LengthPropertyConfiguration IsVariableLength()
	{
		Configuration.IsFixedLength = false;
		return this;
	}
}
