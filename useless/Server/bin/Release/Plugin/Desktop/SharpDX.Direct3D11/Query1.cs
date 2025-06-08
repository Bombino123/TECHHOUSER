using System;
using System.Runtime.InteropServices;

namespace SharpDX.Direct3D11;

[Guid("631b4766-36dc-461d-8db6-c47e13e60916")]
public class Query1 : Query
{
	public QueryDescription1 Description1
	{
		get
		{
			GetDescription1(out var desc1Ref);
			return desc1Ref;
		}
	}

	public Query1(Device3 device, QueryDescription1 description)
		: base(IntPtr.Zero)
	{
		device.CreateQuery1(description, this);
	}

	public Query1(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator Query1(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new Query1(nativePtr);
		}
		return null;
	}

	internal unsafe void GetDescription1(out QueryDescription1 desc1Ref)
	{
		desc1Ref = default(QueryDescription1);
		fixed (QueryDescription1* ptr = &desc1Ref)
		{
			void* ptr2 = ptr;
			((delegate* unmanaged[Stdcall]<void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)9 * (nint)sizeof(void*))))(_nativePointer, ptr2);
		}
	}
}
