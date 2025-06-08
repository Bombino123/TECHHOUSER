using System;
using System.Runtime.InteropServices;

namespace SharpDX.Direct3D11;

[Guid("c0bfa96c-e089-44fb-8eaf-26f8796190da")]
public class PixelShaderStage : CommonShaderStage<PixelShader>
{
	public const float PixelCenterFractionalComponent = 0.5f;

	public const int FrontfacingDefaultValue = -1;

	public const int FrontfacingFalseValue = 0;

	public const int FrontfacingTrueValue = -1;

	public const int InputRegisterComponents = 4;

	public const int InputRegisterComponentBitCount = 32;

	public const int InputRegisterCount = 32;

	public const int InputRegisterReadsPerInst = 2;

	public const int InputRegisterReadPorts = 1;

	public const int LegacyPixelCenterFractionalComponent = 0;

	public const int OutputDepthRegisterComponents = 1;

	public const int OutputDepthRegisterComponentBitCount = 32;

	public const int OutputDepthRegisterCount = 1;

	public const int OutputMaskRegisterComponents = 1;

	public const int OutputMaskRegisterComponentBitCount = 32;

	public const int OutputMaskRegisterCount = 1;

	public const int OutputRegisterComponents = 4;

	public const int OutputRegisterComponentBitCount = 32;

	public const int OutputRegisterCount = 8;

	public PixelShaderStage(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator PixelShaderStage(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new PixelShaderStage(nativePtr);
		}
		return null;
	}

	internal unsafe override void SetShaderResources(int startSlot, int numViews, IntPtr shaderResourceViewsOut)
	{
		((delegate* unmanaged[Stdcall]<void*, int, int, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)8 * (nint)sizeof(void*))))(_nativePointer, startSlot, numViews, (void*)shaderResourceViewsOut);
	}

	internal unsafe override void SetShader(PixelShader pixelShaderRef, ClassInstance[] classInstancesOut, int numClassInstances)
	{
		IntPtr zero = IntPtr.Zero;
		IntPtr* ptr = null;
		if (classInstancesOut != null)
		{
			ptr = stackalloc IntPtr[classInstancesOut.Length];
		}
		zero = CppObject.ToCallbackPtr<PixelShader>(pixelShaderRef);
		if (classInstancesOut != null)
		{
			for (int i = 0; i < classInstancesOut.Length; i++)
			{
				ptr[i] = CppObject.ToCallbackPtr<ClassInstance>(classInstancesOut[i]);
			}
		}
		((delegate* unmanaged[Stdcall]<void*, void*, void*, int, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)9 * (nint)sizeof(void*))))(_nativePointer, (void*)zero, ptr, numClassInstances);
	}

	internal unsafe override void SetSamplers(int startSlot, int numSamplers, IntPtr samplersOut)
	{
		((delegate* unmanaged[Stdcall]<void*, int, int, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)10 * (nint)sizeof(void*))))(_nativePointer, startSlot, numSamplers, (void*)samplersOut);
	}

	internal unsafe override void SetConstantBuffers(int startSlot, int numBuffers, IntPtr constantBuffersOut)
	{
		((delegate* unmanaged[Stdcall]<void*, int, int, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)16 * (nint)sizeof(void*))))(_nativePointer, startSlot, numBuffers, (void*)constantBuffersOut);
	}

	internal unsafe override void GetShaderResources(int startSlot, int numViews, ShaderResourceView[] shaderResourceViewsOut)
	{
		IntPtr* ptr = null;
		if (shaderResourceViewsOut != null)
		{
			ptr = stackalloc IntPtr[shaderResourceViewsOut.Length];
		}
		((delegate* unmanaged[Stdcall]<void*, int, int, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)73 * (nint)sizeof(void*))))(_nativePointer, startSlot, numViews, ptr);
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

	internal unsafe override void GetShader(out PixelShader pixelShaderOut, ClassInstance[] classInstancesOut, ref int numClassInstancesRef)
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
			((delegate* unmanaged[Stdcall]<void*, void*, void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)74 * (nint)sizeof(void*))))(_nativePointer, &zero, ptr, ptr3);
		}
		if (zero != IntPtr.Zero)
		{
			pixelShaderOut = new PixelShader(zero);
		}
		else
		{
			pixelShaderOut = null;
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
		((delegate* unmanaged[Stdcall]<void*, int, int, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)75 * (nint)sizeof(void*))))(_nativePointer, startSlot, numSamplers, ptr);
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
		((delegate* unmanaged[Stdcall]<void*, int, int, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)77 * (nint)sizeof(void*))))(_nativePointer, startSlot, numBuffers, ptr);
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

	internal unsafe override void SetShader(PixelShader pixelShaderRef, ComArray<ClassInstance> classInstancesOut, int numClassInstances)
	{
		IntPtr zero = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<PixelShader>(pixelShaderRef);
		void* nativePointer = _nativePointer;
		void* intPtr = (void*)zero;
		IntPtr intPtr2 = classInstancesOut?.NativePointer ?? IntPtr.Zero;
		((delegate* unmanaged[Stdcall]<void*, void*, void*, int, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)9 * (nint)sizeof(void*))))(nativePointer, intPtr, (void*)intPtr2, numClassInstances);
	}

	private unsafe void SetShader(IntPtr pixelShaderRef, IntPtr classInstancesOut, int numClassInstances)
	{
		((delegate* unmanaged[Stdcall]<void*, void*, void*, int, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)9 * (nint)sizeof(void*))))(_nativePointer, (void*)pixelShaderRef, (void*)classInstancesOut, numClassInstances);
	}
}
