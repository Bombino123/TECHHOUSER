using System.Runtime.InteropServices;
using SharpDX.Mathematics.Interop;

namespace SharpDX.Direct3D11;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
public struct ClassInstanceDescription
{
	public int InstanceId;

	public int InstanceIndex;

	public int TypeId;

	public int ConstantBuffer;

	public int BaseConstantBufferOffset;

	public int BaseTexture;

	public int BaseSampler;

	public RawBool IsCreated;
}
