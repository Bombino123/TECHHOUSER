using System;
using System.Runtime.InteropServices;
using SharpDX.Mathematics.Interop;

namespace SharpDX.DXGI;

[Guid("f2e9edc1-d307-4525-9886-0fafaa44163c")]
public class ISurfaceImageSourceNative : ComObject
{
	public Device Device
	{
		set
		{
			SetDevice(value);
		}
	}

	public ISurfaceImageSourceNative(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator ISurfaceImageSourceNative(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new ISurfaceImageSourceNative(nativePtr);
		}
		return null;
	}

	internal unsafe void SetDevice(Device device)
	{
		IntPtr zero = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<Device>(device);
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)3 * (nint)sizeof(void*))))(_nativePointer, (void*)zero)).CheckError();
	}

	public unsafe Surface BeginDraw(RawRectangle updateRect, out RawPoint offset)
	{
		IntPtr zero = IntPtr.Zero;
		offset = default(RawPoint);
		Result result;
		fixed (RawPoint* ptr = &offset)
		{
			void* ptr2 = ptr;
			result = ((delegate* unmanaged[Stdcall]<void*, RawRectangle, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)4 * (nint)sizeof(void*))))(_nativePointer, updateRect, &zero, ptr2);
		}
		Surface result2 = ((!(zero != IntPtr.Zero)) ? null : new Surface(zero));
		result.CheckError();
		return result2;
	}

	public unsafe void EndDraw()
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)5 * (nint)sizeof(void*))))(_nativePointer)).CheckError();
	}
}
