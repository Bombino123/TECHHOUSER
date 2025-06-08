using System;
using System.Runtime.InteropServices;
using SharpDX.Mathematics.Interop;

namespace SharpDX.DXGI;

[Guid("ae02eedb-c735-4690-8d52-5a8dc20213aa")]
public class Output : DXGIObject
{
	public OutputDescription Description
	{
		get
		{
			GetDescription(out var descRef);
			return descRef;
		}
	}

	public GammaControlCapabilities GammaControlCapabilities
	{
		get
		{
			GetGammaControlCapabilities(out var gammaCapsRef);
			return gammaCapsRef;
		}
	}

	public GammaControl GammaControl
	{
		get
		{
			GetGammaControl(out var arrayRef);
			return arrayRef;
		}
		set
		{
			SetGammaControl(ref value);
		}
	}

	public FrameStatistics FrameStatistics
	{
		get
		{
			GetFrameStatistics(out var statsRef);
			return statsRef;
		}
	}

	public void GetClosestMatchingMode(ComObject device, ModeDescription modeToMatch, out ModeDescription closestMatch)
	{
		FindClosestMatchingMode(ref modeToMatch, out closestMatch, device);
	}

	public ModeDescription[] GetDisplayModeList(Format format, DisplayModeEnumerationFlags flags)
	{
		int numModesRef = 0;
		GetDisplayModeList(format, (int)flags, ref numModesRef, null);
		ModeDescription[] array = new ModeDescription[numModesRef];
		if (numModesRef > 0)
		{
			GetDisplayModeList(format, (int)flags, ref numModesRef, array);
		}
		return array;
	}

	public Output(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator Output(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new Output(nativePtr);
		}
		return null;
	}

	internal unsafe void GetDescription(out OutputDescription descRef)
	{
		OutputDescription.__Native @ref = default(OutputDescription.__Native);
		descRef = default(OutputDescription);
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)7 * (nint)sizeof(void*))))(_nativePointer, &@ref);
		descRef.__MarshalFrom(ref @ref);
		result.CheckError();
	}

	internal unsafe void GetDisplayModeList(Format enumFormat, int flags, ref int numModesRef, ModeDescription[] descRef)
	{
		Result result;
		fixed (ModeDescription* ptr = descRef)
		{
			void* ptr2 = ptr;
			fixed (int* ptr3 = &numModesRef)
			{
				void* ptr4 = ptr3;
				result = ((delegate* unmanaged[Stdcall]<void*, int, int, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)8 * (nint)sizeof(void*))))(_nativePointer, (int)enumFormat, flags, ptr4, ptr2);
			}
		}
		result.CheckError();
	}

	internal unsafe void FindClosestMatchingMode(ref ModeDescription modeToMatchRef, out ModeDescription closestMatchRef, IUnknown concernedDeviceRef)
	{
		closestMatchRef = default(ModeDescription);
		IntPtr zero = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<IUnknown>(concernedDeviceRef);
		Result result;
		fixed (ModeDescription* ptr = &closestMatchRef)
		{
			void* ptr2 = ptr;
			fixed (ModeDescription* ptr3 = &modeToMatchRef)
			{
				void* ptr4 = ptr3;
				result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)9 * (nint)sizeof(void*))))(_nativePointer, ptr4, ptr2, (void*)zero);
			}
		}
		result.CheckError();
	}

	public unsafe void WaitForVerticalBlank()
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)10 * (nint)sizeof(void*))))(_nativePointer)).CheckError();
	}

	public unsafe void TakeOwnership(IUnknown deviceRef, RawBool exclusive)
	{
		IntPtr zero = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<IUnknown>(deviceRef);
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, RawBool, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)11 * (nint)sizeof(void*))))(_nativePointer, (void*)zero, exclusive)).CheckError();
	}

	public unsafe void ReleaseOwnership()
	{
		((delegate* unmanaged[Stdcall]<void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)12 * (nint)sizeof(void*))))(_nativePointer);
	}

	internal unsafe void GetGammaControlCapabilities(out GammaControlCapabilities gammaCapsRef)
	{
		GammaControlCapabilities.__Native @ref = default(GammaControlCapabilities.__Native);
		gammaCapsRef = default(GammaControlCapabilities);
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)13 * (nint)sizeof(void*))))(_nativePointer, &@ref);
		gammaCapsRef.__MarshalFrom(ref @ref);
		result.CheckError();
	}

	internal unsafe void SetGammaControl(ref GammaControl arrayRef)
	{
		GammaControl.__Native @ref = default(GammaControl.__Native);
		arrayRef.__MarshalTo(ref @ref);
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)14 * (nint)sizeof(void*))))(_nativePointer, &@ref);
		arrayRef.__MarshalFree(ref @ref);
		result.CheckError();
	}

	internal unsafe void GetGammaControl(out GammaControl arrayRef)
	{
		GammaControl.__Native @ref = default(GammaControl.__Native);
		arrayRef = default(GammaControl);
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)15 * (nint)sizeof(void*))))(_nativePointer, &@ref);
		arrayRef.__MarshalFrom(ref @ref);
		result.CheckError();
	}

	public unsafe void SetDisplaySurface(Surface scanoutSurfaceRef)
	{
		IntPtr zero = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<Surface>(scanoutSurfaceRef);
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)16 * (nint)sizeof(void*))))(_nativePointer, (void*)zero)).CheckError();
	}

	public unsafe void CopyDisplaySurfaceTo(Surface destinationRef)
	{
		IntPtr zero = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<Surface>(destinationRef);
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)17 * (nint)sizeof(void*))))(_nativePointer, (void*)zero)).CheckError();
	}

	internal unsafe void GetFrameStatistics(out FrameStatistics statsRef)
	{
		statsRef = default(FrameStatistics);
		Result result;
		fixed (FrameStatistics* ptr = &statsRef)
		{
			void* ptr2 = ptr;
			result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)18 * (nint)sizeof(void*))))(_nativePointer, ptr2);
		}
		result.CheckError();
	}
}
