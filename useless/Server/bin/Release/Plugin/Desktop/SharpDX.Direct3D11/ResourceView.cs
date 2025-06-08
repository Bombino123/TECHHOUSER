using System;
using System.Runtime.InteropServices;

namespace SharpDX.Direct3D11;

[Guid("839d1216-bb2e-412b-b7f4-a9dbebe08ed1")]
public class ResourceView : DeviceChild
{
	public Resource Resource
	{
		get
		{
			GetResource(out var resourceOut);
			return new Resource(resourceOut);
		}
	}

	public T ResourceAs<T>() where T : Resource
	{
		GetResource(out var resourceOut);
		return ComObject.As<T>(resourceOut);
	}

	public ResourceView(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator ResourceView(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new ResourceView(nativePtr);
		}
		return null;
	}

	internal unsafe void GetResource(out IntPtr resourceOut)
	{
		fixed (IntPtr* ptr = &resourceOut)
		{
			void* ptr2 = ptr;
			((delegate* unmanaged[Stdcall]<void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)7 * (nint)sizeof(void*))))(_nativePointer, ptr2);
		}
	}
}
