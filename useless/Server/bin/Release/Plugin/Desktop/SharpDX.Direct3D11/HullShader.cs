using System;
using System.Runtime.InteropServices;

namespace SharpDX.Direct3D11;

[Guid("8e5c6061-628a-4c8e-8264-bbe45cb3d5dd")]
public class HullShader : DeviceChild
{
	public unsafe HullShader(Device device, byte[] shaderBytecode, ClassLinkage linkage = null)
		: base(IntPtr.Zero)
	{
		if (shaderBytecode == null)
		{
			throw new ArgumentNullException("shaderBytecode", "ShaderBytecode cannot be null");
		}
		fixed (byte* ptr = shaderBytecode)
		{
			void* ptr2 = ptr;
			device.CreateHullShader((IntPtr)ptr2, shaderBytecode.Length, linkage, this);
		}
	}

	public HullShader(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator HullShader(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new HullShader(nativePtr);
		}
		return null;
	}
}
