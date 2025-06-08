using System;
using System.Runtime.InteropServices;
using SharpDX.DXGI;
using SharpDX.Direct3D;

namespace SharpDX.Direct3D11;

[Guid("a04bfb29-08ef-43d6-a49c-a9bdbdcbe686")]
public class Device1 : Device
{
	protected internal DeviceContext1 ImmediateContext1__;

	public DeviceContext1 ImmediateContext1
	{
		get
		{
			if (ImmediateContext1__ == null)
			{
				GetImmediateContext1(out ImmediateContext1__);
			}
			return ImmediateContext1__;
		}
	}

	public DeviceContextState CreateDeviceContextState<T>(CreateDeviceContextStateFlags flags, FeatureLevel[] featureLevelsRef, out FeatureLevel chosenFeatureLevelRef) where T : ComObject
	{
		DeviceContextState deviceContextState = new DeviceContextState(IntPtr.Zero);
		CreateDeviceContextState(flags, featureLevelsRef, featureLevelsRef.Length, 7, Utilities.GetGuidFromType(typeof(T)), out chosenFeatureLevelRef, deviceContextState);
		return deviceContextState;
	}

	public T OpenSharedResource1<T>(IntPtr resourceHandle) where T : ComObject
	{
		OpenSharedResource1(resourceHandle, Utilities.GetGuidFromType(typeof(T)), out var resourceOut);
		return CppObject.FromPointer<T>(resourceOut);
	}

	public T OpenSharedResource1<T>(string name, SharedResourceFlags desiredAccess) where T : ComObject
	{
		OpenSharedResourceByName(name, desiredAccess, Utilities.GetGuidFromType(typeof(T)), out var resourceOut);
		return CppObject.FromPointer<T>(resourceOut);
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing && ImmediateContext1__ != null)
		{
			ImmediateContext1__.Dispose();
			ImmediateContext1__ = null;
		}
		base.Dispose(disposing);
	}

	public Device1(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator Device1(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new Device1(nativePtr);
		}
		return null;
	}

	internal unsafe void GetImmediateContext1(out DeviceContext1 immediateContextOut)
	{
		IntPtr zero = IntPtr.Zero;
		((delegate* unmanaged[Stdcall]<void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)43 * (nint)sizeof(void*))))(_nativePointer, &zero);
		if (zero != IntPtr.Zero)
		{
			immediateContextOut = new DeviceContext1(zero);
		}
		else
		{
			immediateContextOut = null;
		}
	}

	internal unsafe void CreateDeferredContext1(int contextFlags, DeviceContext1 deferredContextOut)
	{
		IntPtr zero = IntPtr.Zero;
		Result result = ((delegate* unmanaged[Stdcall]<void*, int, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)44 * (nint)sizeof(void*))))(_nativePointer, contextFlags, &zero);
		deferredContextOut.NativePointer = zero;
		result.CheckError();
	}

	internal unsafe void CreateBlendState1(ref BlendStateDescription1 blendStateDescRef, BlendState1 blendStateOut)
	{
		BlendStateDescription1.__Native @ref = default(BlendStateDescription1.__Native);
		IntPtr zero = IntPtr.Zero;
		blendStateDescRef.__MarshalTo(ref @ref);
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)45 * (nint)sizeof(void*))))(_nativePointer, &@ref, &zero);
		blendStateOut.NativePointer = zero;
		blendStateDescRef.__MarshalFree(ref @ref);
		result.CheckError();
	}

	internal unsafe void CreateRasterizerState1(ref RasterizerStateDescription1 rasterizerDescRef, RasterizerState1 rasterizerStateOut)
	{
		IntPtr zero = IntPtr.Zero;
		Result result;
		fixed (RasterizerStateDescription1* ptr = &rasterizerDescRef)
		{
			void* ptr2 = ptr;
			result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)46 * (nint)sizeof(void*))))(_nativePointer, ptr2, &zero);
		}
		rasterizerStateOut.NativePointer = zero;
		result.CheckError();
	}

	internal unsafe void CreateDeviceContextState(CreateDeviceContextStateFlags flags, FeatureLevel[] featureLevelsRef, int featureLevels, int sDKVersion, Guid emulatedInterface, out FeatureLevel chosenFeatureLevelRef, DeviceContextState contextStateOut)
	{
		IntPtr zero = IntPtr.Zero;
		Result result;
		fixed (FeatureLevel* ptr = &chosenFeatureLevelRef)
		{
			void* ptr2 = ptr;
			fixed (FeatureLevel* ptr3 = featureLevelsRef)
			{
				void* ptr4 = ptr3;
				result = ((delegate* unmanaged[Stdcall]<void*, int, void*, int, int, void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)47 * (nint)sizeof(void*))))(_nativePointer, (int)flags, ptr4, featureLevels, sDKVersion, &emulatedInterface, ptr2, &zero);
			}
		}
		contextStateOut.NativePointer = zero;
		result.CheckError();
	}

	internal unsafe void OpenSharedResource1(IntPtr hResource, Guid returnedInterface, out IntPtr resourceOut)
	{
		Result result;
		fixed (IntPtr* ptr = &resourceOut)
		{
			void* ptr2 = ptr;
			result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)48 * (nint)sizeof(void*))))(_nativePointer, (void*)hResource, &returnedInterface, ptr2);
		}
		result.CheckError();
	}

	internal unsafe void OpenSharedResourceByName(string lpName, SharedResourceFlags dwDesiredAccess, Guid returnedInterface, out IntPtr resourceOut)
	{
		Result result;
		fixed (IntPtr* ptr = &resourceOut)
		{
			void* ptr2 = ptr;
			fixed (char* ptr3 = lpName)
			{
				result = ((delegate* unmanaged[Stdcall]<void*, void*, int, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)49 * (nint)sizeof(void*))))(_nativePointer, ptr3, (int)dwDesiredAccess, &returnedInterface, ptr2);
			}
		}
		result.CheckError();
	}
}
