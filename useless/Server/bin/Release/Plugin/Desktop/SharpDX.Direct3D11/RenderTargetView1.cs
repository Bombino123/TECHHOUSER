using System;
using System.Runtime.InteropServices;

namespace SharpDX.Direct3D11;

[Guid("ffbe2e23-f011-418a-ac56-5ceed7c5b94b")]
public class RenderTargetView1 : RenderTargetView
{
	public RenderTargetViewDescription1 Description1
	{
		get
		{
			GetDescription1(out var desc1Ref);
			return desc1Ref;
		}
	}

	public RenderTargetView1(Device3 device, Resource resource)
		: base(IntPtr.Zero)
	{
		device.CreateRenderTargetView1(resource, null, this);
	}

	public RenderTargetView1(Device3 device, Resource resource, RenderTargetViewDescription1 description)
		: base(IntPtr.Zero)
	{
		device.CreateRenderTargetView1(resource, description, this);
	}

	public RenderTargetView1(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator RenderTargetView1(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new RenderTargetView1(nativePtr);
		}
		return null;
	}

	internal unsafe void GetDescription1(out RenderTargetViewDescription1 desc1Ref)
	{
		desc1Ref = default(RenderTargetViewDescription1);
		fixed (RenderTargetViewDescription1* ptr = &desc1Ref)
		{
			void* ptr2 = ptr;
			((delegate* unmanaged[Stdcall]<void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)9 * (nint)sizeof(void*))))(_nativePointer, ptr2);
		}
	}
}
