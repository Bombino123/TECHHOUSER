using System.Runtime.InteropServices;
using SharpDX.Mathematics.Interop;

namespace SharpDX.Direct3D11;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
public struct DepthStencilStateDescription
{
	public RawBool IsDepthEnabled;

	public DepthWriteMask DepthWriteMask;

	public Comparison DepthComparison;

	public RawBool IsStencilEnabled;

	public byte StencilReadMask;

	public byte StencilWriteMask;

	public DepthStencilOperationDescription FrontFace;

	public DepthStencilOperationDescription BackFace;

	public static DepthStencilStateDescription Default()
	{
		DepthStencilStateDescription result = default(DepthStencilStateDescription);
		result.IsDepthEnabled = true;
		result.DepthWriteMask = DepthWriteMask.All;
		result.DepthComparison = Comparison.Less;
		result.IsStencilEnabled = false;
		result.StencilReadMask = byte.MaxValue;
		result.StencilWriteMask = byte.MaxValue;
		result.FrontFace.Comparison = Comparison.Always;
		result.FrontFace.DepthFailOperation = StencilOperation.Keep;
		result.FrontFace.FailOperation = StencilOperation.Keep;
		result.FrontFace.PassOperation = StencilOperation.Keep;
		result.BackFace.Comparison = Comparison.Always;
		result.BackFace.DepthFailOperation = StencilOperation.Keep;
		result.BackFace.FailOperation = StencilOperation.Keep;
		result.BackFace.PassOperation = StencilOperation.Keep;
		return result;
	}
}
