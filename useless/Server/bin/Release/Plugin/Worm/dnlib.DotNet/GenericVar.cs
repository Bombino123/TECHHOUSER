using System.Runtime.InteropServices;

namespace dnlib.DotNet;

[ComVisible(true)]
public sealed class GenericVar : GenericSig
{
	public override ElementType ElementType => ElementType.Var;

	public GenericVar(uint number)
		: base(isTypeVar: true, number)
	{
	}

	public GenericVar(int number)
		: base(isTypeVar: true, (uint)number)
	{
	}

	public GenericVar(uint number, TypeDef genericParamProvider)
		: base(isTypeVar: true, number, genericParamProvider)
	{
	}

	public GenericVar(int number, TypeDef genericParamProvider)
		: base(isTypeVar: true, (uint)number, genericParamProvider)
	{
	}
}
