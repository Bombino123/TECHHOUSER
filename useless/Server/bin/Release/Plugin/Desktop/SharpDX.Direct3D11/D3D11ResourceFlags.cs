using System.Runtime.InteropServices;

namespace SharpDX.Direct3D11;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
public struct D3D11ResourceFlags
{
	public int BindFlags;

	public int MiscFlags;

	public int CPUAccessFlags;

	public int StructureByteStride;
}
