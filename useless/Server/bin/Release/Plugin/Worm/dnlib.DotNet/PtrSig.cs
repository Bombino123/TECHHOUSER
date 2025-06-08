using System.Runtime.InteropServices;

namespace dnlib.DotNet;

[ComVisible(true)]
public sealed class PtrSig : NonLeafSig
{
	public override ElementType ElementType => ElementType.Ptr;

	public PtrSig(TypeSig nextSig)
		: base(nextSig)
	{
	}
}
