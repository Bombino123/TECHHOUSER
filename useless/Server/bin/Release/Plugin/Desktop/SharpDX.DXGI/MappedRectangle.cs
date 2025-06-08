using System;
using System.Runtime.InteropServices;

namespace SharpDX.DXGI;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
internal struct MappedRectangle
{
	public int Pitch;

	public IntPtr PBits;
}
