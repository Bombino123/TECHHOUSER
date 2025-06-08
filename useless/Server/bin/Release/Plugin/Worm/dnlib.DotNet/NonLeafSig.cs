using System.Runtime.InteropServices;

namespace dnlib.DotNet;

[ComVisible(true)]
public abstract class NonLeafSig : TypeSig
{
	private readonly TypeSig nextSig;

	public sealed override TypeSig Next => nextSig;

	protected NonLeafSig(TypeSig nextSig)
	{
		this.nextSig = nextSig;
	}
}
