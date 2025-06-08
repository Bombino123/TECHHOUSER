using System.Runtime.InteropServices;

namespace SharpDX.DXGI;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
public struct InfoQueueFilter
{
	public InfoQueueFilterDescription AllowList;

	public InfoQueueFilterDescription DenyList;
}
