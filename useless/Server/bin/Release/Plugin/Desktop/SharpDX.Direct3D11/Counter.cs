using System;
using System.Runtime.InteropServices;

namespace SharpDX.Direct3D11;

[Guid("6e8c49fb-a371-4770-b440-29086022b741")]
public class Counter : Asynchronous
{
	public CounterDescription Description
	{
		get
		{
			GetDescription(out var descRef);
			return descRef;
		}
	}

	public Counter(Device device, CounterDescription description)
		: base(IntPtr.Zero)
	{
		device.CreateCounter(description, this);
	}

	public Counter(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator Counter(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new Counter(nativePtr);
		}
		return null;
	}

	internal unsafe void GetDescription(out CounterDescription descRef)
	{
		descRef = default(CounterDescription);
		fixed (CounterDescription* ptr = &descRef)
		{
			void* ptr2 = ptr;
			((delegate* unmanaged[Stdcall]<void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)8 * (nint)sizeof(void*))))(_nativePointer, ptr2);
		}
	}
}
