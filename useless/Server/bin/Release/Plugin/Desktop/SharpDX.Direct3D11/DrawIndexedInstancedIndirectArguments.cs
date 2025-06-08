using System.Runtime.InteropServices;

namespace SharpDX.Direct3D11;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
public struct DrawIndexedInstancedIndirectArguments
{
	public int IndexCountPerInstance;

	public int InstanceCount;

	public int StartIndexLocation;

	public int BaseVertexLocation;

	public int StartInstanceLocation;
}
