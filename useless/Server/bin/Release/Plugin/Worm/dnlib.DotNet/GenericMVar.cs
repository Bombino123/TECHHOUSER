using System.Runtime.InteropServices;

namespace dnlib.DotNet;

[ComVisible(true)]
public sealed class GenericMVar : GenericSig
{
	public override ElementType ElementType => ElementType.MVar;

	public GenericMVar(uint number)
		: base(isTypeVar: false, number)
	{
	}

	public GenericMVar(int number)
		: base(isTypeVar: false, (uint)number)
	{
	}

	public GenericMVar(uint number, MethodDef genericParamProvider)
		: base(isTypeVar: false, number, genericParamProvider)
	{
	}

	public GenericMVar(int number, MethodDef genericParamProvider)
		: base(isTypeVar: false, (uint)number, genericParamProvider)
	{
	}
}
