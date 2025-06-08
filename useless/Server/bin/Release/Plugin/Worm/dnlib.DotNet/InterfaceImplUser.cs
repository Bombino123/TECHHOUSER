using System.Runtime.InteropServices;

namespace dnlib.DotNet;

[ComVisible(true)]
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
