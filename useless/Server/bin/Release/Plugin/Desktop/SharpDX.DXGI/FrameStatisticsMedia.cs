using System.Runtime.InteropServices;

namespace SharpDX.DXGI;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
public struct FrameStatisticsMedia
{
	public int PresentCount;

	public int PresentRefreshCount;

	public int SyncRefreshCount;

	public long SyncQPCTime;

	public long SyncGPUTime;

	public FramePresentationMode CompositionMode;

	public int ApprovedPresentDuration;
}
