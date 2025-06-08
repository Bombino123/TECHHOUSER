using System;
using System.Runtime.InteropServices;
using SharpDX.Mathematics.Interop;

namespace SharpDX.DXGI;

[Guid("4AE63092-6327-4c1b-80AE-BFE12EA32B86")]
public class Surface1 : Surface
{
	public void ReleaseDC()
	{
		ReleaseDC_(null);
	}

	public void ReleaseDC(RawRectangle dirtyRect)
	{
		ReleaseDC_(dirtyRect);
	}

	public Surface1(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator Surface1(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new Surface1(nativePtr);
		}
		return null;
	}

	public unsafe IntPtr GetDC(RawBool discard)
	{
		IntPtr result = default(IntPtr);
		((Result)((delegate* unmanaged[Stdcall]<void*, RawBool, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)11 * (nint)sizeof(void*))))(_nativePointer, discard, &result)).CheckError();
		return result;
	}

	internal unsafe void ReleaseDC_(RawRectangle? dirtyRectRef)
	{
		RawRectangle value = default(RawRectangle);
		if (dirtyRectRef.HasValue)
		{
			value = dirtyRectRef.Value;
		}
		void* nativePointer = _nativePointer;
		RawRectangle* intPtr = ((!dirtyRectRef.HasValue) ? null : (&value));
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)12 * (nint)sizeof(void*))))(nativePointer, intPtr)).CheckError();
	}
}
