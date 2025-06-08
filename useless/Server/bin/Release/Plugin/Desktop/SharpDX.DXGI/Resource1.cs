using System;
using System.Runtime.InteropServices;
using SharpDX.Win32;

namespace SharpDX.DXGI;

[Guid("30961379-4609-4a41-998e-54fe567ee0c1")]
public class Resource1 : Resource
{
	public IntPtr CreateSharedHandle(string name, SharedResourceFlags dwAccess, SecurityAttributes? attributesRef = null)
	{
		return CreateSharedHandle(attributesRef, dwAccess, name);
	}

	public Resource1(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator Resource1(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new Resource1(nativePtr);
		}
		return null;
	}

	internal unsafe void CreateSubresourceSurface(int index, Surface2 surfaceOut)
	{
		IntPtr zero = IntPtr.Zero;
		Result result = ((delegate* unmanaged[Stdcall]<void*, int, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)12 * (nint)sizeof(void*))))(_nativePointer, index, &zero);
		surfaceOut.NativePointer = zero;
		result.CheckError();
	}

	internal unsafe IntPtr CreateSharedHandle(SecurityAttributes? attributesRef, SharedResourceFlags dwAccess, string lpName)
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
			result2 = ((delegate* unmanaged[Stdcall]<void*, void*, int, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)13 * (nint)sizeof(void*))))(nativePointer, intPtr, (int)dwAccess, ptr, &result);
		}
		result2.CheckError();
		return result;
	}
}
