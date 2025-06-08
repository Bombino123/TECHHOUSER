using System.Runtime.InteropServices;

namespace dnlib.DotNet;

[ComVisible(true)]
public sealed class ByRefSig : NonLeafSig
{
	public override ElementType ElementType => ElementType.ByRef;

	public ByRefSig(TypeSig nextSig)
		: base(nextSig)
	{
	}
}
