using System;
using System.Runtime.InteropServices;

namespace SharpDX.Direct3D11;

[Guid("ea82e40d-51dc-4f33-93d4-db7c9125ae8c")]
public class PixelShader : DeviceChild
{
	public unsafe PixelShader(Device device, byte[] shaderBytecode, ClassLinkage linkage = null)
		: base(IntPtr.Zero)
	{
		if (shaderBytecode == null)
		{
			throw new ArgumentNullException("shaderBytecode", "ShaderBytecode cannot be null");
		}
		fixed (byte* ptr = shaderBytecode)
		{
			void* ptr2 = ptr;
			device.CreatePixelShader((IntPtr)ptr2, shaderBytecode.Length, linkage, this);
		}
	}

	public PixelShader(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator PixelShader(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new PixelShader(nativePtr);
		}
		return null;
	}
}
