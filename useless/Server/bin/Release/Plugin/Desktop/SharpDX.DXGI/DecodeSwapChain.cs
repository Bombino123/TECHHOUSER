using System;
using System.Runtime.InteropServices;
using SharpDX.Mathematics.Interop;

namespace SharpDX.DXGI;

[Guid("2633066b-4514-4c7a-8fd8-12ea98059d18")]
public class DecodeSwapChain : ComObject
{
	public RawRectangle SourceRect
	{
		get
		{
			GetSourceRect(out var rectRef);
			return rectRef;
		}
		set
		{
			SetSourceRect(value);
		}
	}

	public RawRectangle TargetRect
	{
		get
		{
			GetTargetRect(out var rectRef);
			return rectRef;
		}
		set
		{
			SetTargetRect(value);
		}
	}

	public MultiplaneOverlayYCbCrFlags ColorSpace
	{
		get
		{
			return GetColorSpace();
		}
		set
		{
			SetColorSpace(value);
		}
	}

	public DecodeSwapChain(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator DecodeSwapChain(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new DecodeSwapChain(nativePtr);
		}
		return null;
	}

	public unsafe void PresentBuffer(int bufferToPresent, int syncInterval, int flags)
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, int, int, int, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)3 * (nint)sizeof(void*))))(_nativePointer, bufferToPresent, syncInterval, flags)).CheckError();
	}

	internal unsafe void SetSourceRect(RawRectangle rectRef)
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)4 * (nint)sizeof(void*))))(_nativePointer, &rectRef)).CheckError();
	}

	internal unsafe void SetTargetRect(RawRectangle rectRef)
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)5 * (nint)sizeof(void*))))(_nativePointer, &rectRef)).CheckError();
	}

	public unsafe void SetDestSize(int width, int height)
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, int, int, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)6 * (nint)sizeof(void*))))(_nativePointer, width, height)).CheckError();
	}

	internal unsafe void GetSourceRect(out RawRectangle rectRef)
	{
		rectRef = default(RawRectangle);
		Result result;
		fixed (RawRectangle* ptr = &rectRef)
		{
			void* ptr2 = ptr;
			result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)7 * (nint)sizeof(void*))))(_nativePointer, ptr2);
		}
		result.CheckError();
	}

	internal unsafe void GetTargetRect(out RawRectangle rectRef)
	{
		rectRef = default(RawRectangle);
		Result result;
		fixed (RawRectangle* ptr = &rectRef)
		{
			void* ptr2 = ptr;
			result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)8 * (nint)sizeof(void*))))(_nativePointer, ptr2);
		}
		result.CheckError();
	}

	public unsafe void GetDestSize(out int widthRef, out int heightRef)
	{
		Result result;
		fixed (int* ptr = &heightRef)
		{
			void* ptr2 = ptr;
			fixed (int* ptr3 = &widthRef)
			{
				void* ptr4 = ptr3;
				result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)9 * (nint)sizeof(void*))))(_nativePointer, ptr4, ptr2);
			}
		}
		result.CheckError();
	}

	internal unsafe void SetColorSpace(MultiplaneOverlayYCbCrFlags colorSpace)
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, int, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)10 * (nint)sizeof(void*))))(_nativePointer, (int)colorSpace)).CheckError();
	}

	internal unsafe MultiplaneOverlayYCbCrFlags GetColorSpace()
	{
		return ((delegate* unmanaged[Stdcall]<void*, MultiplaneOverlayYCbCrFlags>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)11 * (nint)sizeof(void*))))(_nativePointer);
	}
}
