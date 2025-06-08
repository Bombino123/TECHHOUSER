using System;
using System.Runtime.InteropServices;

namespace SharpDX.Direct3D11;

[Guid("d6c00747-87b7-425e-b84d-44d108560afd")]
public class Query : Asynchronous
{
	public QueryDescription Description
	{
		get
		{
			GetDescription(out var descRef);
			return descRef;
		}
	}

	public Query(Device device, QueryDescription description)
		: base(IntPtr.Zero)
	{
		device.CreateQuery(description, this);
	}

	public Query(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator Query(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new Query(nativePtr);
		}
		return null;
	}

	internal unsafe void GetDescription(out QueryDescription descRef)
	{
		descRef = default(QueryDescription);
		fixed (QueryDescription* ptr = &descRef)
		{
			void* ptr2 = ptr;
			((delegate* unmanaged[Stdcall]<void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)8 * (nint)sizeof(void*))))(_nativePointer, ptr2);
		}
	}
}
