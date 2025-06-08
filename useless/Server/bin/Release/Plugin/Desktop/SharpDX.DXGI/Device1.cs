using System;
using System.Runtime.InteropServices;

namespace SharpDX.DXGI;

[Guid("77db970f-6276-48ba-ba28-070143b4392c")]
public class Device1 : Device
{
	public int MaximumFrameLatency
	{
		get
		{
			GetMaximumFrameLatency(out var maxLatencyRef);
			return maxLatencyRef;
		}
		set
		{
			SetMaximumFrameLatency(value);
		}
	}

	public Device1(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator Device1(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new Device1(nativePtr);
		}
		return null;
	}

	internal unsafe void SetMaximumFrameLatency(int maxLatency)
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, int, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)12 * (nint)sizeof(void*))))(_nativePointer, maxLatency)).CheckError();
	}

	internal unsafe void GetMaximumFrameLatency(out int maxLatencyRef)
	{
		Result result;
		fixed (int* ptr = &maxLatencyRef)
		{
			void* ptr2 = ptr;
			result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)13 * (nint)sizeof(void*))))(_nativePointer, ptr2);
		}
		result.CheckError();
	}
}
