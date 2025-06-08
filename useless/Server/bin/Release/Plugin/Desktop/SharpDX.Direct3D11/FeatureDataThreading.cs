using System.Runtime.InteropServices;
using SharpDX.Mathematics.Interop;

namespace SharpDX.Direct3D11;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
internal struct FeatureDataThreading
{
	public RawBool DriverConcurrentCreates;

	public RawBool DriverCommandLists;
}
