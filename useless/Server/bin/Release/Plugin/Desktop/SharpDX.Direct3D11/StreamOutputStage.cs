using System;
using System.Runtime.InteropServices;

namespace SharpDX.Direct3D11;

[Guid("c0bfa96c-e089-44fb-8eaf-26f8796190da")]
public class StreamOutputStage : CppObject
{
	internal unsafe void SetTargets(int numBuffers, Buffer[] sOTargetsOut, int[] offsetsRef)
	{
		IntPtr* ptr = null;
		if (sOTargetsOut != null)
		{
			ptr = stackalloc IntPtr[sOTargetsOut.Length];
			for (int i = 0; i < sOTargetsOut.Length; i++)
			{
				ptr[i] = ((sOTargetsOut[i] == null) ? IntPtr.Zero : sOTargetsOut[i].NativePointer);
			}
		}
		fixed (int* ptr2 = offsetsRef)
		{
			void* value = ptr2;
			SetTargets(numBuffers, new IntPtr(ptr), new IntPtr(value));
		}
	}

	public unsafe void SetTarget(Buffer buffer, int offsets)
	{
		IntPtr intPtr = buffer?.NativePointer ?? IntPtr.Zero;
		SetTargets(1, new IntPtr(&intPtr), new IntPtr(&offsets));
	}

	public void SetTargets(StreamOutputBufferBinding[] bufferBindings)
	{
		if (bufferBindings == null)
		{
			SetTargets(0, null, null);
			return;
		}
		Buffer[] array = new Buffer[bufferBindings.Length];
		int[] array2 = new int[bufferBindings.Length];
		for (int i = 0; i < bufferBindings.Length; i++)
		{
			array[i] = bufferBindings[i].Buffer;
			array2[i] = bufferBindings[i].Offset;
		}
		SetTargets(bufferBindings.Length, array, array2);
	}

	public Buffer[] GetTargets(int numBuffers)
	{
		Buffer[] array = new Buffer[numBuffers];
		GetTargets(numBuffers, array);
		return array;
	}

	public StreamOutputStage(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator StreamOutputStage(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new StreamOutputStage(nativePtr);
		}
		return null;
	}

	internal unsafe void SetTargets(int numBuffers, IntPtr sOTargetsOut, IntPtr offsetsRef)
	{
		((delegate* unmanaged[Stdcall]<void*, int, void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)37 * (nint)sizeof(void*))))(_nativePointer, numBuffers, (void*)sOTargetsOut, (void*)offsetsRef);
	}

	internal unsafe void GetTargets(int numBuffers, Buffer[] sOTargetsOut)
	{
		IntPtr* ptr = null;
		if (sOTargetsOut != null)
		{
			ptr = stackalloc IntPtr[sOTargetsOut.Length];
		}
		((delegate* unmanaged[Stdcall]<void*, int, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)93 * (nint)sizeof(void*))))(_nativePointer, numBuffers, ptr);
		if (sOTargetsOut == null)
		{
			return;
		}
		for (int i = 0; i < sOTargetsOut.Length; i++)
		{
			if (ptr[i] != IntPtr.Zero)
			{
				sOTargetsOut[i] = new Buffer(ptr[i]);
			}
			else
			{
				sOTargetsOut[i] = null;
			}
		}
	}
}
