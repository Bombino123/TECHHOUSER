using System;
using System.Runtime.InteropServices;

namespace SharpDX.DXGI;

[Guid("aba496dd-b617-4cb8-a866-bc44d7eb1fa2")]
public class Surface2 : Surface1
{
	public Surface2(Resource1 resource, int index)
		: base(IntPtr.Zero)
	{
		resource.CreateSubresourceSurface(index, this);
	}

	public Surface2(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator Surface2(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new Surface2(nativePtr);
		}
		return null;
	}

	public unsafe void GetResource(Guid riid, out IntPtr parentResourceOut, out int subresourceIndexRef)
	{
		Result result;
		fixed (int* ptr = &subresourceIndexRef)
		{
			void* ptr2 = ptr;
			fixed (IntPtr* ptr3 = &parentResourceOut)
			{
				void* ptr4 = ptr3;
				result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)13 * (nint)sizeof(void*))))(_nativePointer, &riid, ptr4, ptr2);
			}
		}
		result.CheckError();
	}
}
