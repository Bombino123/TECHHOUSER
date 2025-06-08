using System.Runtime.InteropServices;

namespace SharpDX.Direct3D11;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
public struct CounterCapabilities
{
	public CounterKind LastDeviceDependentCounter;

	public int SimultaneousCounterCount;

	public byte DetectableParallelUnitCount;
}
