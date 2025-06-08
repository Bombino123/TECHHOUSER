using System;
using System.Runtime.InteropServices;

namespace SharpDX.Direct3D11;

[Guid("9fdac92a-1876-48c3-afad-25b94f84a9b6")]
public class DepthStencilView : ResourceView
{
	public DepthStencilViewDescription Description
	{
		get
		{
			GetDescription(out var descRef);
			return descRef;
		}
	}

	public DepthStencilView(Device device, Resource resource)
		: base(IntPtr.Zero)
	{
		device.CreateDepthStencilView(resource, null, this);
	}

	public DepthStencilView(Device device, Resource resource, DepthStencilViewDescription description)
		: base(IntPtr.Zero)
	{
		device.CreateDepthStencilView(resource, description, this);
	}

	public DepthStencilView(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator DepthStencilView(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new DepthStencilView(nativePtr);
		}
		return null;
	}

	internal unsafe void GetDescription(out DepthStencilViewDescription descRef)
	{
		descRef = default(DepthStencilViewDescription);
		fixed (DepthStencilViewDescription* ptr = &descRef)
		{
			void* ptr2 = ptr;
			((delegate* unmanaged[Stdcall]<void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)8 * (nint)sizeof(void*))))(_nativePointer, ptr2);
		}
	}
}
