namespace dnlib.DotNet;

public sealed class PtrSig : NonLeafSig
{
	public override ElementType ElementType => ElementType.Ptr;

	public PtrSig(TypeSig nextSig)
		: base(nextSig)
	{
	}
}
