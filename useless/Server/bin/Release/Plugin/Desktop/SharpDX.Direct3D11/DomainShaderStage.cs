using System;
using System.Runtime.InteropServices;

namespace SharpDX.Direct3D11;

[Guid("c0bfa96c-e089-44fb-8eaf-26f8796190da")]
public class DomainShaderStage : CommonShaderStage<DomainShader>
{
	public DomainShaderStage(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator DomainShaderStage(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new DomainShaderStage(nativePtr);
		}
		return null;
	}

	internal unsafe override void SetShaderResources(int startSlot, int numViews, IntPtr shaderResourceViewsOut)
	{
		((delegate* unmanaged[Stdcall]<void*, int, int, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)63 * (nint)sizeof(void*))))(_nativePointer, startSlot, numViews, (void*)shaderResourceViewsOut);
	}

	internal unsafe override void SetShader(DomainShader domainShaderRef, ClassInstance[] classInstancesOut, int numClassInstances)
	{
		IntPtr zero = IntPtr.Zero;
		IntPtr* ptr = null;
		if (classInstancesOut != null)
		{
			ptr = stackalloc IntPtr[classInstancesOut.Length];
		}
		zero = CppObject.ToCallbackPtr<DomainShader>(domainShaderRef);
		if (classInstancesOut != null)
		{
			for (int i = 0; i < classInstancesOut.Length; i++)
			{
				ptr[i] = CppObject.ToCallbackPtr<ClassInstance>(classInstancesOut[i]);
			}
		}
		((delegate* unmanaged[Stdcall]<void*, void*, void*, int, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)64 * (nint)sizeof(void*))))(_nativePointer, (void*)zero, ptr, numClassInstances);
	}

	internal unsafe override void SetSamplers(int startSlot, int numSamplers, IntPtr samplersOut)
	{
		((delegate* unmanaged[Stdcall]<void*, int, int, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)65 * (nint)sizeof(void*))))(_nativePointer, startSlot, numSamplers, (void*)samplersOut);
	}

	internal unsafe override void SetConstantBuffers(int startSlot, int numBuffers, IntPtr constantBuffersOut)
	{
		((delegate* unmanaged[Stdcall]<void*, int, int, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)66 * (nint)sizeof(void*))))(_nativePointer, startSlot, numBuffers, (void*)constantBuffersOut);
	}

	internal unsafe override void GetShaderResources(int startSlot, int numViews, ShaderResourceView[] shaderResourceViewsOut)
	{
		IntPtr* ptr = null;
		if (shaderResourceViewsOut != null)
		{
			ptr = stackalloc IntPtr[shaderResourceViewsOut.Length];
		}
		((delegate* unmanaged[Stdcall]<void*, int, int, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)101 * (nint)sizeof(void*))))(_nativePointer, startSlot, numViews, ptr);
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

	internal unsafe override void GetShader(out DomainShader domainShaderOut, ClassInstance[] classInstancesOut, ref int numClassInstancesRef)
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
			((delegate* unmanaged[Stdcall]<void*, void*, void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)102 * (nint)sizeof(void*))))(_nativePointer, &zero, ptr, ptr3);
		}
		if (zero != IntPtr.Zero)
		{
			domainShaderOut = new DomainShader(zero);
		}
		else
		{
			domainShaderOut = null;
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
		((delegate* unmanaged[Stdcall]<void*, int, int, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)103 * (nint)sizeof(void*))))(_nativePointer, startSlot, numSamplers, ptr);
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
		((delegate* unmanaged[Stdcall]<void*, int, int, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)104 * (nint)sizeof(void*))))(_nativePointer, startSlot, numBuffers, ptr);
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

	internal unsafe override void SetShader(DomainShader domainShaderRef, ComArray<ClassInstance> classInstancesOut, int numClassInstances)
	{
		IntPtr zero = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<DomainShader>(domainShaderRef);
		void* nativePointer = _nativePointer;
		void* intPtr = (void*)zero;
		IntPtr intPtr2 = classInstancesOut?.NativePointer ?? IntPtr.Zero;
		((delegate* unmanaged[Stdcall]<void*, void*, void*, int, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)64 * (nint)sizeof(void*))))(nativePointer, intPtr, (void*)intPtr2, numClassInstances);
	}

	private unsafe void SetShader(IntPtr domainShaderRef, IntPtr classInstancesOut, int numClassInstances)
	{
		((delegate* unmanaged[Stdcall]<void*, void*, void*, int, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)64 * (nint)sizeof(void*))))(_nativePointer, (void*)domainShaderRef, (void*)classInstancesOut, numClassInstances);
	}
}
