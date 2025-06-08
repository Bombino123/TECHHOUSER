using System.Runtime.InteropServices;

namespace SharpDX.Direct3D11;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
public struct VideoProcessorFilterRange
{
	public int Minimum;

	public int Maximum;

	public int Default;

	public float Multiplier;
}
