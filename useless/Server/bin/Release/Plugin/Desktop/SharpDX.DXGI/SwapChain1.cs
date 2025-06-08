using System;
using System.Runtime.InteropServices;
using SharpDX.Mathematics.Interop;

namespace SharpDX.DXGI;

[Guid("790a45f7-0d42-4876-983a-0a55cfe6f4aa")]
public class SwapChain1 : SwapChain
{
	public SwapChainDescription1 Description1
	{
		get
		{
			GetDescription1(out var descRef);
			return descRef;
		}
	}

	public SwapChainFullScreenDescription FullscreenDescription
	{
		get
		{
			GetFullscreenDescription(out var descRef);
			return descRef;
		}
	}

	public IntPtr Hwnd
	{
		get
		{
			GetHwnd(out var hwndRef);
			return hwndRef;
		}
	}

	public RawBool IsTemporaryMonoSupported => IsTemporaryMonoSupported_();

	public Output RestrictToOutput
	{
		get
		{
			GetRestrictToOutput(out var restrictToOutputOut);
			return restrictToOutputOut;
		}
	}

	public RawColor4 BackgroundColor
	{
		get
		{
			GetBackgroundColor(out var colorRef);
			return colorRef;
		}
		set
		{
			SetBackgroundColor(value);
		}
	}

	public DisplayModeRotation Rotation
	{
		get
		{
			GetRotation(out var rotationRef);
			return rotationRef;
		}
		set
		{
			SetRotation(value);
		}
	}

	public SwapChain1(Factory2 factory, ComObject device, IntPtr hwnd, ref SwapChainDescription1 description, SwapChainFullScreenDescription? fullScreenDescription = null, Output restrictToOutput = null)
		: base(IntPtr.Zero)
	{
		factory.CreateSwapChainForHwnd(device, hwnd, ref description, fullScreenDescription, restrictToOutput, this);
	}

	public SwapChain1(Factory2 factory, ComObject device, ComObject coreWindow, ref SwapChainDescription1 description, Output restrictToOutput = null)
		: base(IntPtr.Zero)
	{
		factory.CreateSwapChainForCoreWindow(device, coreWindow, ref description, restrictToOutput, this);
	}

	public SwapChain1(Factory2 factory, ComObject device, ref SwapChainDescription1 description, Output restrictToOutput = null)
		: base(IntPtr.Zero)
	{
		factory.CreateSwapChainForComposition(device, ref description, restrictToOutput, this);
	}

	public unsafe Result Present(int syncInterval, PresentFlags presentFlags, PresentParameters presentParameters)
	{
		bool hasValue = presentParameters.ScrollRectangle.HasValue;
		bool hasValue2 = presentParameters.ScrollOffset.HasValue;
		RawRectangle rawRectangle = (hasValue ? presentParameters.ScrollRectangle.Value : default(RawRectangle));
		RawPoint rawPoint = (hasValue2 ? presentParameters.ScrollOffset.Value : default(RawPoint));
		fixed (RawRectangle* ptr = presentParameters.DirtyRectangles)
		{
			void* ptr2 = ptr;
			PresentParameters.__Native _Native = default(PresentParameters.__Native);
			_Native.DirtyRectsCount = ((presentParameters.DirtyRectangles != null) ? presentParameters.DirtyRectangles.Length : 0);
			_Native.PDirtyRects = (IntPtr)ptr2;
			_Native.PScrollRect = (hasValue ? new IntPtr(&rawRectangle) : IntPtr.Zero);
			_Native.PScrollOffset = (hasValue2 ? new IntPtr(&rawPoint) : IntPtr.Zero);
			return Present1(syncInterval, presentFlags, new IntPtr(&_Native));
		}
	}

	public SwapChain1(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator SwapChain1(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new SwapChain1(nativePtr);
		}
		return null;
	}

	internal unsafe void GetDescription1(out SwapChainDescription1 descRef)
	{
		descRef = default(SwapChainDescription1);
		Result result;
		fixed (SwapChainDescription1* ptr = &descRef)
		{
			void* ptr2 = ptr;
			result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)18 * (nint)sizeof(void*))))(_nativePointer, ptr2);
		}
		result.CheckError();
	}

	internal unsafe void GetFullscreenDescription(out SwapChainFullScreenDescription descRef)
	{
		descRef = default(SwapChainFullScreenDescription);
		Result result;
		fixed (SwapChainFullScreenDescription* ptr = &descRef)
		{
			void* ptr2 = ptr;
			result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)19 * (nint)sizeof(void*))))(_nativePointer, ptr2);
		}
		result.CheckError();
	}

	internal unsafe void GetHwnd(out IntPtr hwndRef)
	{
		Result result;
		fixed (IntPtr* ptr = &hwndRef)
		{
			void* ptr2 = ptr;
			result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)20 * (nint)sizeof(void*))))(_nativePointer, ptr2);
		}
		result.CheckError();
	}

	public unsafe void GetCoreWindow(Guid refiid, out IntPtr unkOut)
	{
		Result result;
		fixed (IntPtr* ptr = &unkOut)
		{
			void* ptr2 = ptr;
			result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)21 * (nint)sizeof(void*))))(_nativePointer, &refiid, ptr2);
		}
		result.CheckError();
	}

	internal unsafe Result Present1(int syncInterval, PresentFlags presentFlags, IntPtr presentParametersRef)
	{
		Result result = ((delegate* unmanaged[Stdcall]<void*, int, int, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)22 * (nint)sizeof(void*))))(_nativePointer, syncInterval, (int)presentFlags, (void*)presentParametersRef);
		result.CheckError();
		return result;
	}

	internal unsafe RawBool IsTemporaryMonoSupported_()
	{
		return ((delegate* unmanaged[Stdcall]<void*, RawBool>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)23 * (nint)sizeof(void*))))(_nativePointer);
	}

	internal unsafe void GetRestrictToOutput(out Output restrictToOutputOut)
	{
		IntPtr zero = IntPtr.Zero;
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)24 * (nint)sizeof(void*))))(_nativePointer, &zero);
		if (zero != IntPtr.Zero)
		{
			restrictToOutputOut = new Output(zero);
		}
		else
		{
			restrictToOutputOut = null;
		}
		result.CheckError();
	}

	internal unsafe void SetBackgroundColor(RawColor4 colorRef)
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)25 * (nint)sizeof(void*))))(_nativePointer, &colorRef)).CheckError();
	}

	internal unsafe void GetBackgroundColor(out RawColor4 colorRef)
	{
		colorRef = default(RawColor4);
		Result result;
		fixed (RawColor4* ptr = &colorRef)
		{
			void* ptr2 = ptr;
			result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)26 * (nint)sizeof(void*))))(_nativePointer, ptr2);
		}
		result.CheckError();
	}

	internal unsafe void SetRotation(DisplayModeRotation rotation)
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, int, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)27 * (nint)sizeof(void*))))(_nativePointer, (int)rotation)).CheckError();
	}

	internal unsafe void GetRotation(out DisplayModeRotation rotationRef)
	{
		Result result;
		fixed (DisplayModeRotation* ptr = &rotationRef)
		{
			void* ptr2 = ptr;
			result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)28 * (nint)sizeof(void*))))(_nativePointer, ptr2);
		}
		result.CheckError();
	}
}
