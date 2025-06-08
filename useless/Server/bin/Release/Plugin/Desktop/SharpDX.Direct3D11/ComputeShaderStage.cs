using System;
using System.Runtime.InteropServices;

namespace SharpDX.Direct3D11;

[Guid("c0bfa96c-e089-44fb-8eaf-26f8796190da")]
public class ComputeShaderStage : CommonShaderStage<ComputeShader>
{
	public const int UnorderedAccessViewSlotCount = 8;

	public const int DispatchMaximumThreadGroupsPerDimension = 65535;

	public const int ThreadGroupSharedMemoryRegisterCount = 8192;

	public const int ThreadGroupSharedMemoryRegisterReadsPerInst = 1;

	public const int ThreadGroupSharedMemoryResourceRegisterComponents = 1;

	public const int ThreadGroupSharedMemoryResourceRegisterReadPorts = 1;

	public const int ThreadgroupidRegisterComponents = 3;

	public const int ThreadgroupidRegisterCount = 1;

	public const int ThreadidingroupflattenedRegisterComponents = 1;

	public const int ThreadidingroupflattenedRegisterCount = 1;

	public const int ThreadidingroupRegisterComponents = 3;

	public const int ThreadidingroupRegisterCount = 1;

	public const int ThreadidRegisterComponents = 3;

	public const int ThreadidRegisterCount = 1;

	public const int ThreadGroupMaximumThreadsPerGroup = 1024;

	public const int ThreadGroupMaximumX = 1024;

	public const int ThreadGroupMaximumY = 1024;

	public const int ThreadGroupMaximumZ = 64;

	public const int ThreadGroupMinimumX = 1;

	public const int ThreadGroupMinimumY = 1;

	public const int ThreadGroupMinimumZ = 1;

	public const int ThreadLocalTempRegisterPool = 16384;

	public UnorderedAccessView[] GetUnorderedAccessViews(int startSlot, int count)
	{
		UnorderedAccessView[] array = new UnorderedAccessView[count];
		GetUnorderedAccessViews(startSlot, count, array);
		return array;
	}

	public void SetUnorderedAccessView(int startSlot, UnorderedAccessView unorderedAccessView)
	{
		SetUnorderedAccessView(startSlot, unorderedAccessView, -1);
	}

	public void SetUnorderedAccessView(int startSlot, UnorderedAccessView unorderedAccessView, int uavInitialCount)
	{
		SetUnorderedAccessViews(startSlot, new UnorderedAccessView[1] { unorderedAccessView }, new int[1] { uavInitialCount });
	}

	public void SetUnorderedAccessViews(int startSlot, params UnorderedAccessView[] unorderedAccessViews)
	{
		int[] array = new int[unorderedAccessViews.Length];
		for (int i = 0; i < unorderedAccessViews.Length; i++)
		{
			array[i] = -1;
		}
		SetUnorderedAccessViews(startSlot, unorderedAccessViews, array);
	}

	public unsafe void SetUnorderedAccessViews(int startSlot, UnorderedAccessView[] unorderedAccessViews, int[] uavInitialCounts)
	{
		IntPtr* ptr = null;
		if (unorderedAccessViews != null)
		{
			ptr = stackalloc IntPtr[unorderedAccessViews.Length];
			for (int i = 0; i < unorderedAccessViews.Length; i++)
			{
				ptr[i] = ((unorderedAccessViews[i] == null) ? IntPtr.Zero : unorderedAccessViews[i].NativePointer);
			}
		}
		fixed (int* ptr2 = uavInitialCounts)
		{
			((CommonShaderStage)this).SetUnorderedAccessViews(uavCount: (IntPtr)ptr2, startSlot: startSlot, numBuffers: (unorderedAccessViews != null) ? unorderedAccessViews.Length : 0, unorderedAccessBuffer: (IntPtr)ptr);
		}
	}

	public ComputeShaderStage(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator ComputeShaderStage(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new ComputeShaderStage(nativePtr);
		}
		return null;
	}

	internal unsafe override void SetShaderResources(int startSlot, int numViews, IntPtr shaderResourceViewsOut)
	{
		((delegate* unmanaged[Stdcall]<void*, int, int, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)67 * (nint)sizeof(void*))))(_nativePointer, startSlot, numViews, (void*)shaderResourceViewsOut);
	}

	internal unsafe override void SetUnorderedAccessViews(int startSlot, int numUAVs, IntPtr unorderedAccessViewsOut, IntPtr uAVInitialCountsRef)
	{
		((delegate* unmanaged[Stdcall]<void*, int, int, void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)68 * (nint)sizeof(void*))))(_nativePointer, startSlot, numUAVs, (void*)unorderedAccessViewsOut, (void*)uAVInitialCountsRef);
	}

	internal unsafe override void SetShader(ComputeShader computeShaderRef, ClassInstance[] classInstancesOut, int numClassInstances)
	{
		IntPtr zero = IntPtr.Zero;
		IntPtr* ptr = null;
		if (classInstancesOut != null)
		{
			ptr = stackalloc IntPtr[classInstancesOut.Length];
		}
		zero = CppObject.ToCallbackPtr<ComputeShader>(computeShaderRef);
		if (classInstancesOut != null)
		{
			for (int i = 0; i < classInstancesOut.Length; i++)
			{
				ptr[i] = CppObject.ToCallbackPtr<ClassInstance>(classInstancesOut[i]);
			}
		}
		((delegate* unmanaged[Stdcall]<void*, void*, void*, int, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)69 * (nint)sizeof(void*))))(_nativePointer, (void*)zero, ptr, numClassInstances);
	}

	internal unsafe override void SetSamplers(int startSlot, int numSamplers, IntPtr samplersOut)
	{
		((delegate* unmanaged[Stdcall]<void*, int, int, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)70 * (nint)sizeof(void*))))(_nativePointer, startSlot, numSamplers, (void*)samplersOut);
	}

	internal unsafe override void SetConstantBuffers(int startSlot, int numBuffers, IntPtr constantBuffersOut)
	{
		((delegate* unmanaged[Stdcall]<void*, int, int, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)71 * (nint)sizeof(void*))))(_nativePointer, startSlot, numBuffers, (void*)constantBuffersOut);
	}

	internal unsafe override void GetShaderResources(int startSlot, int numViews, ShaderResourceView[] shaderResourceViewsOut)
	{
		IntPtr* ptr = null;
		if (shaderResourceViewsOut != null)
		{
			ptr = stackalloc IntPtr[shaderResourceViewsOut.Length];
		}
		((delegate* unmanaged[Stdcall]<void*, int, int, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)105 * (nint)sizeof(void*))))(_nativePointer, startSlot, numViews, ptr);
		if (shaderResourceViewsOut == null)
		{
			return;
		}
		for (int i = 0; i < shaderResourceViewsOut.Length; i++)
		{
			if (ptr[i] != IntPtr.Zero)
			{
				shaderResourceViewsOut[i] = new ShaderResourceView(ptr[i]);
			}
			else
			{
				shaderResourceViewsOut[i] = null;
			}
		}
	}

	internal unsafe void GetUnorderedAccessViews(int startSlot, int numUAVs, UnorderedAccessView[] unorderedAccessViewsOut)
	{
		IntPtr* ptr = null;
		if (unorderedAccessViewsOut != null)
		{
			ptr = stackalloc IntPtr[unorderedAccessViewsOut.Length];
		}
		((delegate* unmanaged[Stdcall]<void*, int, int, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)106 * (nint)sizeof(void*))))(_nativePointer, startSlot, numUAVs, ptr);
		if (unorderedAccessViewsOut == null)
		{
			return;
		}
		for (int i = 0; i < unorderedAccessViewsOut.Length; i++)
		{
			if (ptr[i] != IntPtr.Zero)
			{
				unorderedAccessViewsOut[i] = new UnorderedAccessView(ptr[i]);
			}
			else
			{
				unorderedAccessViewsOut[i] = null;
			}
		}
	}

	internal unsafe override void GetShader(out ComputeShader computeShaderOut, ClassInstance[] classInstancesOut, ref int numClassInstancesRef)
	{
		IntPtr zero = IntPtr.Zero;
		IntPtr* ptr = null;
		if (classInstancesOut != null)
		{
			ptr = stackalloc IntPtr[classInstancesOut.Length];
		}
		fixed (int* ptr2 = &numClassInstancesRef)
		{
			void* ptr3 = ptr2;
			((delegate* unmanaged[Stdcall]<void*, void*, void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)107 * (nint)sizeof(void*))))(_nativePointer, &zero, ptr, ptr3);
		}
		if (zero != IntPtr.Zero)
		{
			computeShaderOut = new ComputeShader(zero);
		}
		else
		{
			computeShaderOut = null;
		}
		if (classInstancesOut == null)
		{
			return;
		}
		for (int i = 0; i < classInstancesOut.Length; i++)
		{
			if (ptr[i] != IntPtr.Zero)
			{
				classInstancesOut[i] = new ClassInstance(ptr[i]);
			}
			else
			{
				classInstancesOut[i] = null;
			}
		}
	}

	internal unsafe override void GetSamplers(int startSlot, int numSamplers, SamplerState[] samplersOut)
	{
		IntPtr* ptr = null;
		if (samplersOut != null)
		{
			ptr = stackalloc IntPtr[samplersOut.Length];
		}
		((delegate* unmanaged[Stdcall]<void*, int, int, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)108 * (nint)sizeof(void*))))(_nativePointer, startSlot, numSamplers, ptr);
		if (samplersOut == null)
		{
			return;
		}
		for (int i = 0; i < samplersOut.Length; i++)
		{
			if (ptr[i] != IntPtr.Zero)
			{
				samplersOut[i] = new SamplerState(ptr[i]);
			}
			else
			{
				samplersOut[i] = null;
			}
		}
	}

	internal unsafe override void GetConstantBuffers(int startSlot, int numBuffers, Buffer[] constantBuffersOut)
	{
		IntPtr* ptr = null;
		if (constantBuffersOut != null)
		{
			ptr = stackalloc IntPtr[constantBuffersOut.Length];
		}
		((delegate* unmanaged[Stdcall]<void*, int, int, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)109 * (nint)sizeof(void*))))(_nativePointer, startSlot, numBuffers, ptr);
		if (constantBuffersOut == null)
		{
			return;
		}
		for (int i = 0; i < constantBuffersOut.Length; i++)
		{
			if (ptr[i] != IntPtr.Zero)
			{
				constantBuffersOut[i] = new Buffer(ptr[i]);
			}
			else
			{
				constantBuffersOut[i] = null;
			}
		}
	}

	internal unsafe override void SetShader(ComputeShader computeShaderRef, ComArray<ClassInstance> classInstancesOut, int numClassInstances)
	{
		IntPtr zero = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<ComputeShader>(computeShaderRef);
		void* nativePointer = _nativePointer;
		void* intPtr = (void*)zero;
		IntPtr intPtr2 = classInstancesOut?.NativePointer ?? IntPtr.Zero;
		((delegate* unmanaged[Stdcall]<void*, void*, void*, int, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)69 * (nint)sizeof(void*))))(nativePointer, intPtr, (void*)intPtr2, numClassInstances);
	}

	private unsafe void SetShader(IntPtr computeShaderRef, IntPtr classInstancesOut, int numClassInstances)
	{
		((delegate* unmanaged[Stdcall]<void*, void*, void*, int, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)69 * (nint)sizeof(void*))))(_nativePointer, (void*)computeShaderRef, (void*)classInstancesOut, numClassInstances);
	}
}
