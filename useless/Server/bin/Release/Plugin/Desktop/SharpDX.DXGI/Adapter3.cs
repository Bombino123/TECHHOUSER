using System;
using System.Runtime.InteropServices;

namespace SharpDX.DXGI;

[Guid("645967A4-1392-4310-A798-8053CE3E93FD")]
public class Adapter3 : Adapter2
{
	public Adapter3(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator Adapter3(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new Adapter3(nativePtr);
		}
		return null;
	}

	public unsafe int RegisterHardwareContentProtectionTeardownStatusEvent(IntPtr hEvent)
	{
		int result = default(int);
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)12 * (nint)sizeof(void*))))(_nativePointer, (void*)hEvent, &result)).CheckError();
		return result;
	}

	public unsafe void UnregisterHardwareContentProtectionTeardownStatus(int dwCookie)
	{
		((delegate* unmanaged[Stdcall]<void*, int, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)13 * (nint)sizeof(void*))))(_nativePointer, dwCookie);
	}

	public unsafe QueryVideoMemoryInformation QueryVideoMemoryInfo(int nodeIndex, MemorySegmentGroup memorySegmentGroup)
	{
		QueryVideoMemoryInformation result = default(QueryVideoMemoryInformation);
		((Result)((delegate* unmanaged[Stdcall]<void*, int, int, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)14 * (nint)sizeof(void*))))(_nativePointer, nodeIndex, (int)memorySegmentGroup, &result)).CheckError();
		return result;
	}

	public unsafe void SetVideoMemoryReservation(int nodeIndex, MemorySegmentGroup memorySegmentGroup, long reservation)
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, int, int, long, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)15 * (nint)sizeof(void*))))(_nativePointer, nodeIndex, (int)memorySegmentGroup, reservation)).CheckError();
	}

	public unsafe int RegisterVideoMemoryBudgetChangeNotificationEvent(IntPtr hEvent)
	{
		int result = default(int);
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)16 * (nint)sizeof(void*))))(_nativePointer, (void*)hEvent, &result)).CheckError();
		return result;
	}

	public unsafe void UnregisterVideoMemoryBudgetChangeNotification(int dwCookie)
	{
		((delegate* unmanaged[Stdcall]<void*, int, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)17 * (nint)sizeof(void*))))(_nativePointer, dwCookie);
	}
}
