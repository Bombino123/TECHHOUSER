using System;
using System.Runtime.InteropServices;
using SharpDX.Direct3D;

namespace SharpDX.Direct3D11;

[Guid("1841e5c8-16b0-489b-bcc8-44cfb0d5deae")]
public class DeviceChild : ComObject
{
	protected internal Device Device__;

	public unsafe string DebugName
	{
		get
		{
			byte* ptr = stackalloc byte[1024];
			int dataSizeRef = 1023;
			if (GetPrivateData(CommonGuid.DebugObjectName, ref dataSizeRef, new IntPtr(ptr)).Failure)
			{
				return string.Empty;
			}
			ptr[dataSizeRef] = 0;
			return Marshal.PtrToStringAnsi(new IntPtr(ptr));
		}
		set
		{
			if (string.IsNullOrEmpty(value))
			{
				SetPrivateData(CommonGuid.DebugObjectName, 0, IntPtr.Zero);
				return;
			}
			IntPtr intPtr = Utilities.StringToHGlobalAnsi(value);
			SetPrivateData(CommonGuid.DebugObjectName, value.Length, intPtr);
			Marshal.FreeHGlobal(intPtr);
		}
	}

	public Device Device
	{
		get
		{
			if (Device__ == null)
			{
				GetDevice(out Device__);
			}
			return Device__;
		}
	}

	protected override void NativePointerUpdated(IntPtr oldNativePointer)
	{
		DisposeDevice();
		base.NativePointerUpdated(oldNativePointer);
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			DisposeDevice();
		}
		base.Dispose(disposing);
	}

	private void DisposeDevice()
	{
		if (Device__ != null)
		{
			((IUnknown)Device__).Release();
			Device__ = null;
		}
	}

	public DeviceChild(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator DeviceChild(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new DeviceChild(nativePtr);
		}
		return null;
	}

	internal unsafe void GetDevice(out Device deviceOut)
	{
		IntPtr zero = IntPtr.Zero;
		((delegate* unmanaged[Stdcall]<void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)3 * (nint)sizeof(void*))))(_nativePointer, &zero);
		if (zero != IntPtr.Zero)
		{
			deviceOut = new Device(zero);
		}
		else
		{
			deviceOut = null;
		}
	}

	public unsafe Result GetPrivateData(Guid guid, ref int dataSizeRef, IntPtr dataRef)
	{
		Result result;
		fixed (int* ptr = &dataSizeRef)
		{
			void* ptr2 = ptr;
			result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)4 * (nint)sizeof(void*))))(_nativePointer, &guid, ptr2, (void*)dataRef);
		}
		return result;
	}

	public unsafe void SetPrivateData(Guid guid, int dataSize, IntPtr dataRef)
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, int, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)5 * (nint)sizeof(void*))))(_nativePointer, &guid, dataSize, (void*)dataRef)).CheckError();
	}

	public unsafe void SetPrivateDataInterface(Guid guid, IUnknown dataRef)
	{
		IntPtr zero = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<IUnknown>(dataRef);
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)6 * (nint)sizeof(void*))))(_nativePointer, &guid, (void*)zero)).CheckError();
	}
}
