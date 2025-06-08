namespace dnlib.DotNet;

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
