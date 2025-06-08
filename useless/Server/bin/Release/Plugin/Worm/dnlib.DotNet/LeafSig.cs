using System.Runtime.InteropServices;

namespace dnlib.DotNet;

[ComVisible(true)]
public abstract class LeafSig : TypeSig
{
	public sealed override TypeSig Next => null;
}
