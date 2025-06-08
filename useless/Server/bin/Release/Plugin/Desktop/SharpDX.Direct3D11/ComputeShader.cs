using System;
using System.Runtime.InteropServices;

namespace SharpDX.Direct3D11;

[Guid("4f5b196e-c2bd-495e-bd01-1fded38e4969")]
public class ComputeShader : DeviceChild
{
	public unsafe ComputeShader(Device device, byte[] shaderBytecode, ClassLinkage linkage = null)
		: base(IntPtr.Zero)
	{
		if (shaderBytecode == null)
		{
			throw new ArgumentNullException("shaderBytecode", "ShaderBytecode cannot be null");
		}
		fixed (byte* ptr = shaderBytecode)
		{
			void* ptr2 = ptr;
			device.CreateComputeShader((IntPtr)ptr2, shaderBytecode.Length, linkage, this);
		}
	}

	public ComputeShader(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator ComputeShader(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new ComputeShader(nativePtr);
		}
		return null;
	}
}
