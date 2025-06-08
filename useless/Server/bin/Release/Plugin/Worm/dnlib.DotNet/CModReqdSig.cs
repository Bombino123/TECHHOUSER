using System.Runtime.InteropServices;

namespace dnlib.DotNet;

[ComVisible(true)]
public sealed class CModReqdSig : ModifierSig
{
	public override ElementType ElementType => ElementType.CModReqd;

	public CModReqdSig(ITypeDefOrRef modifier, TypeSig nextSig)
		: base(modifier, nextSig)
	{
	}
}
