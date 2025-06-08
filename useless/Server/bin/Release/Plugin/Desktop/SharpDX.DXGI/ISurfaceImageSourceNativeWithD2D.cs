using System;
using System.Runtime.InteropServices;
using SharpDX.Mathematics.Interop;

namespace SharpDX.DXGI;

[Guid("54298223-41e1-4a41-9c08-02e8256864a1")]
public class ISurfaceImageSourceNativeWithD2D : ComObject
{
	public IUnknown Device
	{
		set
		{
			SetDevice(value);
		}
	}

	public ISurfaceImageSourceNativeWithD2D(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator ISurfaceImageSourceNativeWithD2D(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new ISurfaceImageSourceNativeWithD2D(nativePtr);
		}
		return null;
	}

	internal unsafe void SetDevice(IUnknown device)
	{
		IntPtr zero = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<IUnknown>(device);
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)3 * (nint)sizeof(void*))))(_nativePointer, (void*)zero)).CheckError();
	}

	public unsafe void BeginDraw(RawRectangle updateRect, Guid iid, out IntPtr updateObject, out RawPoint offset)
	{
		offset = default(RawPoint);
		Result result;
		fixed (RawPoint* ptr = &offset)
		{
			void* ptr2 = ptr;
			fixed (IntPtr* ptr3 = &updateObject)
			{
				void* ptr4 = ptr3;
				result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)4 * (nint)sizeof(void*))))(_nativePointer, &updateRect, &iid, ptr4, ptr2);
			}
		}
		result.CheckError();
	}

	public unsafe void EndDraw()
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)5 * (nint)sizeof(void*))))(_nativePointer)).CheckError();
	}

	public unsafe void SuspendDraw()
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)6 * (nint)sizeof(void*))))(_nativePointer)).CheckError();
	}

	public unsafe void ResumeDraw()
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)7 * (nint)sizeof(void*))))(_nativePointer)).CheckError();
	}
}
