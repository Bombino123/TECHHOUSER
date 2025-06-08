using System;
using System.Runtime.InteropServices;

namespace SharpDX.Direct3D11;

[Guid("91308b87-9040-411d-8c67-c39253ce3802")]
public class ShaderResourceView1 : ShaderResourceView
{
	public ShaderResourceViewDescription1 Description1
	{
		get
		{
			GetDescription1(out var desc1Ref);
			return desc1Ref;
		}
	}

	public ShaderResourceView1(Device3 device, Resource resource)
		: base(IntPtr.Zero)
	{
		device.CreateShaderResourceView1(resource, null, this);
	}

	public ShaderResourceView1(Device3 device, Resource resource, ShaderResourceViewDescription1 description)
		: base(IntPtr.Zero)
	{
		device.CreateShaderResourceView1(resource, description, this);
	}

	public ShaderResourceView1(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator ShaderResourceView1(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new ShaderResourceView1(nativePtr);
		}
		return null;
	}

	internal unsafe void GetDescription1(out ShaderResourceViewDescription1 desc1Ref)
	{
		desc1Ref = default(ShaderResourceViewDescription1);
		fixed (ShaderResourceViewDescription1* ptr = &desc1Ref)
		{
			void* ptr2 = ptr;
			((delegate* unmanaged[Stdcall]<void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)9 * (nint)sizeof(void*))))(_nativePointer, ptr2);
		}
	}
}
