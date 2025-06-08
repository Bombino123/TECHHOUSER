using System.Runtime.InteropServices;

namespace SharpDX.Direct3D11;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
public struct DrawInstancedIndirectArguments
{
	public int VertexCountPerInstance;

	public int InstanceCount;

	public int StartVertexLocation;

	public int StartInstanceLocation;
}
