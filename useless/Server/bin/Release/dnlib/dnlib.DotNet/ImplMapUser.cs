namespace dnlib.DotNet;

public class ImplMapUser : ImplMap
{
	public ImplMapUser()
	{
	}

	public ImplMapUser(ModuleRef scope, UTF8String name, PInvokeAttributes flags)
	{
		module = scope;
		base.name = name;
		attributes = (int)flags;
	}
}
