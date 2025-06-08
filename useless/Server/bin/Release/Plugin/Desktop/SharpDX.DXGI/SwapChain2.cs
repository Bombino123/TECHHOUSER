using System;
using System.Runtime.InteropServices;
using SharpDX.Mathematics.Interop;

namespace SharpDX.DXGI;

[Guid("a8be2ac4-199f-4946-b331-79599fb98de7")]
public class SwapChain2 : SwapChain1
{
	public Size2 SourceSize
	{
		get
		{
			GetSourceSize(out var widthRef, out var heightRef);
			return new Size2(widthRef, heightRef);
		}
		set
		{
			SetSourceSize(value.Width, value.Height);
		}
	}

	public int MaximumFrameLatency
	{
		get
		{
			GetMaximumFrameLatency(out var maxLatencyRef);
			return maxLatencyRef;
		}
		set
		{
			SetMaximumFrameLatency(value);
		}
	}

	public IntPtr FrameLatencyWaitableObject => GetFrameLatencyWaitableObject();

	public RawMatrix3x2 MatrixTransform
	{
		get
		{
			GetMatrixTransform(out var matrixRef);
			return matrixRef;
		}
		set
		{
			SetMatrixTransform(ref value);
		}
	}

	public SwapChain2(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator SwapChain2(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new SwapChain2(nativePtr);
		}
		return null;
	}

	internal unsafe void SetSourceSize(int width, int height)
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, int, int, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)29 * (nint)sizeof(void*))))(_nativePointer, width, height)).CheckError();
	}

	internal unsafe void GetSourceSize(out int widthRef, out int heightRef)
	{
		Result result;
		fixed (int* ptr = &heightRef)
		{
			void* ptr2 = ptr;
			fixed (int* ptr3 = &widthRef)
			{
				void* ptr4 = ptr3;
				result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)30 * (nint)sizeof(void*))))(_nativePointer, ptr4, ptr2);
			}
		}
		result.CheckError();
	}

	internal unsafe void SetMaximumFrameLatency(int maxLatency)
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, int, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)31 * (nint)sizeof(void*))))(_nativePointer, maxLatency)).CheckError();
	}

	internal unsafe void GetMaximumFrameLatency(out int maxLatencyRef)
	{
		Result result;
		fixed (int* ptr = &maxLatencyRef)
		{
			void* ptr2 = ptr;
			result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)32 * (nint)sizeof(void*))))(_nativePointer, ptr2);
		}
		result.CheckError();
	}

	internal unsafe IntPtr GetFrameLatencyWaitableObject()
	{
		return ((delegate* unmanaged[Stdcall]<void*, IntPtr>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)33 * (nint)sizeof(void*))))(_nativePointer);
	}

	internal unsafe void SetMatrixTransform(ref RawMatrix3x2 matrixRef)
	{
		Result result;
		fixed (RawMatrix3x2* ptr = &matrixRef)
		{
			void* ptr2 = ptr;
			result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)34 * (nint)sizeof(void*))))(_nativePointer, ptr2);
		}
		result.CheckError();
	}

	internal unsafe void GetMatrixTransform(out RawMatrix3x2 matrixRef)
	{
		matrixRef = default(RawMatrix3x2);
		Result result;
		fixed (RawMatrix3x2* ptr = &matrixRef)
		{
			void* ptr2 = ptr;
			result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)35 * (nint)sizeof(void*))))(_nativePointer, ptr2);
		}
		result.CheckError();
	}
}
