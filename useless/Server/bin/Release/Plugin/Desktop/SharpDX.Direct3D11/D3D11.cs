using System;
using System.Runtime.InteropServices;
using SharpDX.DXGI;
using SharpDX.Direct3D;

namespace SharpDX.Direct3D11;

internal static class D3D11
{
	public const int SdkVersion = 7;

	public unsafe static Result CreateDevice(Adapter adapterRef, DriverType driverType, IntPtr software, DeviceCreationFlags flags, FeatureLevel[] featureLevelsRef, int featureLevels, int sDKVersion, Device deviceOut, out FeatureLevel featureLevelRef, out DeviceContext immediateContextOut)
	{
		_ = IntPtr.Zero;
		IntPtr zero = IntPtr.Zero;
		IntPtr zero2 = IntPtr.Zero;
		IntPtr intPtr = CppObject.ToCallbackPtr<Adapter>(adapterRef);
		Result result;
		fixed (FeatureLevel* ptr = &featureLevelRef)
		{
			void* param = ptr;
			fixed (FeatureLevel* ptr2 = featureLevelsRef)
			{
				void* param2 = ptr2;
				result = D3D11CreateDevice_((void*)intPtr, (int)driverType, (void*)software, (int)flags, param2, featureLevels, sDKVersion, &zero, param, &zero2);
			}
		}
		deviceOut.NativePointer = zero;
		if (zero2 != IntPtr.Zero)
		{
			immediateContextOut = new DeviceContext(zero2);
			return result;
		}
		immediateContextOut = null;
		return result;
	}

	[DllImport("d3d11.dll", CallingConvention = CallingConvention.StdCall, EntryPoint = "D3D11CreateDevice")]
	private unsafe static extern int D3D11CreateDevice_(void* param0, int param1, void* param2, int param3, void* param4, int param5, int param6, void* param7, void* param8, void* param9);

	public unsafe static void On12CreateDevice(IUnknown deviceRef, DeviceCreationFlags flags, FeatureLevel[] featureLevelsRef, int featureLevels, IUnknown[] commandQueuesOut, int numQueues, int nodeMask, out Device deviceOut, out DeviceContext immediateContextOut, out FeatureLevel chosenFeatureLevelRef)
	{
		IntPtr zero = IntPtr.Zero;
		IntPtr* ptr = null;
		if (commandQueuesOut != null)
		{
			ptr = stackalloc IntPtr[commandQueuesOut.Length];
		}
		IntPtr zero2 = IntPtr.Zero;
		IntPtr zero3 = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<IUnknown>(deviceRef);
		if (commandQueuesOut != null)
		{
			for (int i = 0; i < commandQueuesOut.Length; i++)
			{
				ptr[i] = CppObject.ToCallbackPtr<IUnknown>(commandQueuesOut[i]);
			}
		}
		Result result;
		fixed (FeatureLevel* ptr2 = &chosenFeatureLevelRef)
		{
			void* param = ptr2;
			fixed (FeatureLevel* ptr3 = featureLevelsRef)
			{
				void* param2 = ptr3;
				result = D3D11On12CreateDevice_((void*)zero, (int)flags, param2, featureLevels, ptr, numQueues, nodeMask, &zero2, &zero3, param);
			}
		}
		if (zero2 != IntPtr.Zero)
		{
			deviceOut = new Device(zero2);
		}
		else
		{
			deviceOut = null;
		}
		if (zero3 != IntPtr.Zero)
		{
			immediateContextOut = new DeviceContext(zero3);
		}
		else
		{
			immediateContextOut = null;
		}
		result.CheckError();
	}

	[DllImport("d3d11.dll", CallingConvention = CallingConvention.StdCall, EntryPoint = "D3D11On12CreateDevice")]
	private unsafe static extern int D3D11On12CreateDevice_(void* param0, int param1, void* param2, int param3, void* param4, int param5, int param6, void* param7, void* param8, void* param9);
}
