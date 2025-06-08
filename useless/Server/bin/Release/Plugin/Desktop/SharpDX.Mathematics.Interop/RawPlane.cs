using System.Diagnostics;
using System.Runtime.InteropServices;

namespace SharpDX.Mathematics.Interop;

[StructLayout(LayoutKind.Sequential, Pack = 4)]
[DebuggerDisplay("Normal: {Normal}, D: {D}")]
public struct RawPlane
{
	public RawVector3 Normal;

	public float D;

	public RawPlane(RawVector3 normal, float d)
	{
		Normal = normal;
		D = d;
	}
}
