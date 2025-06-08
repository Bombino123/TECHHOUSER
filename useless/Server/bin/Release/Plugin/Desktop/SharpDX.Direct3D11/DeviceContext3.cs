using System;
using System.Runtime.InteropServices;
using SharpDX.Mathematics.Interop;

namespace SharpDX.Direct3D11;

[Guid("b4e3c01d-e79e-4637-91b2-510e9f4c9b8f")]
public class DeviceContext3 : DeviceContext2
{
	public RawBool HardwareProtectionState
	{
		get
		{
			GetHardwareProtectionState(out var hwProtectionEnableRef);
			return hwProtectionEnableRef;
		}
		set
		{
			SetHardwareProtectionState(value);
		}
	}

	public DeviceContext3(Device3 device)
		: base(IntPtr.Zero)
	{
		device.CreateDeferredContext3(0, this);
	}

	public DeviceContext3(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator DeviceContext3(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new DeviceContext3(nativePtr);
		}
		return null;
	}

	public unsafe void Flush1(ContextType contextType, IntPtr hEvent)
	{
		((delegate* unmanaged[Stdcall]<void*, int, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)144 * (nint)sizeof(void*))))(_nativePointer, (int)contextType, (void*)hEvent);
	}

	internal unsafe void SetHardwareProtectionState(RawBool hwProtectionEnable)
	{
		((delegate* unmanaged[Stdcall]<void*, RawBool, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)145 * (nint)sizeof(void*))))(_nativePointer, hwProtectionEnable);
	}

	internal unsafe void GetHardwareProtectionState(out RawBool hwProtectionEnableRef)
	{
		hwProtectionEnableRef = default(RawBool);
		fixed (RawBool* ptr = &hwProtectionEnableRef)
		{
			void* ptr2 = ptr;
			((delegate* unmanaged[Stdcall]<void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)146 * (nint)sizeof(void*))))(_nativePointer, ptr2);
		}
	}
}
