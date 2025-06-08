using System;
using System.Runtime.InteropServices;

namespace SharpDX.Direct3D11;

[Guid("dfdba067-0b8d-4865-875b-d7b4516cc164")]
public class RenderTargetView : ResourceView
{
	public RenderTargetViewDescription Description
	{
		get
		{
			GetDescription(out var descRef);
			return descRef;
		}
	}

	public RenderTargetView(Device device, Resource resource)
		: base(IntPtr.Zero)
	{
		device.CreateRenderTargetView(resource, null, this);
	}

	public RenderTargetView(Device device, Resource resource, RenderTargetViewDescription description)
		: base(IntPtr.Zero)
	{
		device.CreateRenderTargetView(resource, description, this);
	}

	public RenderTargetView(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator RenderTargetView(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new RenderTargetView(nativePtr);
		}
		return null;
	}

	internal unsafe void GetDescription(out RenderTargetViewDescription descRef)
	{
		descRef = default(RenderTargetViewDescription);
		fixed (RenderTargetViewDescription* ptr = &descRef)
		{
			void* ptr2 = ptr;
			((delegate* unmanaged[Stdcall]<void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)8 * (nint)sizeof(void*))))(_nativePointer, ptr2);
		}
	}
}
