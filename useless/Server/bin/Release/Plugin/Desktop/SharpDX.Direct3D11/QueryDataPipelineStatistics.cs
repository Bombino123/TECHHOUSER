using System.Runtime.InteropServices;

namespace SharpDX.Direct3D11;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
public struct QueryDataPipelineStatistics
{
	public long IAVerticeCount;

	public long IAPrimitiveCount;

	public long VSInvocationCount;

	public long GSInvocationCount;

	public long GSPrimitiveCount;

	public long CInvocationCount;

	public long CPrimitiveCount;

	public long PSInvocationCount;

	public long HSInvocationCount;

	public long DSInvocationCount;

	public long CSInvocationCount;
}
