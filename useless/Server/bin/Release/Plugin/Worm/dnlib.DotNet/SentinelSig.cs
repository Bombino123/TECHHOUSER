using System.Runtime.InteropServices;

namespace dnlib.DotNet;

[ComVisible(true)]
public sealed class SentinelSig : LeafSig
{
	public override ElementType ElementType => ElementType.Sentinel;
}
