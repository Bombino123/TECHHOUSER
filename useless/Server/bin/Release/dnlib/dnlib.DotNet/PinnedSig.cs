namespace dnlib.DotNet;

public sealed class PinnedSig : NonLeafSig
{
	public override ElementType ElementType => ElementType.Pinned;

	public PinnedSig(TypeSig nextSig)
		: base(nextSig)
	{
	}
}
