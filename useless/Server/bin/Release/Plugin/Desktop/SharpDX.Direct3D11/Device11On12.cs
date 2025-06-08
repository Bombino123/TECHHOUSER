using System;
using System.Runtime.InteropServices;

namespace SharpDX.Direct3D11;

[Guid("85611e73-70a9-490e-9614-a9e302777904")]
public class Device11On12 : ComObject
{
	public Device11On12(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator Device11On12(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new Device11On12(nativePtr);
		}
		return null;
	}

	public unsafe void CreateWrappedResource(IUnknown resource12Ref, D3D11ResourceFlags flags11Ref, int inState, int outState, Guid riid, out Resource resource11Out)
	{
		IntPtr zero = IntPtr.Zero;
		IntPtr zero2 = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<IUnknown>(resource12Ref);
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, int, int, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)3 * (nint)sizeof(void*))))(_nativePointer, (void*)zero, &flags11Ref, inState, outState, &riid, &zero2);
		if (zero2 != IntPtr.Zero)
		{
			resource11Out = new Resource(zero2);
		}
		else
		{
			resource11Out = null;
		}
		result.CheckError();
	}

	public unsafe void ReleaseWrappedResources(Resource[] resourcesOut, int numResources)
	{
		IntPtr* ptr = null;
		if (resourcesOut != null)
		{
			ptr = stackalloc IntPtr[resourcesOut.Length];
		}
		if (resourcesOut != null)
		{
			for (int i = 0; i < resourcesOut.Length; i++)
			{
				ptr[i] = CppObject.ToCallbackPtr<Resource>(resourcesOut[i]);
			}
		}
		((delegate* unmanaged[Stdcall]<void*, void*, int, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)4 * (nint)sizeof(void*))))(_nativePointer, ptr, numResources);
	}

	public unsafe void AcquireWrappedResources(Resource[] resourcesOut, int numResources)
	{
		IntPtr* ptr = null;
		if (resourcesOut != null)
		{
			ptr = stackalloc IntPtr[resourcesOut.Length];
		}
		if (resourcesOut != null)
		{
			for (int i = 0; i < resourcesOut.Length; i++)
			{
				ptr[i] = CppObject.ToCallbackPtr<Resource>(resourcesOut[i]);
			}
		}
		((delegate* unmanaged[Stdcall]<void*, void*, int, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)5 * (nint)sizeof(void*))))(_nativePointer, ptr, numResources);
	}

	public unsafe void ReleaseWrappedResources(ComArray<Resource> resourcesOut, int numResources)
	{
		void* nativePointer = _nativePointer;
		IntPtr intPtr = resourcesOut?.NativePointer ?? IntPtr.Zero;
		((delegate* unmanaged[Stdcall]<void*, void*, int, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)4 * (nint)sizeof(void*))))(nativePointer, (void*)intPtr, numResources);
	}

	private unsafe void ReleaseWrappedResources(IntPtr resourcesOut, int numResources)
	{
		((delegate* unmanaged[Stdcall]<void*, void*, int, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)4 * (nint)sizeof(void*))))(_nativePointer, (void*)resourcesOut, numResources);
	}

	public unsafe void AcquireWrappedResources(ComArray<Resource> resourcesOut, int numResources)
	{
		void* nativePointer = _nativePointer;
		IntPtr intPtr = resourcesOut?.NativePointer ?? IntPtr.Zero;
		((delegate* unmanaged[Stdcall]<void*, void*, int, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)5 * (nint)sizeof(void*))))(nativePointer, (void*)intPtr, numResources);
	}

	private unsafe void AcquireWrappedResources(IntPtr resourcesOut, int numResources)
	{
		((delegate* unmanaged[Stdcall]<void*, void*, int, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)5 * (nint)sizeof(void*))))(_nativePointer, (void*)resourcesOut, numResources);
	}
}
