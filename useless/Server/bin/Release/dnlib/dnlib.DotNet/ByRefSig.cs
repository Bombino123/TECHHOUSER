namespace dnlib.DotNet;

public sealed class ByRefSig : NonLeafSig
{
	public override ElementType ElementType => ElementType.ByRef;

	public ByRefSig(TypeSig nextSig)
		: base(nextSig)
	{
	}
}
