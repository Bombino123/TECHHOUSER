using System;
using System.Runtime.InteropServices;

namespace SharpDX.Direct3D11;

[Guid("a6cd7faa-b0b7-4a2f-9436-8662a65797cb")]
public class ClassInstance : DeviceChild
{
	public unsafe string InstanceName
	{
		get
		{
			PointerSize bufferLengthRef = default(PointerSize);
			GetInstanceName(IntPtr.Zero, ref bufferLengthRef);
			sbyte* ptr = stackalloc sbyte[(int)(uint)(int)bufferLengthRef];
			GetInstanceName((IntPtr)ptr, ref bufferLengthRef);
			return Marshal.PtrToStringAnsi((IntPtr)ptr);
		}
	}

	public unsafe string TypeName
	{
		get
		{
			PointerSize bufferLengthRef = default(PointerSize);
			GetInstanceName(IntPtr.Zero, ref bufferLengthRef);
			sbyte* ptr = stackalloc sbyte[(int)(uint)(int)bufferLengthRef];
			GetTypeName((IntPtr)ptr, ref bufferLengthRef);
			return Marshal.PtrToStringAnsi((IntPtr)ptr);
		}
	}

	public ClassLinkage ClassLinkage
	{
		get
		{
			GetClassLinkage(out var linkageOut);
			return linkageOut;
		}
	}

	public ClassInstanceDescription Description
	{
		get
		{
			GetDescription(out var descRef);
			return descRef;
		}
	}

	public ClassInstance(ClassLinkage linkage, string classTypeName, int constantBufferOffset, int constantVectorOffset, int textureOffset, int samplerOffset)
		: base(IntPtr.Zero)
	{
		linkage.CreateClassInstance(classTypeName, constantBufferOffset, constantVectorOffset, textureOffset, samplerOffset, this);
	}

	public ClassInstance(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator ClassInstance(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new ClassInstance(nativePtr);
		}
		return null;
	}

	internal unsafe void GetClassLinkage(out ClassLinkage linkageOut)
	{
		IntPtr zero = IntPtr.Zero;
		((delegate* unmanaged[Stdcall]<void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)7 * (nint)sizeof(void*))))(_nativePointer, &zero);
		if (zero != IntPtr.Zero)
		{
			linkageOut = new ClassLinkage(zero);
		}
		else
		{
			linkageOut = null;
		}
	}

	internal unsafe void GetDescription(out ClassInstanceDescription descRef)
	{
		descRef = default(ClassInstanceDescription);
		fixed (ClassInstanceDescription* ptr = &descRef)
		{
			void* ptr2 = ptr;
			((delegate* unmanaged[Stdcall]<void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)8 * (nint)sizeof(void*))))(_nativePointer, ptr2);
		}
	}

	internal unsafe void GetInstanceName(IntPtr instanceNameRef, ref PointerSize bufferLengthRef)
	{
		fixed (PointerSize* ptr = &bufferLengthRef)
		{
			void* ptr2 = ptr;
			((delegate* unmanaged[Stdcall]<void*, void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)9 * (nint)sizeof(void*))))(_nativePointer, (void*)instanceNameRef, ptr2);
		}
	}

	internal unsafe void GetTypeName(IntPtr typeNameRef, ref PointerSize bufferLengthRef)
	{
		fixed (PointerSize* ptr = &bufferLengthRef)
		{
			void* ptr2 = ptr;
			((delegate* unmanaged[Stdcall]<void*, void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)10 * (nint)sizeof(void*))))(_nativePointer, (void*)typeNameRef, ptr2);
		}
	}
}
