using System;
using System.Runtime.InteropServices;
using SharpDX.Mathematics.Interop;

namespace SharpDX.DXGI;

[Guid("310d36a0-d2e7-4c0a-aa04-6a9d23b8886a")]
public class SwapChain : DeviceChild
{
	public FrameStatistics FrameStatistics
	{
		get
		{
			TryGetFrameStatistics(out var statsRef).CheckError();
			return statsRef;
		}
	}

	public bool IsFullScreen
	{
		get
		{
			GetFullscreenState(out var fullscreenRef, out var targetOut);
			targetOut?.Dispose();
			return fullscreenRef;
		}
		set
		{
			SetFullscreenState(value, null);
		}
	}

	public SwapChainDescription Description
	{
		get
		{
			GetDescription(out var descRef);
			return descRef;
		}
	}

	public Output ContainingOutput
	{
		get
		{
			GetContainingOutput(out var outputOut);
			return outputOut;
		}
	}

	public int LastPresentCount
	{
		get
		{
			GetLastPresentCount(out var lastPresentCountRef);
			return lastPresentCountRef;
		}
	}

	public SwapChain(Factory factory, ComObject device, SwapChainDescription description)
		: base(IntPtr.Zero)
	{
		factory.CreateSwapChain(device, ref description, this);
	}

	public T GetBackBuffer<T>(int index) where T : ComObject
	{
		GetBuffer(index, Utilities.GetGuidFromType(typeof(T)), out var surfaceOut);
		return CppObject.FromPointer<T>(surfaceOut);
	}

	public Result Present(int syncInterval, PresentFlags flags)
	{
		Result result = TryPresent(syncInterval, flags);
		result.CheckError();
		return result;
	}

	public SwapChain(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator SwapChain(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new SwapChain(nativePtr);
		}
		return null;
	}

	public unsafe Result TryPresent(int syncInterval, PresentFlags flags)
	{
		return ((delegate* unmanaged[Stdcall]<void*, int, int, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)8 * (nint)sizeof(void*))))(_nativePointer, syncInterval, (int)flags);
	}

	internal unsafe void GetBuffer(int buffer, Guid riid, out IntPtr surfaceOut)
	{
		Result result;
		fixed (IntPtr* ptr = &surfaceOut)
		{
			void* ptr2 = ptr;
			result = ((delegate* unmanaged[Stdcall]<void*, int, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)9 * (nint)sizeof(void*))))(_nativePointer, buffer, &riid, ptr2);
		}
		result.CheckError();
	}

	public unsafe void SetFullscreenState(RawBool fullscreen, Output targetRef)
	{
		IntPtr zero = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<Output>(targetRef);
		((Result)((delegate* unmanaged[Stdcall]<void*, RawBool, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)10 * (nint)sizeof(void*))))(_nativePointer, fullscreen, (void*)zero)).CheckError();
	}

	public unsafe void GetFullscreenState(out RawBool fullscreenRef, out Output targetOut)
	{
		fullscreenRef = default(RawBool);
		IntPtr zero = IntPtr.Zero;
		Result result;
		fixed (RawBool* ptr = &fullscreenRef)
		{
			void* ptr2 = ptr;
			result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)11 * (nint)sizeof(void*))))(_nativePointer, ptr2, &zero);
		}
		if (zero != IntPtr.Zero)
		{
			targetOut = new Output(zero);
		}
		else
		{
			targetOut = null;
		}
		result.CheckError();
	}

	internal unsafe void GetDescription(out SwapChainDescription descRef)
	{
		descRef = default(SwapChainDescription);
		Result result;
		fixed (SwapChainDescription* ptr = &descRef)
		{
			void* ptr2 = ptr;
			result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)12 * (nint)sizeof(void*))))(_nativePointer, ptr2);
		}
		result.CheckError();
	}

	public unsafe void ResizeBuffers(int bufferCount, int width, int height, Format newFormat, SwapChainFlags swapChainFlags)
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, int, int, int, int, int, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)13 * (nint)sizeof(void*))))(_nativePointer, bufferCount, width, height, (int)newFormat, (int)swapChainFlags)).CheckError();
	}

	public unsafe void ResizeTarget(ref ModeDescription newTargetParametersRef)
	{
		Result result;
		fixed (ModeDescription* ptr = &newTargetParametersRef)
		{
			void* ptr2 = ptr;
			result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)14 * (nint)sizeof(void*))))(_nativePointer, ptr2);
		}
		result.CheckError();
	}

	internal unsafe void GetContainingOutput(out Output outputOut)
	{
		IntPtr zero = IntPtr.Zero;
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)15 * (nint)sizeof(void*))))(_nativePointer, &zero);
		if (zero != IntPtr.Zero)
		{
			outputOut = new Output(zero);
		}
		else
		{
			outputOut = null;
		}
		result.CheckError();
	}

	public unsafe Result TryGetFrameStatistics(out FrameStatistics statsRef)
	{
		statsRef = default(FrameStatistics);
		Result result;
		fixed (FrameStatistics* ptr = &statsRef)
		{
			void* ptr2 = ptr;
			result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)16 * (nint)sizeof(void*))))(_nativePointer, ptr2);
		}
		return result;
	}

	internal unsafe void GetLastPresentCount(out int lastPresentCountRef)
	{
		Result result;
		fixed (int* ptr = &lastPresentCountRef)
		{
			void* ptr2 = ptr;
			result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)17 * (nint)sizeof(void*))))(_nativePointer, ptr2);
		}
		result.CheckError();
	}
}
