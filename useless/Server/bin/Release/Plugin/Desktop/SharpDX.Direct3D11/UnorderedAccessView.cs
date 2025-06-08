using System;
using System.Runtime.InteropServices;

namespace SharpDX.Direct3D11;

[Guid("28acf509-7f5c-48f6-8611-f316010a6380")]
public class UnorderedAccessView : ResourceView
{
	public UnorderedAccessViewDescription Description
	{
		get
		{
			GetDescription(out var descRef);
			return descRef;
		}
	}

	public UnorderedAccessView(Device device, Resource resource)
		: base(IntPtr.Zero)
	{
		device.CreateUnorderedAccessView(resource, null, this);
	}

	public UnorderedAccessView(Device device, Resource resource, UnorderedAccessViewDescription description)
		: base(IntPtr.Zero)
	{
		device.CreateUnorderedAccessView(resource, description, this);
	}

	public UnorderedAccessView(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator UnorderedAccessView(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new UnorderedAccessView(nativePtr);
		}
		return null;
	}

	internal unsafe void GetDescription(out UnorderedAccessViewDescription descRef)
	{
		descRef = default(UnorderedAccessViewDescription);
		fixed (UnorderedAccessViewDescription* ptr = &descRef)
		{
			void* ptr2 = ptr;
			((delegate* unmanaged[Stdcall]<void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)8 * (nint)sizeof(void*))))(_nativePointer, ptr2);
		}
	}
}
