using System;
using System.Runtime.InteropServices;

namespace SharpDX.Direct3D11;

[Guid("8ffde202-a0e7-45df-9e01-e837801b5ea0")]
public class Device5 : Device4
{
	public Fence OpenSharedFence(IntPtr resourceHandle)
	{
		OpenSharedFence(resourceHandle, Utilities.GetGuidFromType(typeof(Fence)), out var fenceOut);
		return CppObject.FromPointer<Fence>(fenceOut);
	}

	public Device5(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator Device5(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new Device5(nativePtr);
		}
		return null;
	}

	internal unsafe void OpenSharedFence(IntPtr hFence, Guid returnedInterface, out IntPtr fenceOut)
	{
		Result result;
		fixed (IntPtr* ptr = &fenceOut)
		{
			void* ptr2 = ptr;
			result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)67 * (nint)sizeof(void*))))(_nativePointer, (void*)hFence, &returnedInterface, ptr2);
		}
		result.CheckError();
	}

	internal unsafe void CreateFence(long initialValue, FenceFlags flags, Guid returnedInterface, Fence fenceOut)
	{
		IntPtr zero = IntPtr.Zero;
		Result result = ((delegate* unmanaged[Stdcall]<void*, long, int, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)68 * (nint)sizeof(void*))))(_nativePointer, initialValue, (int)flags, &returnedInterface, &zero);
		fenceOut.NativePointer = zero;
		result.CheckError();
	}
}
