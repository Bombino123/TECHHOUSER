using System.Runtime.InteropServices;
using SharpDX.Mathematics.Interop;

namespace SharpDX.DXGI;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
public struct OutputDuplicatePointerPosition
{
	public RawPoint Position;

	public RawBool Visible;
}
