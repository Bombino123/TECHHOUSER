using System;
using System.Runtime.InteropServices;

namespace SharpDX.DXGI;

[Guid("54ec77fa-1377-44e6-8c32-88fd5f44c84c")]
public class Device : DXGIObject
{
	public Adapter Adapter
	{
		get
		{
			GetAdapter(out var adapterRef);
			return adapterRef;
		}
	}

	public int GPUThreadPriority
	{
		get
		{
			GetGPUThreadPriority(out var priorityRef);
			return priorityRef;
		}
		set
		{
			SetGPUThreadPriority(value);
		}
	}

	public Residency[] QueryResourceResidency(params ComObject[] comObjects)
	{
		int num = comObjects.Length;
		Residency[] array = new Residency[num];
		QueryResourceResidency(comObjects, array, num);
		return array;
	}

	public Device(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator Device(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new Device(nativePtr);
		}
		return null;
	}

	internal unsafe void GetAdapter(out Adapter adapterRef)
	{
		IntPtr zero = IntPtr.Zero;
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)7 * (nint)sizeof(void*))))(_nativePointer, &zero);
		if (zero != IntPtr.Zero)
		{
			adapterRef = new Adapter(zero);
		}
		else
		{
			adapterRef = null;
		}
		result.CheckError();
	}

	internal unsafe void CreateSurface(ref SurfaceDescription descRef, int numSurfaces, int usage, SharedResource? sharedResourceRef, out Surface surfaceOut)
	{
		IntPtr zero = IntPtr.Zero;
		SharedResource value = default(SharedResource);
		if (sharedResourceRef.HasValue)
		{
			value = sharedResourceRef.Value;
		}
		Result result;
		fixed (SurfaceDescription* ptr = &descRef)
		{
			void* ptr2 = ptr;
			void* nativePointer = _nativePointer;
			SharedResource* intPtr = ((!sharedResourceRef.HasValue) ? null : (&value));
			result = ((delegate* unmanaged[Stdcall]<void*, void*, int, int, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)8 * (nint)sizeof(void*))))(nativePointer, ptr2, numSurfaces, usage, intPtr, &zero);
		}
		if (zero != IntPtr.Zero)
		{
			surfaceOut = new Surface(zero);
		}
		else
		{
			surfaceOut = null;
		}
		result.CheckError();
	}

	internal unsafe void QueryResourceResidency(IUnknown[] resourcesOut, Residency[] residencyStatusRef, int numResources)
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
				ptr[i] = CppObject.ToCallbackPtr<IUnknown>(resourcesOut[i]);
			}
		}
		Result result;
		fixed (Residency* ptr2 = residencyStatusRef)
		{
			void* ptr3 = ptr2;
			result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, int, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)9 * (nint)sizeof(void*))))(_nativePointer, ptr, ptr3, numResources);
		}
		result.CheckError();
	}

	internal unsafe void SetGPUThreadPriority(int priority)
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, int, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)10 * (nint)sizeof(void*))))(_nativePointer, priority)).CheckError();
	}

	internal unsafe void GetGPUThreadPriority(out int priorityRef)
	{
		Result result;
		fixed (int* ptr = &priorityRef)
		{
			void* ptr2 = ptr;
			result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)11 * (nint)sizeof(void*))))(_nativePointer, ptr2);
		}
		result.CheckError();
	}
}
