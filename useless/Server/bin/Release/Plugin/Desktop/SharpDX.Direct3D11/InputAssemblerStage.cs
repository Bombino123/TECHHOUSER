using System;
using System.Runtime.InteropServices;
using SharpDX.DXGI;
using SharpDX.Direct3D;

namespace SharpDX.Direct3D11;

[Guid("c0bfa96c-e089-44fb-8eaf-26f8796190da")]
public class InputAssemblerStage : CppObject
{
	public const int DefaultIndexBufferOffsetInBytes = 0;

	public const int DefaultPrimitiveTopology = 0;

	public const int DefaultVertexBufferOffsetInBytes = 0;

	public const int IndexInputResourceSlotCount = 1;

	public const int InstanceIdBitCount = 32;

	public const int IntegerArithmeticBitCount = 32;

	public const int PatchMaximumControlPointCount = 32;

	public const int PrimitiveIdBitCount = 32;

	public const int VertexIdBitCount = 32;

	public const int VertexInputResourceSlotCount = 32;

	public const int VertexInputStructureElementsComponents = 128;

	public const int VertexInputStructureElementCount = 32;

	public InputLayout InputLayout
	{
		get
		{
			GetInputLayout(out var inputLayoutOut);
			return inputLayoutOut;
		}
		set
		{
			SetInputLayout(value);
		}
	}

	public PrimitiveTopology PrimitiveTopology
	{
		get
		{
			GetPrimitiveTopology(out var topologyRef);
			return topologyRef;
		}
		set
		{
			SetPrimitiveTopology(value);
		}
	}

	public unsafe void SetVertexBuffers(int slot, VertexBufferBinding vertexBufferBinding)
	{
		int stride = vertexBufferBinding.Stride;
		int offset = vertexBufferBinding.Offset;
		IntPtr intPtr = ((vertexBufferBinding.Buffer == null) ? IntPtr.Zero : vertexBufferBinding.Buffer.NativePointer);
		SetVertexBuffers(slot, 1, new IntPtr(&intPtr), new IntPtr(&stride), new IntPtr(&offset));
	}

	public unsafe void SetVertexBuffers(int firstSlot, params VertexBufferBinding[] vertexBufferBindings)
	{
		int num = vertexBufferBindings.Length;
		IntPtr* ptr = stackalloc IntPtr[num];
		int* ptr2 = stackalloc int[num];
		int* ptr3 = stackalloc int[num];
		for (int i = 0; i < vertexBufferBindings.Length; i++)
		{
			ptr[i] = ((vertexBufferBindings[i].Buffer == null) ? IntPtr.Zero : vertexBufferBindings[i].Buffer.NativePointer);
			ptr2[i] = vertexBufferBindings[i].Stride;
			ptr3[i] = vertexBufferBindings[i].Offset;
		}
		SetVertexBuffers(firstSlot, num, new IntPtr(ptr), new IntPtr(ptr2), new IntPtr(ptr3));
	}

	public unsafe void SetVertexBuffers(int slot, Buffer[] vertexBuffers, int[] stridesRef, int[] offsetsRef)
	{
		IntPtr* ptr = stackalloc IntPtr[vertexBuffers.Length];
		for (int i = 0; i < vertexBuffers.Length; i++)
		{
			ptr[i] = ((vertexBuffers[i] == null) ? IntPtr.Zero : vertexBuffers[i].NativePointer);
		}
		fixed (int* ptr2 = stridesRef)
		{
			void* ptr3 = ptr2;
			fixed (int* ptr4 = offsetsRef)
			{
				void* ptr5 = ptr4;
				SetVertexBuffers(slot, vertexBuffers.Length, new IntPtr(ptr), (IntPtr)ptr3, (IntPtr)ptr5);
			}
		}
	}

	public InputAssemblerStage(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator InputAssemblerStage(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new InputAssemblerStage(nativePtr);
		}
		return null;
	}

	internal unsafe void SetInputLayout(InputLayout inputLayoutRef)
	{
		IntPtr zero = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<InputLayout>(inputLayoutRef);
		((delegate* unmanaged[Stdcall]<void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)17 * (nint)sizeof(void*))))(_nativePointer, (void*)zero);
	}

	public unsafe void SetVertexBuffers(int startSlot, int numBuffers, IntPtr vertexBuffersOut, IntPtr stridesRef, IntPtr offsetsRef)
	{
		((delegate* unmanaged[Stdcall]<void*, int, int, void*, void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)18 * (nint)sizeof(void*))))(_nativePointer, startSlot, numBuffers, (void*)vertexBuffersOut, (void*)stridesRef, (void*)offsetsRef);
	}

	public unsafe void SetIndexBuffer(Buffer indexBufferRef, Format format, int offset)
	{
		IntPtr zero = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<Buffer>(indexBufferRef);
		((delegate* unmanaged[Stdcall]<void*, void*, int, int, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)19 * (nint)sizeof(void*))))(_nativePointer, (void*)zero, (int)format, offset);
	}

	internal unsafe void SetPrimitiveTopology(PrimitiveTopology topology)
	{
		((delegate* unmanaged[Stdcall]<void*, int, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)24 * (nint)sizeof(void*))))(_nativePointer, (int)topology);
	}

	internal unsafe void GetInputLayout(out InputLayout inputLayoutOut)
	{
		IntPtr zero = IntPtr.Zero;
		((delegate* unmanaged[Stdcall]<void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)78 * (nint)sizeof(void*))))(_nativePointer, &zero);
		if (zero != IntPtr.Zero)
		{
			inputLayoutOut = new InputLayout(zero);
		}
		else
		{
			inputLayoutOut = null;
		}
	}

	public unsafe void GetVertexBuffers(int startSlot, int numBuffers, Buffer[] vertexBuffersOut, int[] stridesRef, int[] offsetsRef)
	{
		IntPtr* ptr = null;
		if (vertexBuffersOut != null)
		{
			ptr = stackalloc IntPtr[vertexBuffersOut.Length];
		}
		fixed (int* ptr2 = offsetsRef)
		{
			void* ptr3 = ptr2;
			fixed (int* ptr4 = stridesRef)
			{
				void* ptr5 = ptr4;
				((delegate* unmanaged[Stdcall]<void*, int, int, void*, void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)79 * (nint)sizeof(void*))))(_nativePointer, startSlot, numBuffers, ptr, ptr5, ptr3);
			}
		}
		if (vertexBuffersOut == null)
		{
			return;
		}
		for (int i = 0; i < vertexBuffersOut.Length; i++)
		{
			if (ptr[i] != IntPtr.Zero)
			{
				vertexBuffersOut[i] = new Buffer(ptr[i]);
			}
			else
			{
				vertexBuffersOut[i] = null;
			}
		}
	}

	public unsafe void GetIndexBuffer(out Buffer indexBufferRef, out Format format, out int offset)
	{
		IntPtr zero = IntPtr.Zero;
		fixed (int* ptr = &offset)
		{
			void* ptr2 = ptr;
			fixed (Format* ptr3 = &format)
			{
				void* ptr4 = ptr3;
				((delegate* unmanaged[Stdcall]<void*, void*, void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)80 * (nint)sizeof(void*))))(_nativePointer, &zero, ptr4, ptr2);
			}
		}
		if (zero != IntPtr.Zero)
		{
			indexBufferRef = new Buffer(zero);
		}
		else
		{
			indexBufferRef = null;
		}
	}

	internal unsafe void GetPrimitiveTopology(out PrimitiveTopology topologyRef)
	{
		fixed (PrimitiveTopology* ptr = &topologyRef)
		{
			void* ptr2 = ptr;
			((delegate* unmanaged[Stdcall]<void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)83 * (nint)sizeof(void*))))(_nativePointer, ptr2);
		}
	}
}
