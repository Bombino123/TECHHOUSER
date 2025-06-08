namespace dnlib.DotNet;

public class InterfaceImplUser : InterfaceImpl
{
	public InterfaceImplUser()
	{
	}

	public InterfaceImplUser(ITypeDefOrRef @interface)
	{
		base.@interface = @interface;
	}
}
