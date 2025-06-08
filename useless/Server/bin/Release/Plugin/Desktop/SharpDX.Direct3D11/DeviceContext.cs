using System;
using System.Globalization;
using System.Runtime.InteropServices;
using SharpDX.DXGI;
using SharpDX.Mathematics.Interop;

namespace SharpDX.Direct3D11;

[Guid("c0bfa96c-e089-44fb-8eaf-26f8796190da")]
public class DeviceContext : DeviceChild
{
	private bool isCheckThreadingSupport;

	private bool supportsCommandLists;

	public VertexShaderStage VertexShader { get; private set; }

	public PixelShaderStage PixelShader { get; private set; }

	public InputAssemblerStage InputAssembler { get; private set; }

	public GeometryShaderStage GeometryShader { get; private set; }

	public OutputMergerStage OutputMerger { get; private set; }

	public StreamOutputStage StreamOutput { get; private set; }

	public RasterizerStage Rasterizer { get; private set; }

	public HullShaderStage HullShader { get; private set; }

	public DomainShaderStage DomainShader { get; private set; }

	public ComputeShaderStage ComputeShader { get; private set; }

	public DeviceContextType TypeInfo => GetTypeInfo();

	public int ContextFlags => GetContextFlags();

	public DeviceContext(Device device)
		: base(IntPtr.Zero)
	{
		device.CreateDeferredContext(0, this);
		((IUnknown)device).AddReference();
		Device__ = device;
	}

	public CommandList FinishCommandList(bool restoreState)
	{
		FinishCommandListInternal(restoreState, out var commandListOut);
		return commandListOut;
	}

	public bool IsDataAvailable(Asynchronous data)
	{
		return IsDataAvailable(data, AsynchronousFlags.None);
	}

	public bool IsDataAvailable(Asynchronous data, AsynchronousFlags flags)
	{
		return GetDataInternal(data, IntPtr.Zero, 0, flags) == Result.Ok;
	}

	public DataStream GetData(Asynchronous data)
	{
		return GetData(data, AsynchronousFlags.None);
	}

	public T GetData<T>(Asynchronous data) where T : struct
	{
		return GetData<T>(data, AsynchronousFlags.None);
	}

	public bool GetData<T>(Asynchronous data, out T result) where T : struct
	{
		return GetData<T>(data, AsynchronousFlags.None, out result);
	}

	public DataStream GetData(Asynchronous data, AsynchronousFlags flags)
	{
		DataStream dataStream = new DataStream(data.DataSize, canRead: true, canWrite: true);
		GetDataInternal(data, dataStream.DataPointer, (int)dataStream.Length, flags);
		return dataStream;
	}

	public T GetData<T>(Asynchronous data, AsynchronousFlags flags) where T : struct
	{
		GetData<T>(data, flags, out var result);
		return result;
	}

	public unsafe bool GetData<T>(Asynchronous data, AsynchronousFlags flags, out T result) where T : struct
	{
		result = default(T);
		fixed (T* ptr = &result)
		{
			return GetDataInternal(data, (IntPtr)ptr, Utilities.SizeOf<T>(), flags) == Result.Ok;
		}
	}

	public void CopyResource(Resource source, Resource destination)
	{
		CopyResource_(destination, source);
	}

	public void CopySubresourceRegion(Resource source, int sourceSubresource, ResourceRegion? sourceRegion, Resource destination, int destinationSubResource, int dstX = 0, int dstY = 0, int dstZ = 0)
	{
		CopySubresourceRegion_(destination, destinationSubResource, dstX, dstY, dstZ, source, sourceSubresource, sourceRegion);
	}

	public void ResolveSubresource(Resource source, int sourceSubresource, Resource destination, int destinationSubresource, Format format)
	{
		ResolveSubresource_(destination, destinationSubresource, source, sourceSubresource, format);
	}

	public DataBox MapSubresource(Texture1D resource, int mipSlice, int arraySlice, MapMode mode, MapFlags flags, out DataStream stream)
	{
		int mipSize;
		DataBox result = MapSubresource((Resource)resource, mipSlice, arraySlice, mode, flags, out mipSize);
		stream = new DataStream(result.DataPointer, mipSize * resource.Description.Format.SizeOfInBytes(), canRead: true, canWrite: true);
		return result;
	}

	public DataBox MapSubresource(Texture2D resource, int mipSlice, int arraySlice, MapMode mode, MapFlags flags, out DataStream stream)
	{
		int mipSize;
		DataBox result = MapSubresource((Resource)resource, mipSlice, arraySlice, mode, flags, out mipSize);
		stream = new DataStream(result.DataPointer, mipSize * result.RowPitch, canRead: true, canWrite: true);
		return result;
	}

	public DataBox MapSubresource(Texture3D resource, int mipSlice, int arraySlice, MapMode mode, MapFlags flags, out DataStream stream)
	{
		int mipSize;
		DataBox result = MapSubresource((Resource)resource, mipSlice, arraySlice, mode, flags, out mipSize);
		stream = new DataStream(result.DataPointer, mipSize * result.SlicePitch, canRead: true, canWrite: true);
		return result;
	}

	public DataBox MapSubresource(Buffer resource, MapMode mode, MapFlags flags, out DataStream stream)
	{
		DataBox result = MapSubresource(resource, 0, mode, flags);
		stream = new DataStream(result.DataPointer, resource.Description.SizeInBytes, canRead: true, canWrite: true);
		return result;
	}

	public DataBox MapSubresource(Resource resource, int mipSlice, int arraySlice, MapMode mode, MapFlags flags, out int mipSize)
	{
		int subresource = resource.CalculateSubResourceIndex(mipSlice, arraySlice, out mipSize);
		return MapSubresource(resource, subresource, mode, flags);
	}

	public DataBox MapSubresource(Resource resource, int subresource, MapMode mode, MapFlags flags, out DataStream stream)
	{
		switch (resource.Dimension)
		{
		case ResourceDimension.Buffer:
			return MapSubresource((Buffer)resource, mode, flags, out stream);
		case ResourceDimension.Texture1D:
		{
			Texture1D texture1D = (Texture1D)resource;
			int mipLevels = texture1D.Description.MipLevels;
			return MapSubresource(texture1D, subresource % mipLevels, subresource / mipLevels, mode, flags, out stream);
		}
		case ResourceDimension.Texture2D:
		{
			Texture2D texture2D = (Texture2D)resource;
			int mipLevels = texture2D.Description.MipLevels;
			return MapSubresource(texture2D, subresource % mipLevels, subresource / mipLevels, mode, flags, out stream);
		}
		case ResourceDimension.Texture3D:
		{
			Texture3D texture3D = (Texture3D)resource;
			int mipLevels = texture3D.Description.MipLevels;
			return MapSubresource(texture3D, subresource % mipLevels, subresource / mipLevels, mode, flags, out stream);
		}
		default:
			throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "MapSubresource is not supported for Resource [{0}]", new object[1] { resource.Dimension }));
		}
	}

	public DataBox MapSubresource(Resource resourceRef, int subresource, MapMode mapType, MapFlags mapFlags)
	{
		DataBox mappedResourceRef = default(DataBox);
		Result result = MapSubresource(resourceRef, subresource, mapType, mapFlags, out mappedResourceRef);
		if ((mapFlags & MapFlags.DoNotWait) != 0 && result == SharpDX.DXGI.ResultCode.WasStillDrawing)
		{
			return mappedResourceRef;
		}
		result.CheckError();
		return mappedResourceRef;
	}

	public unsafe void UpdateSubresource<T>(ref T data, Resource resource, int subresource = 0, int rowPitch = 0, int depthPitch = 0, ResourceRegion? region = null) where T : struct
	{
		fixed (T* ptr = &data)
		{
			UpdateSubresource(resource, subresource, region, (IntPtr)ptr, rowPitch, depthPitch);
		}
	}

	public unsafe void UpdateSubresource<T>(T[] data, Resource resource, int subresource = 0, int rowPitch = 0, int depthPitch = 0, ResourceRegion? region = null) where T : struct
	{
		fixed (T* ptr = &data[0])
		{
			UpdateSubresource(resource, subresource, region, (IntPtr)ptr, rowPitch, depthPitch);
		}
	}

	public void UpdateSubresource(DataBox source, Resource resource, int subresource = 0)
	{
		UpdateSubresource(resource, subresource, null, source.DataPointer, source.RowPitch, source.SlicePitch);
	}

	public void UpdateSubresource(DataBox source, Resource resource, int subresource, ResourceRegion region)
	{
		UpdateSubresource(resource, subresource, region, source.DataPointer, source.RowPitch, source.SlicePitch);
	}

	public unsafe void UpdateSubresourceSafe<T>(ref T data, Resource resource, int srcBytesPerElement, int subresource = 0, int rowPitch = 0, int depthPitch = 0, bool isCompressedResource = false) where T : struct
	{
		fixed (T* ptr = &data)
		{
			UpdateSubresourceSafe(resource, subresource, null, (IntPtr)ptr, rowPitch, depthPitch, srcBytesPerElement, isCompressedResource);
		}
	}

	public unsafe void UpdateSubresourceSafe<T>(T[] data, Resource resource, int srcBytesPerElement, int subresource = 0, int rowPitch = 0, int depthPitch = 0, bool isCompressedResource = false) where T : struct
	{
		fixed (T* ptr = &data[0])
		{
			UpdateSubresourceSafe(resource, subresource, null, (IntPtr)ptr, rowPitch, depthPitch, srcBytesPerElement, isCompressedResource);
		}
	}

	public void UpdateSubresourceSafe(DataBox source, Resource resource, int srcBytesPerElement, int subresource = 0, bool isCompressedResource = false)
	{
		UpdateSubresourceSafe(resource, subresource, null, source.DataPointer, source.RowPitch, source.SlicePitch, srcBytesPerElement, isCompressedResource);
	}

	public void UpdateSubresourceSafe(DataBox source, Resource resource, int srcBytesPerElement, int subresource, ResourceRegion region, bool isCompressedResource = false)
	{
		UpdateSubresourceSafe(resource, subresource, region, source.DataPointer, source.RowPitch, source.SlicePitch, srcBytesPerElement, isCompressedResource);
	}

	internal unsafe bool UpdateSubresourceSafe(Resource dstResourceRef, int dstSubresource, ResourceRegion? dstBoxRef, IntPtr pSrcData, int srcRowPitch, int srcDepthPitch, int srcBytesPerElement, bool isCompressedResource)
	{
		bool flag = false;
		if (!isCheckThreadingSupport)
		{
			base.Device.CheckThreadingSupport(out var _, out supportsCommandLists);
			isCheckThreadingSupport = true;
		}
		if (dstBoxRef.HasValue && TypeInfo == DeviceContextType.Deferred)
		{
			flag = !supportsCommandLists;
		}
		IntPtr srcDataRef = pSrcData;
		if (flag)
		{
			ResourceRegion value = dstBoxRef.Value;
			if (isCompressedResource)
			{
				value.Left /= 4;
				value.Right /= 4;
				value.Top /= 4;
				value.Bottom /= 4;
			}
			srcDataRef = (IntPtr)((byte*)(void*)pSrcData - value.Front * srcDepthPitch - value.Top * srcRowPitch - value.Left * srcBytesPerElement);
		}
		UpdateSubresource(dstResourceRef, dstSubresource, dstBoxRef, srcDataRef, srcRowPitch, srcDepthPitch);
		return flag;
	}

	public DeviceContext(IntPtr nativePtr)
		: base(nativePtr)
	{
		VertexShader = new VertexShaderStage(nativePtr);
		PixelShader = new PixelShaderStage(nativePtr);
		InputAssembler = new InputAssemblerStage(nativePtr);
		GeometryShader = new GeometryShaderStage(nativePtr);
		OutputMerger = new OutputMergerStage(nativePtr);
		StreamOutput = new StreamOutputStage(nativePtr);
		Rasterizer = new RasterizerStage(nativePtr);
		HullShader = new HullShaderStage(nativePtr);
		DomainShader = new DomainShaderStage(nativePtr);
		ComputeShader = new ComputeShaderStage(nativePtr);
	}

	public static explicit operator DeviceContext(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new DeviceContext(nativePtr);
		}
		return null;
	}

	protected override void NativePointerUpdated(IntPtr oldPointer)
	{
		base.NativePointerUpdated(oldPointer);
		if (VertexShader == null)
		{
			VertexShader = new VertexShaderStage(IntPtr.Zero);
		}
		VertexShader.NativePointer = base.NativePointer;
		if (PixelShader == null)
		{
			PixelShader = new PixelShaderStage(IntPtr.Zero);
		}
		PixelShader.NativePointer = base.NativePointer;
		if (InputAssembler == null)
		{
			InputAssembler = new InputAssemblerStage(IntPtr.Zero);
		}
		InputAssembler.NativePointer = base.NativePointer;
		if (GeometryShader == null)
		{
			GeometryShader = new GeometryShaderStage(IntPtr.Zero);
		}
		GeometryShader.NativePointer = base.NativePointer;
		if (OutputMerger == null)
		{
			OutputMerger = new OutputMergerStage(IntPtr.Zero);
		}
		OutputMerger.NativePointer = base.NativePointer;
		if (StreamOutput == null)
		{
			StreamOutput = new StreamOutputStage(IntPtr.Zero);
		}
		StreamOutput.NativePointer = base.NativePointer;
		if (Rasterizer == null)
		{
			Rasterizer = new RasterizerStage(IntPtr.Zero);
		}
		Rasterizer.NativePointer = base.NativePointer;
		if (HullShader == null)
		{
			HullShader = new HullShaderStage(IntPtr.Zero);
		}
		HullShader.NativePointer = base.NativePointer;
		if (DomainShader == null)
		{
			DomainShader = new DomainShaderStage(IntPtr.Zero);
		}
		DomainShader.NativePointer = base.NativePointer;
		if (ComputeShader == null)
		{
			ComputeShader = new ComputeShaderStage(IntPtr.Zero);
		}
		ComputeShader.NativePointer = base.NativePointer;
	}

	public unsafe void DrawIndexed(int indexCount, int startIndexLocation, int baseVertexLocation)
	{
		((delegate* unmanaged[Stdcall]<void*, int, int, int, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)12 * (nint)sizeof(void*))))(_nativePointer, indexCount, startIndexLocation, baseVertexLocation);
	}

	public unsafe void Draw(int vertexCount, int startVertexLocation)
	{
		((delegate* unmanaged[Stdcall]<void*, int, int, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)13 * (nint)sizeof(void*))))(_nativePointer, vertexCount, startVertexLocation);
	}

	internal unsafe Result MapSubresource(Resource resourceRef, int subresource, MapMode mapType, MapFlags mapFlags, out DataBox mappedResourceRef)
	{
		IntPtr zero = IntPtr.Zero;
		mappedResourceRef = default(DataBox);
		zero = CppObject.ToCallbackPtr<Resource>(resourceRef);
		Result result;
		fixed (DataBox* ptr = &mappedResourceRef)
		{
			void* ptr2 = ptr;
			result = ((delegate* unmanaged[Stdcall]<void*, void*, int, int, int, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)14 * (nint)sizeof(void*))))(_nativePointer, (void*)zero, subresource, (int)mapType, (int)mapFlags, ptr2);
		}
		return result;
	}

	public unsafe void UnmapSubresource(Resource resourceRef, int subresource)
	{
		IntPtr zero = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<Resource>(resourceRef);
		((delegate* unmanaged[Stdcall]<void*, void*, int, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)15 * (nint)sizeof(void*))))(_nativePointer, (void*)zero, subresource);
	}

	public unsafe void DrawIndexedInstanced(int indexCountPerInstance, int instanceCount, int startIndexLocation, int baseVertexLocation, int startInstanceLocation)
	{
		((delegate* unmanaged[Stdcall]<void*, int, int, int, int, int, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)20 * (nint)sizeof(void*))))(_nativePointer, indexCountPerInstance, instanceCount, startIndexLocation, baseVertexLocation, startInstanceLocation);
	}

	public unsafe void DrawInstanced(int vertexCountPerInstance, int instanceCount, int startVertexLocation, int startInstanceLocation)
	{
		((delegate* unmanaged[Stdcall]<void*, int, int, int, int, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)21 * (nint)sizeof(void*))))(_nativePointer, vertexCountPerInstance, instanceCount, startVertexLocation, startInstanceLocation);
	}

	public unsafe void Begin(Asynchronous asyncRef)
	{
		IntPtr zero = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<Asynchronous>(asyncRef);
		((delegate* unmanaged[Stdcall]<void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)27 * (nint)sizeof(void*))))(_nativePointer, (void*)zero);
	}

	public unsafe void End(Asynchronous asyncRef)
	{
		IntPtr zero = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<Asynchronous>(asyncRef);
		((delegate* unmanaged[Stdcall]<void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)28 * (nint)sizeof(void*))))(_nativePointer, (void*)zero);
	}

	internal unsafe Result GetDataInternal(Asynchronous asyncRef, IntPtr dataRef, int dataSize, AsynchronousFlags getDataFlags)
	{
		IntPtr zero = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<Asynchronous>(asyncRef);
		return ((delegate* unmanaged[Stdcall]<void*, void*, void*, int, int, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)29 * (nint)sizeof(void*))))(_nativePointer, (void*)zero, (void*)dataRef, dataSize, (int)getDataFlags);
	}

	public unsafe void SetPredication(Predicate predicateRef, RawBool predicateValue)
	{
		IntPtr zero = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<Predicate>(predicateRef);
		((delegate* unmanaged[Stdcall]<void*, void*, RawBool, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)30 * (nint)sizeof(void*))))(_nativePointer, (void*)zero, predicateValue);
	}

	public unsafe void DrawAuto()
	{
		((delegate* unmanaged[Stdcall]<void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)38 * (nint)sizeof(void*))))(_nativePointer);
	}

	public unsafe void DrawIndexedInstancedIndirect(Buffer bufferForArgsRef, int alignedByteOffsetForArgs)
	{
		IntPtr zero = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<Buffer>(bufferForArgsRef);
		((delegate* unmanaged[Stdcall]<void*, void*, int, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)39 * (nint)sizeof(void*))))(_nativePointer, (void*)zero, alignedByteOffsetForArgs);
	}

	public unsafe void DrawInstancedIndirect(Buffer bufferForArgsRef, int alignedByteOffsetForArgs)
	{
		IntPtr zero = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<Buffer>(bufferForArgsRef);
		((delegate* unmanaged[Stdcall]<void*, void*, int, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)40 * (nint)sizeof(void*))))(_nativePointer, (void*)zero, alignedByteOffsetForArgs);
	}

	public unsafe void Dispatch(int threadGroupCountX, int threadGroupCountY, int threadGroupCountZ)
	{
		((delegate* unmanaged[Stdcall]<void*, int, int, int, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)41 * (nint)sizeof(void*))))(_nativePointer, threadGroupCountX, threadGroupCountY, threadGroupCountZ);
	}

	public unsafe void DispatchIndirect(Buffer bufferForArgsRef, int alignedByteOffsetForArgs)
	{
		IntPtr zero = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<Buffer>(bufferForArgsRef);
		((delegate* unmanaged[Stdcall]<void*, void*, int, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)42 * (nint)sizeof(void*))))(_nativePointer, (void*)zero, alignedByteOffsetForArgs);
	}

	internal unsafe void CopySubresourceRegion_(Resource dstResourceRef, int dstSubresource, int dstX, int dstY, int dstZ, Resource srcResourceRef, int srcSubresource, ResourceRegion? srcBoxRef)
	{
		IntPtr zero = IntPtr.Zero;
		IntPtr zero2 = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<Resource>(dstResourceRef);
		zero2 = CppObject.ToCallbackPtr<Resource>(srcResourceRef);
		ResourceRegion value = default(ResourceRegion);
		if (srcBoxRef.HasValue)
		{
			value = srcBoxRef.Value;
		}
		void* nativePointer = _nativePointer;
		void* intPtr = (void*)zero;
		void* intPtr2 = (void*)zero2;
		ResourceRegion* intPtr3 = ((!srcBoxRef.HasValue) ? null : (&value));
		((delegate* unmanaged[Stdcall]<void*, void*, int, int, int, int, void*, int, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)46 * (nint)sizeof(void*))))(nativePointer, intPtr, dstSubresource, dstX, dstY, dstZ, intPtr2, srcSubresource, intPtr3);
	}

	internal unsafe void CopyResource_(Resource dstResourceRef, Resource srcResourceRef)
	{
		IntPtr zero = IntPtr.Zero;
		IntPtr zero2 = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<Resource>(dstResourceRef);
		zero2 = CppObject.ToCallbackPtr<Resource>(srcResourceRef);
		((delegate* unmanaged[Stdcall]<void*, void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)47 * (nint)sizeof(void*))))(_nativePointer, (void*)zero, (void*)zero2);
	}

	public unsafe void UpdateSubresource(Resource dstResourceRef, int dstSubresource, ResourceRegion? dstBoxRef, IntPtr srcDataRef, int srcRowPitch, int srcDepthPitch)
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
		((delegate* unmanaged[Stdcall]<void*, void*, int, void*, void*, int, int, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)48 * (nint)sizeof(void*))))(nativePointer, intPtr, dstSubresource, intPtr2, (void*)srcDataRef, srcRowPitch, srcDepthPitch);
	}

	public unsafe void CopyStructureCount(Buffer dstBufferRef, int dstAlignedByteOffset, UnorderedAccessView srcViewRef)
	{
		IntPtr zero = IntPtr.Zero;
		IntPtr zero2 = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<Buffer>(dstBufferRef);
		zero2 = CppObject.ToCallbackPtr<UnorderedAccessView>(srcViewRef);
		((delegate* unmanaged[Stdcall]<void*, void*, int, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)49 * (nint)sizeof(void*))))(_nativePointer, (void*)zero, dstAlignedByteOffset, (void*)zero2);
	}

	public unsafe void ClearRenderTargetView(RenderTargetView renderTargetViewRef, RawColor4 colorRGBA)
	{
		IntPtr zero = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<RenderTargetView>(renderTargetViewRef);
		((delegate* unmanaged[Stdcall]<void*, void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)50 * (nint)sizeof(void*))))(_nativePointer, (void*)zero, &colorRGBA);
	}

	public unsafe void ClearUnorderedAccessView(UnorderedAccessView unorderedAccessViewRef, RawInt4 values)
	{
		IntPtr zero = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<UnorderedAccessView>(unorderedAccessViewRef);
		((delegate* unmanaged[Stdcall]<void*, void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)51 * (nint)sizeof(void*))))(_nativePointer, (void*)zero, &values);
	}

	public unsafe void ClearUnorderedAccessView(UnorderedAccessView unorderedAccessViewRef, RawVector4 values)
	{
		IntPtr zero = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<UnorderedAccessView>(unorderedAccessViewRef);
		((delegate* unmanaged[Stdcall]<void*, void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)52 * (nint)sizeof(void*))))(_nativePointer, (void*)zero, &values);
	}

	public unsafe void ClearDepthStencilView(DepthStencilView depthStencilViewRef, DepthStencilClearFlags clearFlags, float depth, byte stencil)
	{
		IntPtr zero = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<DepthStencilView>(depthStencilViewRef);
		((delegate* unmanaged[Stdcall]<void*, void*, int, float, byte, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)53 * (nint)sizeof(void*))))(_nativePointer, (void*)zero, (int)clearFlags, depth, stencil);
	}

	public unsafe void GenerateMips(ShaderResourceView shaderResourceViewRef)
	{
		IntPtr zero = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<ShaderResourceView>(shaderResourceViewRef);
		((delegate* unmanaged[Stdcall]<void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)54 * (nint)sizeof(void*))))(_nativePointer, (void*)zero);
	}

	public unsafe void SetMinimumLod(Resource resourceRef, float minLOD)
	{
		IntPtr zero = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<Resource>(resourceRef);
		((delegate* unmanaged[Stdcall]<void*, void*, float, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)55 * (nint)sizeof(void*))))(_nativePointer, (void*)zero, minLOD);
	}

	public unsafe float GetMinimumLod(Resource resourceRef)
	{
		IntPtr zero = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<Resource>(resourceRef);
		return ((delegate* unmanaged[Stdcall]<void*, void*, float>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)56 * (nint)sizeof(void*))))(_nativePointer, (void*)zero);
	}

	internal unsafe void ResolveSubresource_(Resource dstResourceRef, int dstSubresource, Resource srcResourceRef, int srcSubresource, Format format)
	{
		IntPtr zero = IntPtr.Zero;
		IntPtr zero2 = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<Resource>(dstResourceRef);
		zero2 = CppObject.ToCallbackPtr<Resource>(srcResourceRef);
		((delegate* unmanaged[Stdcall]<void*, void*, int, void*, int, int, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)57 * (nint)sizeof(void*))))(_nativePointer, (void*)zero, dstSubresource, (void*)zero2, srcSubresource, (int)format);
	}

	public unsafe void ExecuteCommandList(CommandList commandListRef, RawBool restoreContextState)
	{
		IntPtr zero = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<CommandList>(commandListRef);
		((delegate* unmanaged[Stdcall]<void*, void*, RawBool, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)58 * (nint)sizeof(void*))))(_nativePointer, (void*)zero, restoreContextState);
	}

	public unsafe Predicate GetPredication(out RawBool predicateValueRef)
	{
		IntPtr zero = IntPtr.Zero;
		predicateValueRef = default(RawBool);
		fixed (RawBool* ptr = &predicateValueRef)
		{
			void* ptr2 = ptr;
			((delegate* unmanaged[Stdcall]<void*, void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)86 * (nint)sizeof(void*))))(_nativePointer, &zero, ptr2);
		}
		if (zero != IntPtr.Zero)
		{
			return new Predicate(zero);
		}
		return null;
	}

	public unsafe void ClearState()
	{
		((delegate* unmanaged[Stdcall]<void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)110 * (nint)sizeof(void*))))(_nativePointer);
	}

	public unsafe void Flush()
	{
		((delegate* unmanaged[Stdcall]<void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)111 * (nint)sizeof(void*))))(_nativePointer);
	}

	internal unsafe DeviceContextType GetTypeInfo()
	{
		return ((delegate* unmanaged[Stdcall]<void*, DeviceContextType>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)112 * (nint)sizeof(void*))))(_nativePointer);
	}

	internal unsafe int GetContextFlags()
	{
		return ((delegate* unmanaged[Stdcall]<void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)113 * (nint)sizeof(void*))))(_nativePointer);
	}

	internal unsafe void FinishCommandListInternal(RawBool restoreDeferredContextState, out CommandList commandListOut)
	{
		IntPtr zero = IntPtr.Zero;
		Result result = ((delegate* unmanaged[Stdcall]<void*, RawBool, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)114 * (nint)sizeof(void*))))(_nativePointer, restoreDeferredContextState, &zero);
		if (zero != IntPtr.Zero)
		{
			commandListOut = new CommandList(zero);
		}
		else
		{
			commandListOut = null;
		}
		result.CheckError();
	}
}
