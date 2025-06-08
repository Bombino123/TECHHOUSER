using System;
using System.Runtime.InteropServices;

namespace SharpDX.DXGI;

[Guid("aec22fb8-76f3-4639-9be0-28eb43a67a2e")]
public class DXGIObject : ComObject
{
	public T GetParent<T>() where T : ComObject
	{
		GetParent(Utilities.GetGuidFromType(typeof(T)), out var parentOut);
		return CppObject.FromPointer<T>(parentOut);
	}

	public DXGIObject(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator DXGIObject(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new DXGIObject(nativePtr);
		}
		return null;
	}

	public unsafe void SetPrivateData(Guid name, int dataSize, IntPtr dataRef)
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, int, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)3 * (nint)sizeof(void*))))(_nativePointer, &name, dataSize, (void*)dataRef)).CheckError();
	}

	public unsafe void SetPrivateDataInterface(Guid name, IUnknown unknownRef)
	{
		IntPtr zero = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<IUnknown>(unknownRef);
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)4 * (nint)sizeof(void*))))(_nativePointer, &name, (void*)zero)).CheckError();
	}

	public unsafe Result GetPrivateData(Guid name, ref int dataSizeRef, IntPtr dataRef)
	{
		Result result;
		fixed (int* ptr = &dataSizeRef)
		{
			void* ptr2 = ptr;
			result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)5 * (nint)sizeof(void*))))(_nativePointer, &name, ptr2, (void*)dataRef);
		}
		return result;
	}

	public unsafe void GetParent(Guid riid, out IntPtr parentOut)
	{
		Result result;
		fixed (IntPtr* ptr = &parentOut)
		{
			void* ptr2 = ptr;
			result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)6 * (nint)sizeof(void*))))(_nativePointer, &riid, ptr2);
		}
		result.CheckError();
	}
}
