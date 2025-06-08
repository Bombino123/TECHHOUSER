namespace dnlib.DotNet;

public sealed class FnPtrSig : LeafSig
{
	private readonly CallingConventionSig signature;

	public override ElementType ElementType => ElementType.FnPtr;

	public CallingConventionSig Signature => signature;

	public MethodSig MethodSig => signature as MethodSig;

	public FnPtrSig(CallingConventionSig signature)
	{
		this.signature = signature;
	}
}
