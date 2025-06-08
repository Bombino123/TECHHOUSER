using System.Runtime.InteropServices;

namespace SharpDX.Direct3D11;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
public struct DepthStencilOperationDescription
{
	public StencilOperation FailOperation;

	public StencilOperation DepthFailOperation;

	public StencilOperation PassOperation;

	public Comparison Comparison;
}
