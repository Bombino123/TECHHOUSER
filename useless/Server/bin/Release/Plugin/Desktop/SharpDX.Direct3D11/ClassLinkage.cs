using System;
using System.Runtime.InteropServices;

namespace SharpDX.Direct3D11;

[Guid("ddf57cba-9543-46e4-a12b-f207a0fe7fed")]
public class ClassLinkage : DeviceChild
{
	public ClassLinkage(Device device)
		: base(IntPtr.Zero)
	{
		device.CreateClassLinkage(this);
	}

	public ClassLinkage(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator ClassLinkage(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new ClassLinkage(nativePtr);
		}
		return null;
	}

	public unsafe ClassInstance GetClassInstance(string classInstanceNameRef, int instanceIndex)
	{
		IntPtr zero = IntPtr.Zero;
		IntPtr intPtr = Marshal.StringToHGlobalAnsi(classInstanceNameRef);
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, int, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)7 * (nint)sizeof(void*))))(_nativePointer, (void*)intPtr, instanceIndex, &zero);
		ClassInstance result2 = ((!(zero != IntPtr.Zero)) ? null : new ClassInstance(zero));
		Marshal.FreeHGlobal(intPtr);
		result.CheckError();
		return result2;
	}

	internal unsafe void CreateClassInstance(string classTypeNameRef, int constantBufferOffset, int constantVectorOffset, int textureOffset, int samplerOffset, ClassInstance instanceOut)
	{
		IntPtr zero = IntPtr.Zero;
		IntPtr intPtr = Marshal.StringToHGlobalAnsi(classTypeNameRef);
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, int, int, int, int, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)8 * (nint)sizeof(void*))))(_nativePointer, (void*)intPtr, constantBufferOffset, constantVectorOffset, textureOffset, samplerOffset, &zero);
		instanceOut.NativePointer = zero;
		Marshal.FreeHGlobal(intPtr);
		result.CheckError();
	}
}
