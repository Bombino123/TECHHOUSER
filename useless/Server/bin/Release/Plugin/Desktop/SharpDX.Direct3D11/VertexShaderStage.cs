using System;
using System.Runtime.InteropServices;

namespace SharpDX.Direct3D11;

[Guid("c0bfa96c-e089-44fb-8eaf-26f8796190da")]
public class VertexShaderStage : CommonShaderStage<VertexShader>
{
	public VertexShaderStage(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator VertexShaderStage(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new VertexShaderStage(nativePtr);
		}
		return null;
	}

	internal unsafe override void SetConstantBuffers(int startSlot, int numBuffers, IntPtr constantBuffersOut)
	{
		((delegate* unmanaged[Stdcall]<void*, int, int, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)7 * (nint)sizeof(void*))))(_nativePointer, startSlot, numBuffers, (void*)constantBuffersOut);
	}

	internal unsafe override void SetShader(VertexShader vertexShaderRef, ClassInstance[] classInstancesOut, int numClassInstances)
	{
		IntPtr zero = IntPtr.Zero;
		IntPtr* ptr = null;
		if (classInstancesOut != null)
		{
			ptr = stackalloc IntPtr[classInstancesOut.Length];
		}
		zero = CppObject.ToCallbackPtr<VertexShader>(vertexShaderRef);
		if (classInstancesOut != null)
		{
			for (int i = 0; i < classInstancesOut.Length; i++)
			{
				ptr[i] = CppObject.ToCallbackPtr<ClassInstance>(classInstancesOut[i]);
			}
		}
		((delegate* unmanaged[Stdcall]<void*, void*, void*, int, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)11 * (nint)sizeof(void*))))(_nativePointer, (void*)zero, ptr, numClassInstances);
	}

	internal unsafe override void SetShaderResources(int startSlot, int numViews, IntPtr shaderResourceViewsOut)
	{
		((delegate* unmanaged[Stdcall]<void*, int, int, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)25 * (nint)sizeof(void*))))(_nativePointer, startSlot, numViews, (void*)shaderResourceViewsOut);
	}

	internal unsafe override void SetSamplers(int startSlot, int numSamplers, IntPtr samplersOut)
	{
		((delegate* unmanaged[Stdcall]<void*, int, int, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)26 * (nint)sizeof(void*))))(_nativePointer, startSlot, numSamplers, (void*)samplersOut);
	}

	internal unsafe override void GetConstantBuffers(int startSlot, int numBuffers, Buffer[] constantBuffersOut)
	{
		IntPtr* ptr = null;
		if (constantBuffersOut != null)
		{
			ptr = stackalloc IntPtr[constantBuffersOut.Length];
		}
		((delegate* unmanaged[Stdcall]<void*, int, int, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)72 * (nint)sizeof(void*))))(_nativePointer, startSlot, numBuffers, ptr);
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

	internal unsafe override void GetShader(out VertexShader vertexShaderOut, ClassInstance[] classInstancesOut, ref int numClassInstancesRef)
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
			((delegate* unmanaged[Stdcall]<void*, void*, void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)76 * (nint)sizeof(void*))))(_nativePointer, &zero, ptr, ptr3);
		}
		if (zero != IntPtr.Zero)
		{
			vertexShaderOut = new VertexShader(zero);
		}
		else
		{
			vertexShaderOut = null;
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

	internal unsafe override void GetShaderResources(int startSlot, int numViews, ShaderResourceView[] shaderResourceViewsOut)
	{
		IntPtr* ptr = null;
		if (shaderResourceViewsOut != null)
		{
			ptr = stackalloc IntPtr[shaderResourceViewsOut.Length];
		}
		((delegate* unmanaged[Stdcall]<void*, int, int, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)84 * (nint)sizeof(void*))))(_nativePointer, startSlot, numViews, ptr);
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

	internal unsafe override void GetSamplers(int startSlot, int numSamplers, SamplerState[] samplersOut)
	{
		IntPtr* ptr = null;
		if (samplersOut != null)
		{
			ptr = stackalloc IntPtr[samplersOut.Length];
		}
		((delegate* unmanaged[Stdcall]<void*, int, int, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)85 * (nint)sizeof(void*))))(_nativePointer, startSlot, numSamplers, ptr);
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

	internal unsafe override void SetShader(VertexShader vertexShaderRef, ComArray<ClassInstance> classInstancesOut, int numClassInstances)
	{
		IntPtr zero = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<VertexShader>(vertexShaderRef);
		void* nativePointer = _nativePointer;
		void* intPtr = (void*)zero;
		IntPtr intPtr2 = classInstancesOut?.NativePointer ?? IntPtr.Zero;
		((delegate* unmanaged[Stdcall]<void*, void*, void*, int, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)11 * (nint)sizeof(void*))))(nativePointer, intPtr, (void*)intPtr2, numClassInstances);
	}

	private unsafe void SetShader(IntPtr vertexShaderRef, IntPtr classInstancesOut, int numClassInstances)
	{
		((delegate* unmanaged[Stdcall]<void*, void*, void*, int, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)11 * (nint)sizeof(void*))))(_nativePointer, (void*)vertexShaderRef, (void*)classInstancesOut, numClassInstances);
	}
}
