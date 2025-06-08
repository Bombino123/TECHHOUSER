using System;
using System.Runtime.InteropServices;

namespace SharpDX.Direct3D11;

[Guid("3b301d64-d678-4289-8897-22f8928b72f3")]
public class VertexShader : DeviceChild
{
	public unsafe VertexShader(Device device, byte[] shaderBytecode, ClassLinkage linkage = null)
		: base(IntPtr.Zero)
	{
		if (shaderBytecode == null)
		{
			throw new ArgumentNullException("shaderBytecode", "ShaderBytecode cannot be null");
		}
		fixed (byte* ptr = shaderBytecode)
		{
			void* ptr2 = ptr;
			device.CreateVertexShader((IntPtr)ptr2, shaderBytecode.Length, linkage, this);
		}
	}

	public VertexShader(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator VertexShader(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new VertexShader(nativePtr);
		}
		return null;
	}
}
