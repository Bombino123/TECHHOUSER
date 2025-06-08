namespace dnlib.DotNet;

public class PropertyDefUser : PropertyDef
{
	public PropertyDefUser()
	{
	}

	public PropertyDefUser(UTF8String name)
		: this(name, null)
	{
	}

	public PropertyDefUser(UTF8String name, PropertySig sig)
		: this(name, sig, (PropertyAttributes)0)
	{
	}

	public PropertyDefUser(UTF8String name, PropertySig sig, PropertyAttributes flags)
	{
		base.name = name;
		type = sig;
		attributes = (int)flags;
	}
}
