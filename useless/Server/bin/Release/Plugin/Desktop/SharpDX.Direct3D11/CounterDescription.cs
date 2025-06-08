using System.Runtime.InteropServices;

namespace SharpDX.Direct3D11;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
public struct CounterDescription
{
	public CounterKind Counter;

	public int MiscFlags;
}
