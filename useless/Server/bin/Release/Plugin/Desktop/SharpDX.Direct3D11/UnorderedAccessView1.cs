using System;
using System.Runtime.InteropServices;

namespace SharpDX.Direct3D11;

[Guid("7b3b6153-a886-4544-ab37-6537c8500403")]
public class UnorderedAccessView1 : UnorderedAccessView
{
	public UnorderedAccessViewDescription1 Description1
	{
		get
		{
			GetDescription1(out var desc1Ref);
			return desc1Ref;
		}
	}

	public UnorderedAccessView1(Device3 device, Resource resource)
		: base(IntPtr.Zero)
	{
		device.CreateUnorderedAccessView1(resource, null, this);
	}

	public UnorderedAccessView1(Device3 device, Resource resource, UnorderedAccessViewDescription1 description)
		: base(IntPtr.Zero)
	{
		device.CreateUnorderedAccessView1(resource, description, this);
	}

	public UnorderedAccessView1(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator UnorderedAccessView1(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new UnorderedAccessView1(nativePtr);
		}
		return null;
	}

	internal unsafe void GetDescription1(out UnorderedAccessViewDescription1 desc1Ref)
	{
		desc1Ref = default(UnorderedAccessViewDescription1);
		fixed (UnorderedAccessViewDescription1* ptr = &desc1Ref)
		{
			void* ptr2 = ptr;
			((delegate* unmanaged[Stdcall]<void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)9 * (nint)sizeof(void*))))(_nativePointer, ptr2);
		}
	}
}
