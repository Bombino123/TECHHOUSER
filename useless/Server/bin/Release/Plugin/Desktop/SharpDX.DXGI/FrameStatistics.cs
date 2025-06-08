using System.Runtime.InteropServices;

namespace SharpDX.DXGI;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
public struct FrameStatistics
{
	public int PresentCount;

	public int PresentRefreshCount;

	public int SyncRefreshCount;

	public long SyncQPCTime;

	public long SyncGPUTime;
}
