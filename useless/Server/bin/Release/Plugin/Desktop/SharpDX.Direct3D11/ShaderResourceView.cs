using System;
using System.Runtime.InteropServices;

namespace SharpDX.Direct3D11;

[Guid("b0e06fe0-8192-4e1a-b1ca-36d7414710b2")]
public class ShaderResourceView : ResourceView
{
	public ShaderResourceViewDescription Description
	{
		get
		{
			GetDescription(out var descRef);
			return descRef;
		}
	}

	public ShaderResourceView(Device device, Resource resource)
		: base(IntPtr.Zero)
	{
		device.CreateShaderResourceView(resource, null, this);
	}

	public ShaderResourceView(Device device, Resource resource, ShaderResourceViewDescription description)
		: base(IntPtr.Zero)
	{
		device.CreateShaderResourceView(resource, description, this);
	}

	public ShaderResourceView(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator ShaderResourceView(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new ShaderResourceView(nativePtr);
		}
		return null;
	}

	internal unsafe void GetDescription(out ShaderResourceViewDescription descRef)
	{
		descRef = default(ShaderResourceViewDescription);
		fixed (ShaderResourceViewDescription* ptr = &descRef)
		{
			void* ptr2 = ptr;
			((delegate* unmanaged[Stdcall]<void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)8 * (nint)sizeof(void*))))(_nativePointer, ptr2);
		}
	}
}
