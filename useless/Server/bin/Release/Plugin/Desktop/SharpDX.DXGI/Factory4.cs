using System;
using System.Runtime.InteropServices;

namespace SharpDX.DXGI;

[Guid("1bc6ea02-ef36-464f-bf0c-21ca39e5168a")]
public class Factory4 : Factory3
{
	public Factory4()
		: this(IntPtr.Zero)
	{
		DXGI.CreateDXGIFactory1(Utilities.GetGuidFromType(GetType()), out var factoryOut);
		base.NativePointer = factoryOut;
	}

	public Adapter GetWarpAdapter()
	{
		EnumWarpAdapter(Utilities.GetGuidFromType(typeof(Adapter)), out var vAdapterOut);
		return new Adapter(vAdapterOut);
	}

	public Adapter GetAdapterByLuid(long adapterLuid)
	{
		EnumAdapterByLuid(adapterLuid, Utilities.GetGuidFromType(typeof(Adapter)), out var vAdapterOut);
		return new Adapter(vAdapterOut);
	}

	public Factory4(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator Factory4(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new Factory4(nativePtr);
		}
		return null;
	}

	private unsafe void EnumAdapterByLuid(long adapterLuid, Guid riid, out IntPtr vAdapterOut)
	{
		Result result;
		fixed (IntPtr* ptr = &vAdapterOut)
		{
			void* ptr2 = ptr;
			result = ((delegate* unmanaged[Stdcall]<void*, long, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)26 * (nint)sizeof(void*))))(_nativePointer, adapterLuid, &riid, ptr2);
		}
		result.CheckError();
	}

	private unsafe void EnumWarpAdapter(Guid riid, out IntPtr vAdapterOut)
	{
		Result result;
		fixed (IntPtr* ptr = &vAdapterOut)
		{
			void* ptr2 = ptr;
			result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)27 * (nint)sizeof(void*))))(_nativePointer, &riid, ptr2);
		}
		result.CheckError();
	}
}
