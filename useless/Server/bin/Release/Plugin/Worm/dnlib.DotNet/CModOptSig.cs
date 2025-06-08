using System.Runtime.InteropServices;

namespace dnlib.DotNet;

[ComVisible(true)]
public sealed class CModOptSig : ModifierSig
{
	public override ElementType ElementType => ElementType.CModOpt;

	public CModOptSig(ITypeDefOrRef modifier, TypeSig nextSig)
		: base(modifier, nextSig)
	{
	}
}
