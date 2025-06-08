using System;
using System.Runtime.InteropServices;

namespace SharpDX.Direct3D11;

[Guid("f582c508-0f36-490c-9977-31eece268cfa")]
public class DomainShader : DeviceChild
{
	public unsafe DomainShader(Device device, byte[] shaderBytecode, ClassLinkage linkage = null)
		: base(IntPtr.Zero)
	{
		if (shaderBytecode == null)
		{
			throw new ArgumentNullException("shaderBytecode", "ShaderBytecode cannot be null");
		}
		fixed (byte* ptr = shaderBytecode)
		{
			void* ptr2 = ptr;
			device.CreateDomainShader((IntPtr)ptr2, shaderBytecode.Length, linkage, this);
		}
	}

	public DomainShader(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator DomainShader(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new DomainShader(nativePtr);
		}
		return null;
	}
}
