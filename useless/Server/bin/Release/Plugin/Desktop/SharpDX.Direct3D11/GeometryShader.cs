using System;
using System.Runtime.InteropServices;

namespace SharpDX.Direct3D11;

[Guid("38325b96-effb-4022-ba02-2e795b70275c")]
public class GeometryShader : DeviceChild
{
	public const int StreamOutputNoRasterizedStream = -1;

	public const int StreamOutputStreamCount = 4;

	public const int StreamOutputOutputComponentCount = 128;

	public const int StreamOutputBufferSlotCount = 4;

	public unsafe GeometryShader(Device device, byte[] shaderBytecode, ClassLinkage linkage = null)
		: base(IntPtr.Zero)
	{
		if (shaderBytecode == null)
		{
			throw new ArgumentNullException("shaderBytecode", "ShaderBytecode cannot be null");
		}
		fixed (byte* ptr = shaderBytecode)
		{
			void* ptr2 = ptr;
			device.CreateGeometryShader((IntPtr)ptr2, shaderBytecode.Length, linkage, this);
		}
	}

	public unsafe GeometryShader(Device device, byte[] shaderBytecode, StreamOutputElement[] elements, int[] bufferedStrides, int rasterizedStream, ClassLinkage linkage = null)
		: base(IntPtr.Zero)
	{
		fixed (byte* ptr = shaderBytecode)
		{
			void* ptr2 = ptr;
			device.CreateGeometryShaderWithStreamOutput((IntPtr)ptr2, shaderBytecode.Length, elements, elements.Length, bufferedStrides, bufferedStrides.Length, rasterizedStream, linkage, this);
		}
	}

	public GeometryShader(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator GeometryShader(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new GeometryShader(nativePtr);
		}
		return null;
	}
}
