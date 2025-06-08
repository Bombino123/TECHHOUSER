using System;
using System.Runtime.InteropServices;

namespace SharpDX.DXGI;

[Guid("dd95b90b-f05f-4f6a-bd65-25bfb264bd84")]
public class SwapChainMedia : ComObject
{
	public FrameStatisticsMedia FrameStatisticsMedia
	{
		get
		{
			GetFrameStatisticsMedia(out var statsRef);
			return statsRef;
		}
	}

	public int PresentDuration
	{
		set
		{
			SetPresentDuration(value);
		}
	}

	public SwapChainMedia(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator SwapChainMedia(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new SwapChainMedia(nativePtr);
		}
		return null;
	}

	internal unsafe void GetFrameStatisticsMedia(out FrameStatisticsMedia statsRef)
	{
		statsRef = default(FrameStatisticsMedia);
		Result result;
		fixed (FrameStatisticsMedia* ptr = &statsRef)
		{
			void* ptr2 = ptr;
			result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)3 * (nint)sizeof(void*))))(_nativePointer, ptr2);
		}
		result.CheckError();
	}

	internal unsafe void SetPresentDuration(int duration)
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, int, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)4 * (nint)sizeof(void*))))(_nativePointer, duration)).CheckError();
	}

	public unsafe void CheckPresentDurationSupport(int desiredPresentDuration, out int closestSmallerPresentDurationRef, out int closestLargerPresentDurationRef)
	{
		Result result;
		fixed (int* ptr = &closestLargerPresentDurationRef)
		{
			void* ptr2 = ptr;
			fixed (int* ptr3 = &closestSmallerPresentDurationRef)
			{
				void* ptr4 = ptr3;
				result = ((delegate* unmanaged[Stdcall]<void*, int, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)5 * (nint)sizeof(void*))))(_nativePointer, desiredPresentDuration, ptr4, ptr2);
			}
		}
		result.CheckError();
	}
}
