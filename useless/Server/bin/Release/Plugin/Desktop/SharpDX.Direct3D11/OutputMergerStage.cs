using System;
using System.Runtime.InteropServices;
using SharpDX.Mathematics.Interop;

namespace SharpDX.Direct3D11;

[Guid("c0bfa96c-e089-44fb-8eaf-26f8796190da")]
public class OutputMergerStage : CppObject
{
	public const int SimultaneousRenderTargetCount = 8;

	public RawColor4 BlendFactor
	{
		get
		{
			GetBlendState(out var blendStateOut, out var blendFactor, out var _);
			blendStateOut?.Dispose();
			return blendFactor;
		}
		set
		{
			GetBlendState(out var blendStateOut, out var _, out var sampleMaskRef);
			SetBlendState(blendStateOut, value, sampleMaskRef);
			blendStateOut?.Dispose();
		}
	}

	public int BlendSampleMask
	{
		get
		{
			GetBlendState(out var blendStateOut, out var _, out var sampleMaskRef);
			blendStateOut?.Dispose();
			return sampleMaskRef;
		}
		set
		{
			GetBlendState(out var blendStateOut, out var blendFactor, out var _);
			SetBlendState(blendStateOut, blendFactor, value);
			blendStateOut?.Dispose();
		}
	}

	public BlendState BlendState
	{
		get
		{
			GetBlendState(out var blendStateOut, out var _, out var _);
			return blendStateOut;
		}
		set
		{
			GetBlendState(out var blendStateOut, out var blendFactor, out var sampleMaskRef);
			blendStateOut?.Dispose();
			SetBlendState(value, blendFactor, sampleMaskRef);
		}
	}

	public int DepthStencilReference
	{
		get
		{
			GetDepthStencilState(out var depthStencilStateOut, out var stencilRefRef);
			depthStencilStateOut?.Dispose();
			return stencilRefRef;
		}
		set
		{
			GetDepthStencilState(out var depthStencilStateOut, out var _);
			SetDepthStencilState(depthStencilStateOut, value);
			depthStencilStateOut?.Dispose();
		}
	}

	public DepthStencilState DepthStencilState
	{
		get
		{
			GetDepthStencilState(out var depthStencilStateOut, out var _);
			return depthStencilStateOut;
		}
		set
		{
			GetDepthStencilState(out var depthStencilStateOut, out var stencilRefRef);
			depthStencilStateOut?.Dispose();
			SetDepthStencilState(value, stencilRefRef);
		}
	}

	public void GetRenderTargets(out DepthStencilView depthStencilViewRef)
	{
		GetRenderTargets(0, new RenderTargetView[0], out depthStencilViewRef);
	}

	public RenderTargetView[] GetRenderTargets(int numViews)
	{
		RenderTargetView[] array = new RenderTargetView[numViews];
		GetRenderTargets(numViews, array, out var depthStencilViewOut);
		depthStencilViewOut?.Dispose();
		return array;
	}

	public RenderTargetView[] GetRenderTargets(int numViews, out DepthStencilView depthStencilViewRef)
	{
		RenderTargetView[] array = new RenderTargetView[numViews];
		GetRenderTargets(numViews, array, out depthStencilViewRef);
		return array;
	}

	public BlendState GetBlendState(out RawColor4 blendFactor, out int sampleMaskRef)
	{
		GetBlendState(out var blendStateOut, out blendFactor, out sampleMaskRef);
		return blendStateOut;
	}

	public DepthStencilState GetDepthStencilState(out int stencilRefRef)
	{
		GetDepthStencilState(out var depthStencilStateOut, out stencilRefRef);
		return depthStencilStateOut;
	}

	public UnorderedAccessView[] GetUnorderedAccessViews(int startSlot, int count)
	{
		UnorderedAccessView[] array = new UnorderedAccessView[count];
		GetRenderTargetsAndUnorderedAccessViews(0, new RenderTargetView[0], out var depthStencilViewOut, startSlot, count, array);
		depthStencilViewOut.Dispose();
		return array;
	}

	public void ResetTargets()
	{
		SetRenderTargets(0, IntPtr.Zero, null);
	}

	public void SetTargets(params RenderTargetView[] renderTargetViews)
	{
		SetTargets(null, renderTargetViews);
	}

	public void SetTargets(RenderTargetView renderTargetView)
	{
		SetTargets((DepthStencilView)null, renderTargetView);
	}

	public void SetTargets(DepthStencilView depthStencilView, params RenderTargetView[] renderTargetViews)
	{
		SetRenderTargets((renderTargetViews != null) ? renderTargetViews.Length : 0, renderTargetViews, depthStencilView);
	}

	public void SetTargets(DepthStencilView depthStencilView, int renderTargetCount, RenderTargetView[] renderTargetViews)
	{
		SetRenderTargets(renderTargetCount, renderTargetViews, depthStencilView);
	}

	public unsafe void SetTargets(DepthStencilView depthStencilView, RenderTargetView renderTargetView)
	{
		IntPtr intPtr = renderTargetView?.NativePointer ?? IntPtr.Zero;
		SetRenderTargets(1, new IntPtr(&intPtr), depthStencilView);
	}

	public void SetTargets(DepthStencilView depthStencilView, ComArray<RenderTargetView> renderTargetViews)
	{
		SetRenderTargets(renderTargetViews?.Length ?? 0, renderTargetViews?.NativePointer ?? IntPtr.Zero, depthStencilView);
	}

	public void SetTargets(ComArray<RenderTargetView> renderTargetViews)
	{
		SetRenderTargets(renderTargetViews?.Length ?? 0, renderTargetViews?.NativePointer ?? IntPtr.Zero, null);
	}

	public void SetTargets(RenderTargetView renderTargetView, int startSlot, UnorderedAccessView[] unorderedAccessViews)
	{
		SetTargets(startSlot, unorderedAccessViews, renderTargetView);
	}

	public void SetTargets(int startSlot, UnorderedAccessView[] unorderedAccessViews, params RenderTargetView[] renderTargetViews)
	{
		SetTargets(null, startSlot, unorderedAccessViews, renderTargetViews);
	}

	public void SetTargets(DepthStencilView depthStencilView, RenderTargetView renderTargetView, int startSlot, UnorderedAccessView[] unorderedAccessViews)
	{
		SetTargets(depthStencilView, startSlot, unorderedAccessViews, renderTargetView);
	}

	public void SetTargets(DepthStencilView depthStencilView, int startSlot, UnorderedAccessView[] unorderedAccessViews, params RenderTargetView[] renderTargetViews)
	{
		int[] array = new int[unorderedAccessViews.Length];
		for (int i = 0; i < unorderedAccessViews.Length; i++)
		{
			array[i] = -1;
		}
		SetTargets(depthStencilView, startSlot, unorderedAccessViews, array, renderTargetViews);
	}

	public void SetTargets(RenderTargetView renderTargetView, int startSlot, UnorderedAccessView[] unorderedAccessViews, int[] initialLengths)
	{
		SetTargets(startSlot, unorderedAccessViews, initialLengths, renderTargetView);
	}

	public void SetTargets(int startSlot, UnorderedAccessView[] unorderedAccessViews, int[] initialLengths, params RenderTargetView[] renderTargetViews)
	{
		SetTargets(null, startSlot, unorderedAccessViews, initialLengths, renderTargetViews);
	}

	public void SetTargets(DepthStencilView depthStencilView, RenderTargetView renderTargetView, int startSlot, UnorderedAccessView[] unorderedAccessViews, int[] initialLengths)
	{
		SetTargets(depthStencilView, startSlot, unorderedAccessViews, initialLengths, renderTargetView);
	}

	public void SetTargets(DepthStencilView depthStencilView, int startSlot, UnorderedAccessView[] unorderedAccessViews, int[] initialLengths, params RenderTargetView[] renderTargetViews)
	{
		SetRenderTargetsAndUnorderedAccessViews(renderTargetViews.Length, renderTargetViews, depthStencilView, startSlot, unorderedAccessViews.Length, unorderedAccessViews, initialLengths);
	}

	private unsafe void SetRenderTargets(int numViews, RenderTargetView[] renderTargetViews, DepthStencilView depthStencilViewRef)
	{
		IntPtr* ptr = null;
		if (numViews > 0)
		{
			ptr = stackalloc IntPtr[numViews];
			for (int i = 0; i < numViews; i++)
			{
				ptr[i] = ((renderTargetViews[i] == null) ? IntPtr.Zero : renderTargetViews[i].NativePointer);
			}
		}
		SetRenderTargets(numViews, (IntPtr)ptr, depthStencilViewRef);
	}

	public unsafe void SetRenderTargets(DepthStencilView depthStencilView, RenderTargetView renderTargetView)
	{
		IntPtr intPtr = IntPtr.Zero;
		if (renderTargetView != null)
		{
			intPtr = renderTargetView.NativePointer;
		}
		SetRenderTargetsAndKeepUAV(1, new IntPtr(&intPtr), depthStencilView);
	}

	public void SetRenderTargets(RenderTargetView renderTargetView)
	{
		SetRenderTargets(null, renderTargetView);
	}

	public unsafe void SetRenderTargets(DepthStencilView depthStencilView, params RenderTargetView[] renderTargetViews)
	{
		IntPtr* ptr = null;
		int numRTVs = 0;
		if (renderTargetViews != null)
		{
			numRTVs = renderTargetViews.Length;
			ptr = stackalloc IntPtr[renderTargetViews.Length];
			for (int i = 0; i < renderTargetViews.Length; i++)
			{
				ptr[i] = ((renderTargetViews[i] == null) ? IntPtr.Zero : renderTargetViews[i].NativePointer);
			}
		}
		SetRenderTargetsAndKeepUAV(numRTVs, new IntPtr(ptr), depthStencilView);
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
			SetUnorderedAccessViewsKeepRTV(uavCount: (IntPtr)ptr2, startSlot: startSlot, numBuffers: (unorderedAccessViews != null) ? unorderedAccessViews.Length : 0, unorderedAccessBuffer: (IntPtr)ptr);
		}
	}

	internal unsafe void SetRenderTargetsAndUnorderedAccessViews(int numRTVs, RenderTargetView[] renderTargetViewsOut, DepthStencilView depthStencilViewRef, int uAVStartSlot, int numUAVs, UnorderedAccessView[] unorderedAccessViewsOut, int[] uAVInitialCountsRef)
	{
		IntPtr* ptr = null;
		if (renderTargetViewsOut != null)
		{
			ptr = stackalloc IntPtr[renderTargetViewsOut.Length];
			for (int i = 0; i < renderTargetViewsOut.Length; i++)
			{
				ptr[i] = ((renderTargetViewsOut[i] == null) ? IntPtr.Zero : renderTargetViewsOut[i].NativePointer);
			}
		}
		IntPtr* ptr2 = null;
		if (unorderedAccessViewsOut != null)
		{
			ptr2 = stackalloc IntPtr[unorderedAccessViewsOut.Length];
			for (int j = 0; j < unorderedAccessViewsOut.Length; j++)
			{
				ptr2[j] = ((unorderedAccessViewsOut[j] == null) ? IntPtr.Zero : unorderedAccessViewsOut[j].NativePointer);
			}
		}
		fixed (int* ptr3 = uAVInitialCountsRef)
		{
			void* ptr4 = ptr3;
			SetRenderTargetsAndUnorderedAccessViews(numRTVs, (IntPtr)ptr, depthStencilViewRef, uAVStartSlot, numUAVs, (IntPtr)ptr2, (IntPtr)ptr4);
		}
	}

	internal unsafe void SetRenderTargetsAndUnorderedAccessViews(int numRTVs, ComArray<RenderTargetView> renderTargetViewsOut, DepthStencilView depthStencilViewRef, int uAVStartSlot, int numUAVs, ComArray<UnorderedAccessView> unorderedAccessViewsOut, int[] uAVInitialCountsRef)
	{
		fixed (int* ptr = uAVInitialCountsRef)
		{
			SetRenderTargetsAndUnorderedAccessViews(uAVInitialCountsRef: (IntPtr)ptr, numRTVs: numRTVs, renderTargetViewsOut: renderTargetViewsOut?.NativePointer ?? IntPtr.Zero, depthStencilViewRef: depthStencilViewRef, uAVStartSlot: uAVStartSlot, numUAVs: numUAVs, unorderedAccessViewsOut: unorderedAccessViewsOut?.NativePointer ?? IntPtr.Zero);
		}
	}

	internal void SetRenderTargetsAndKeepUAV(int numRTVs, IntPtr rtvs, DepthStencilView depthStencilViewRef)
	{
		SetRenderTargetsAndUnorderedAccessViews(numRTVs, rtvs, depthStencilViewRef, 0, -1, IntPtr.Zero, IntPtr.Zero);
	}

	internal void SetUnorderedAccessViewsKeepRTV(int startSlot, int numBuffers, IntPtr unorderedAccessBuffer, IntPtr uavCount)
	{
		SetRenderTargetsAndUnorderedAccessViews(-1, IntPtr.Zero, null, startSlot, numBuffers, unorderedAccessBuffer, uavCount);
	}

	public void SetBlendState(BlendState blendStateRef, RawColor4? blendFactor, uint sampleMask)
	{
		SetBlendState(blendStateRef, blendFactor, (int)sampleMask);
	}

	public OutputMergerStage(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator OutputMergerStage(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new OutputMergerStage(nativePtr);
		}
		return null;
	}

	internal unsafe void SetRenderTargets(int numViews, IntPtr renderTargetViewsOut, DepthStencilView depthStencilViewRef)
	{
		IntPtr zero = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<DepthStencilView>(depthStencilViewRef);
		((delegate* unmanaged[Stdcall]<void*, int, void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)33 * (nint)sizeof(void*))))(_nativePointer, numViews, (void*)renderTargetViewsOut, (void*)zero);
	}

	internal unsafe void SetRenderTargetsAndUnorderedAccessViews(int numRTVs, IntPtr renderTargetViewsOut, DepthStencilView depthStencilViewRef, int uAVStartSlot, int numUAVs, IntPtr unorderedAccessViewsOut, IntPtr uAVInitialCountsRef)
	{
		IntPtr zero = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<DepthStencilView>(depthStencilViewRef);
		((delegate* unmanaged[Stdcall]<void*, int, void*, void*, int, int, void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)34 * (nint)sizeof(void*))))(_nativePointer, numRTVs, (void*)renderTargetViewsOut, (void*)zero, uAVStartSlot, numUAVs, (void*)unorderedAccessViewsOut, (void*)uAVInitialCountsRef);
	}

	public unsafe void SetBlendState(BlendState blendStateRef, RawColor4? blendFactor = null, int sampleMask = -1)
	{
		IntPtr zero = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<BlendState>(blendStateRef);
		RawColor4 value = default(RawColor4);
		if (blendFactor.HasValue)
		{
			value = blendFactor.Value;
		}
		void* nativePointer = _nativePointer;
		void* intPtr = (void*)zero;
		RawColor4* intPtr2 = ((!blendFactor.HasValue) ? null : (&value));
		((delegate* unmanaged[Stdcall]<void*, void*, void*, int, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)35 * (nint)sizeof(void*))))(nativePointer, intPtr, intPtr2, sampleMask);
	}

	public unsafe void SetDepthStencilState(DepthStencilState depthStencilStateRef, int stencilRef = 0)
	{
		IntPtr zero = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<DepthStencilState>(depthStencilStateRef);
		((delegate* unmanaged[Stdcall]<void*, void*, int, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)36 * (nint)sizeof(void*))))(_nativePointer, (void*)zero, stencilRef);
	}

	internal unsafe void GetRenderTargets(int numViews, RenderTargetView[] renderTargetViewsOut, out DepthStencilView depthStencilViewOut)
	{
		IntPtr* ptr = null;
		if (renderTargetViewsOut != null)
		{
			ptr = stackalloc IntPtr[renderTargetViewsOut.Length];
		}
		IntPtr zero = IntPtr.Zero;
		((delegate* unmanaged[Stdcall]<void*, int, void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)89 * (nint)sizeof(void*))))(_nativePointer, numViews, ptr, &zero);
		if (renderTargetViewsOut != null)
		{
			for (int i = 0; i < renderTargetViewsOut.Length; i++)
			{
				if (ptr[i] != IntPtr.Zero)
				{
					renderTargetViewsOut[i] = new RenderTargetView(ptr[i]);
				}
				else
				{
					renderTargetViewsOut[i] = null;
				}
			}
		}
		if (zero != IntPtr.Zero)
		{
			depthStencilViewOut = new DepthStencilView(zero);
		}
		else
		{
			depthStencilViewOut = null;
		}
	}

	internal unsafe void GetRenderTargetsAndUnorderedAccessViews(int numRTVs, RenderTargetView[] renderTargetViewsOut, out DepthStencilView depthStencilViewOut, int uAVStartSlot, int numUAVs, UnorderedAccessView[] unorderedAccessViewsOut)
	{
		IntPtr* ptr = null;
		if (renderTargetViewsOut != null)
		{
			ptr = stackalloc IntPtr[renderTargetViewsOut.Length];
		}
		IntPtr zero = IntPtr.Zero;
		IntPtr* ptr2 = null;
		if (unorderedAccessViewsOut != null)
		{
			ptr2 = stackalloc IntPtr[unorderedAccessViewsOut.Length];
		}
		((delegate* unmanaged[Stdcall]<void*, int, void*, void*, int, int, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)90 * (nint)sizeof(void*))))(_nativePointer, numRTVs, ptr, &zero, uAVStartSlot, numUAVs, ptr2);
		if (renderTargetViewsOut != null)
		{
			for (int i = 0; i < renderTargetViewsOut.Length; i++)
			{
				if (ptr[i] != IntPtr.Zero)
				{
					renderTargetViewsOut[i] = new RenderTargetView(ptr[i]);
				}
				else
				{
					renderTargetViewsOut[i] = null;
				}
			}
		}
		if (zero != IntPtr.Zero)
		{
			depthStencilViewOut = new DepthStencilView(zero);
		}
		else
		{
			depthStencilViewOut = null;
		}
		if (unorderedAccessViewsOut == null)
		{
			return;
		}
		for (int j = 0; j < unorderedAccessViewsOut.Length; j++)
		{
			if (ptr2[j] != IntPtr.Zero)
			{
				unorderedAccessViewsOut[j] = new UnorderedAccessView(ptr2[j]);
			}
			else
			{
				unorderedAccessViewsOut[j] = null;
			}
		}
	}

	internal unsafe void GetBlendState(out BlendState blendStateOut, out RawColor4 blendFactor, out int sampleMaskRef)
	{
		IntPtr zero = IntPtr.Zero;
		blendFactor = default(RawColor4);
		fixed (int* ptr = &sampleMaskRef)
		{
			void* ptr2 = ptr;
			fixed (RawColor4* ptr3 = &blendFactor)
			{
				void* ptr4 = ptr3;
				((delegate* unmanaged[Stdcall]<void*, void*, void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)91 * (nint)sizeof(void*))))(_nativePointer, &zero, ptr4, ptr2);
			}
		}
		if (zero != IntPtr.Zero)
		{
			blendStateOut = new BlendState(zero);
		}
		else
		{
			blendStateOut = null;
		}
	}

	internal unsafe void GetDepthStencilState(out DepthStencilState depthStencilStateOut, out int stencilRefRef)
	{
		IntPtr zero = IntPtr.Zero;
		fixed (int* ptr = &stencilRefRef)
		{
			void* ptr2 = ptr;
			((delegate* unmanaged[Stdcall]<void*, void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)92 * (nint)sizeof(void*))))(_nativePointer, &zero, ptr2);
		}
		if (zero != IntPtr.Zero)
		{
			depthStencilStateOut = new DepthStencilState(zero);
		}
		else
		{
			depthStencilStateOut = null;
		}
	}
}
