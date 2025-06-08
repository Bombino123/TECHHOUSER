using System.Runtime.InteropServices;

namespace dnlib.DotNet;

[ComVisible(true)]
public sealed class PinnedSig : NonLeafSig
{
	public override ElementType ElementType => ElementType.Pinned;

	public PinnedSig(TypeSig nextSig)
		: base(nextSig)
	{
	}
}
