using System;
using System.Runtime.InteropServices;
using SharpDX.Win32;

namespace SharpDX.Direct3D11;

[Guid("affde9d1-1df7-4bb7-8a34-0f46251dab80")]
public class Fence : DeviceChild
{
	public long CompletedValue => GetCompletedValue();

	public Fence(Device5 device, long initialValue, FenceFlags flags)
		: base(IntPtr.Zero)
	{
		device.CreateFence(initialValue, flags, Utilities.GetGuidFromType(typeof(Fence)), this);
	}

	public Fence(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator Fence(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new Fence(nativePtr);
		}
		return null;
	}

	public unsafe IntPtr CreateSharedHandle(SecurityAttributes? attributesRef, int dwAccess, string lpName)
	{
		SecurityAttributes value = default(SecurityAttributes);
		if (attributesRef.HasValue)
		{
			value = attributesRef.Value;
		}
		Result result2;
		IntPtr result = default(IntPtr);
		fixed (char* ptr = lpName)
		{
			void* nativePointer = _nativePointer;
			SecurityAttributes* intPtr = ((!attributesRef.HasValue) ? null : (&value));
			result2 = ((delegate* unmanaged[Stdcall]<void*, void*, int, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)7 * (nint)sizeof(void*))))(nativePointer, intPtr, dwAccess, ptr, &result);
		}
		result2.CheckError();
		return result;
	}

	internal unsafe long GetCompletedValue()
	{
		return ((delegate* unmanaged[Stdcall]<void*, long>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)8 * (nint)sizeof(void*))))(_nativePointer);
	}

	public unsafe void SetEventOnCompletion(long value, IntPtr hEvent)
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, long, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)9 * (nint)sizeof(void*))))(_nativePointer, value, (void*)hEvent)).CheckError();
	}
}
