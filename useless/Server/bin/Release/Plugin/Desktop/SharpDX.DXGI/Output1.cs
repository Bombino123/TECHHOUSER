using System;
using System.Runtime.InteropServices;

namespace SharpDX.DXGI;

[Guid("00cddea8-939b-4b83-a340-a685226666cc")]
public class Output1 : Output
{
	public ModeDescription1[] GetDisplayModeList1(Format enumFormat, DisplayModeEnumerationFlags flags)
	{
		int numModesRef = 0;
		GetDisplayModeList1(enumFormat, (int)flags, ref numModesRef, null);
		ModeDescription1[] array = new ModeDescription1[numModesRef];
		if (numModesRef > 0)
		{
			GetDisplayModeList1(enumFormat, (int)flags, ref numModesRef, array);
		}
		return array;
	}

	public Output1(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator Output1(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new Output1(nativePtr);
		}
		return null;
	}

	internal unsafe void GetDisplayModeList1(Format enumFormat, int flags, ref int numModesRef, ModeDescription1[] descRef)
	{
		Result result;
		fixed (ModeDescription1* ptr = descRef)
		{
			void* ptr2 = ptr;
			fixed (int* ptr3 = &numModesRef)
			{
				void* ptr4 = ptr3;
				result = ((delegate* unmanaged[Stdcall]<void*, int, int, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)19 * (nint)sizeof(void*))))(_nativePointer, (int)enumFormat, flags, ptr4, ptr2);
			}
		}
		result.CheckError();
	}

	public unsafe void FindClosestMatchingMode1(ref ModeDescription1 modeToMatchRef, out ModeDescription1 closestMatchRef, IUnknown concernedDeviceRef)
	{
		closestMatchRef = default(ModeDescription1);
		IntPtr zero = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<IUnknown>(concernedDeviceRef);
		Result result;
		fixed (ModeDescription1* ptr = &closestMatchRef)
		{
			void* ptr2 = ptr;
			fixed (ModeDescription1* ptr3 = &modeToMatchRef)
			{
				void* ptr4 = ptr3;
				result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)20 * (nint)sizeof(void*))))(_nativePointer, ptr4, ptr2, (void*)zero);
			}
		}
		result.CheckError();
	}

	public unsafe void GetDisplaySurfaceData1(Resource destinationRef)
	{
		IntPtr zero = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<Resource>(destinationRef);
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)21 * (nint)sizeof(void*))))(_nativePointer, (void*)zero)).CheckError();
	}

	public unsafe OutputDuplication DuplicateOutput(IUnknown deviceRef)
	{
		IntPtr zero = IntPtr.Zero;
		IntPtr zero2 = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<IUnknown>(deviceRef);
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)22 * (nint)sizeof(void*))))(_nativePointer, (void*)zero, &zero2);
		OutputDuplication result2 = ((!(zero2 != IntPtr.Zero)) ? null : new OutputDuplication(zero2));
		result.CheckError();
		return result2;
	}
}
