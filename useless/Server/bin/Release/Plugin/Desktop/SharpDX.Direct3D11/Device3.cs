using System;
using System.Runtime.InteropServices;

namespace SharpDX.Direct3D11;

[Guid("A05C8C37-D2C6-4732-B3A0-9CE0B0DC9AE6")]
public class Device3 : Device2
{
	protected internal DeviceContext3 ImmediateContext3__;

	public DeviceContext3 ImmediateContext3
	{
		get
		{
			if (ImmediateContext3__ == null)
			{
				GetImmediateContext3(out ImmediateContext3__);
			}
			return ImmediateContext3__;
		}
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing && ImmediateContext3__ != null)
		{
			ImmediateContext3__.Dispose();
			ImmediateContext3__ = null;
		}
		base.Dispose(disposing);
	}

	public Device3(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator Device3(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new Device3(nativePtr);
		}
		return null;
	}

	internal unsafe void CreateTexture2D1(ref Texture2DDescription1 desc1Ref, DataBox[] initialDataRef, Texture2D1 texture2DOut)
	{
		IntPtr zero = IntPtr.Zero;
		Result result;
		fixed (DataBox* ptr = initialDataRef)
		{
			void* ptr2 = ptr;
			fixed (Texture2DDescription1* ptr3 = &desc1Ref)
			{
				void* ptr4 = ptr3;
				result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)54 * (nint)sizeof(void*))))(_nativePointer, ptr4, ptr2, &zero);
			}
		}
		texture2DOut.NativePointer = zero;
		result.CheckError();
	}

	internal unsafe void CreateTexture3D1(ref Texture3DDescription1 desc1Ref, DataBox[] initialDataRef, Texture3D1 texture3DOut)
	{
		IntPtr zero = IntPtr.Zero;
		Result result;
		fixed (DataBox* ptr = initialDataRef)
		{
			void* ptr2 = ptr;
			fixed (Texture3DDescription1* ptr3 = &desc1Ref)
			{
				void* ptr4 = ptr3;
				result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)55 * (nint)sizeof(void*))))(_nativePointer, ptr4, ptr2, &zero);
			}
		}
		texture3DOut.NativePointer = zero;
		result.CheckError();
	}

	internal unsafe void CreateRasterizerState2(ref RasterizerStateDescription2 rasterizerDescRef, RasterizerState2 rasterizerStateOut)
	{
		IntPtr zero = IntPtr.Zero;
		Result result;
		fixed (RasterizerStateDescription2* ptr = &rasterizerDescRef)
		{
			void* ptr2 = ptr;
			result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)56 * (nint)sizeof(void*))))(_nativePointer, ptr2, &zero);
		}
		rasterizerStateOut.NativePointer = zero;
		result.CheckError();
	}

	internal unsafe void CreateShaderResourceView1(Resource resourceRef, ShaderResourceViewDescription1? desc1Ref, ShaderResourceView1 sRView1Out)
	{
		IntPtr zero = IntPtr.Zero;
		IntPtr zero2 = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<Resource>(resourceRef);
		ShaderResourceViewDescription1 value = default(ShaderResourceViewDescription1);
		if (desc1Ref.HasValue)
		{
			value = desc1Ref.Value;
		}
		void* nativePointer = _nativePointer;
		void* intPtr = (void*)zero;
		ShaderResourceViewDescription1* intPtr2 = ((!desc1Ref.HasValue) ? null : (&value));
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)57 * (nint)sizeof(void*))))(nativePointer, intPtr, intPtr2, &zero2);
		sRView1Out.NativePointer = zero2;
		result.CheckError();
	}

	internal unsafe void CreateUnorderedAccessView1(Resource resourceRef, UnorderedAccessViewDescription1? desc1Ref, UnorderedAccessView1 uAView1Out)
	{
		IntPtr zero = IntPtr.Zero;
		IntPtr zero2 = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<Resource>(resourceRef);
		UnorderedAccessViewDescription1 value = default(UnorderedAccessViewDescription1);
		if (desc1Ref.HasValue)
		{
			value = desc1Ref.Value;
		}
		void* nativePointer = _nativePointer;
		void* intPtr = (void*)zero;
		UnorderedAccessViewDescription1* intPtr2 = ((!desc1Ref.HasValue) ? null : (&value));
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)58 * (nint)sizeof(void*))))(nativePointer, intPtr, intPtr2, &zero2);
		uAView1Out.NativePointer = zero2;
		result.CheckError();
	}

	internal unsafe void CreateRenderTargetView1(Resource resourceRef, RenderTargetViewDescription1? desc1Ref, RenderTargetView1 rTView1Out)
	{
		IntPtr zero = IntPtr.Zero;
		IntPtr zero2 = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<Resource>(resourceRef);
		RenderTargetViewDescription1 value = default(RenderTargetViewDescription1);
		if (desc1Ref.HasValue)
		{
			value = desc1Ref.Value;
		}
		void* nativePointer = _nativePointer;
		void* intPtr = (void*)zero;
		RenderTargetViewDescription1* intPtr2 = ((!desc1Ref.HasValue) ? null : (&value));
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)59 * (nint)sizeof(void*))))(nativePointer, intPtr, intPtr2, &zero2);
		rTView1Out.NativePointer = zero2;
		result.CheckError();
	}

	internal unsafe void CreateQuery1(QueryDescription1 queryDesc1Ref, Query1 query1Out)
	{
		IntPtr zero = IntPtr.Zero;
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)60 * (nint)sizeof(void*))))(_nativePointer, &queryDesc1Ref, &zero);
		query1Out.NativePointer = zero;
		result.CheckError();
	}

	internal unsafe void GetImmediateContext3(out DeviceContext3 immediateContextOut)
	{
		IntPtr zero = IntPtr.Zero;
		((delegate* unmanaged[Stdcall]<void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)61 * (nint)sizeof(void*))))(_nativePointer, &zero);
		if (zero != IntPtr.Zero)
		{
			immediateContextOut = new DeviceContext3(zero);
		}
		else
		{
			immediateContextOut = null;
		}
	}

	internal unsafe void CreateDeferredContext3(int contextFlags, DeviceContext3 deferredContextOut)
	{
		IntPtr zero = IntPtr.Zero;
		Result result = ((delegate* unmanaged[Stdcall]<void*, int, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)62 * (nint)sizeof(void*))))(_nativePointer, contextFlags, &zero);
		deferredContextOut.NativePointer = zero;
		result.CheckError();
	}

	public unsafe void WriteToSubresource(Resource dstResourceRef, int dstSubresource, ResourceRegion? dstBoxRef, IntPtr srcDataRef, int srcRowPitch, int srcDepthPitch)
	{
		IntPtr zero = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<Resource>(dstResourceRef);
		ResourceRegion value = default(ResourceRegion);
		if (dstBoxRef.HasValue)
		{
			value = dstBoxRef.Value;
		}
		void* nativePointer = _nativePointer;
		void* intPtr = (void*)zero;
		ResourceRegion* intPtr2 = ((!dstBoxRef.HasValue) ? null : (&value));
		((delegate* unmanaged[Stdcall]<void*, void*, int, void*, void*, int, int, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)63 * (nint)sizeof(void*))))(nativePointer, intPtr, dstSubresource, intPtr2, (void*)srcDataRef, srcRowPitch, srcDepthPitch);
	}

	public unsafe void ReadFromSubresource(IntPtr dstDataRef, int dstRowPitch, int dstDepthPitch, Resource srcResourceRef, int srcSubresource, ResourceRegion? srcBoxRef)
	{
		IntPtr zero = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<Resource>(srcResourceRef);
		ResourceRegion value = default(ResourceRegion);
		if (srcBoxRef.HasValue)
		{
			value = srcBoxRef.Value;
		}
		void* nativePointer = _nativePointer;
		void* intPtr = (void*)dstDataRef;
		void* intPtr2 = (void*)zero;
		ResourceRegion* intPtr3 = ((!srcBoxRef.HasValue) ? null : (&value));
		((delegate* unmanaged[Stdcall]<void*, void*, int, int, void*, int, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)64 * (nint)sizeof(void*))))(nativePointer, intPtr, dstRowPitch, dstDepthPitch, intPtr2, srcSubresource, intPtr3);
	}
}
