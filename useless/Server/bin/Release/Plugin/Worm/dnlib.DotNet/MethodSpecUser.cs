using System.Runtime.InteropServices;

namespace dnlib.DotNet;

[ComVisible(true)]
public class MethodSpecUser : MethodSpec
{
	public MethodSpecUser()
	{
	}

	public MethodSpecUser(IMethodDefOrRef method)
		: this(method, null)
	{
	}

	public MethodSpecUser(IMethodDefOrRef method, GenericInstMethodSig sig)
	{
		base.method = method;
		instantiation = sig;
	}
}
