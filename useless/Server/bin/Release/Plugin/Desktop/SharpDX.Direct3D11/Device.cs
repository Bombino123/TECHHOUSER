using System;
using System.Runtime.InteropServices;
using SharpDX.DXGI;
using SharpDX.Direct3D;

namespace SharpDX.Direct3D11;

[Guid("db6f6ddb-ac77-4e88-8253-819df9bbf140")]
public class Device : ComObject
{
	public const int MultisampleCountMaximum = 32;

	protected internal DeviceContext ImmediateContext__;

	public bool IsReferenceDevice
	{
		get
		{
			try
			{
				using SwitchToRef switchToRef = QueryInterface<SwitchToRef>();
				return switchToRef.GetUseRef();
			}
			catch (SharpDXException)
			{
				return false;
			}
		}
	}

	public unsafe string DebugName
	{
		get
		{
			byte* ptr = stackalloc byte[1024];
			int dataSizeRef = 1023;
			if (GetPrivateData(CommonGuid.DebugObjectName, ref dataSizeRef, new IntPtr(ptr)).Failure)
			{
				return string.Empty;
			}
			ptr[dataSizeRef] = 0;
			return Marshal.PtrToStringAnsi(new IntPtr(ptr));
		}
		set
		{
			if (string.IsNullOrEmpty(value))
			{
				SetPrivateData(CommonGuid.DebugObjectName, 0, IntPtr.Zero);
				return;
			}
			IntPtr dataRef = Utilities.StringToHGlobalAnsi(value);
			SetPrivateData(CommonGuid.DebugObjectName, value.Length, dataRef);
		}
	}

	public FeatureLevel FeatureLevel => GetFeatureLevel();

	public DeviceCreationFlags CreationFlags => GetCreationFlags();

	public Result DeviceRemovedReason => GetDeviceRemovedReason();

	public DeviceContext ImmediateContext
	{
		get
		{
			if (ImmediateContext__ == null)
			{
				GetImmediateContext(out ImmediateContext__);
			}
			return ImmediateContext__;
		}
	}

	public int ExceptionMode
	{
		get
		{
			return GetExceptionMode();
		}
		set
		{
			SetExceptionMode(value);
		}
	}

	public Device(DriverType driverType)
		: this(driverType, DeviceCreationFlags.None)
	{
	}

	public Device(Adapter adapter)
		: this(adapter, DeviceCreationFlags.None)
	{
	}

	public Device(DriverType driverType, DeviceCreationFlags flags)
	{
		CreateDevice(null, driverType, flags, null);
	}

	public Device(Adapter adapter, DeviceCreationFlags flags)
	{
		CreateDevice(adapter, DriverType.Unknown, flags, null);
	}

	public Device(DriverType driverType, DeviceCreationFlags flags, params FeatureLevel[] featureLevels)
	{
		CreateDevice(null, driverType, flags, featureLevels);
	}

	public Device(Adapter adapter, DeviceCreationFlags flags, params FeatureLevel[] featureLevels)
	{
		CreateDevice(adapter, DriverType.Unknown, flags, featureLevels);
	}

	public static void CreateWithSwapChain(DriverType driverType, DeviceCreationFlags flags, SwapChainDescription swapChainDescription, out Device device, out SwapChain swapChain)
	{
		CreateWithSwapChain(null, driverType, flags, null, swapChainDescription, out device, out swapChain);
	}

	public static void CreateWithSwapChain(Adapter adapter, DeviceCreationFlags flags, SwapChainDescription swapChainDescription, out Device device, out SwapChain swapChain)
	{
		CreateWithSwapChain(adapter, DriverType.Unknown, flags, null, swapChainDescription, out device, out swapChain);
	}

	public static void CreateWithSwapChain(DriverType driverType, DeviceCreationFlags flags, FeatureLevel[] featureLevels, SwapChainDescription swapChainDescription, out Device device, out SwapChain swapChain)
	{
		CreateWithSwapChain(null, driverType, flags, featureLevels, swapChainDescription, out device, out swapChain);
	}

	public static void CreateWithSwapChain(Adapter adapter, DeviceCreationFlags flags, FeatureLevel[] featureLevels, SwapChainDescription swapChainDescription, out Device device, out SwapChain swapChain)
	{
		CreateWithSwapChain(adapter, DriverType.Unknown, flags, featureLevels, swapChainDescription, out device, out swapChain);
	}

	private static void CreateWithSwapChain(Adapter adapter, DriverType driverType, DeviceCreationFlags flags, FeatureLevel[] featureLevels, SwapChainDescription swapChainDescription, out Device device, out SwapChain swapChain)
	{
		device = ((adapter == null) ? new Device(driverType, flags, featureLevels) : new Device(adapter, flags, featureLevels));
		using Factory1 factory = new Factory1();
		swapChain = new SwapChain(factory, device, swapChainDescription);
	}

	public unsafe CounterMetadata GetCounterMetadata(CounterDescription counterDescription)
	{
		CounterMetadata counterMetadata = new CounterMetadata();
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		CheckCounter(counterDescription, out var typeRef, out var activeCountersRef, IntPtr.Zero, new IntPtr(&num), IntPtr.Zero, new IntPtr(&num2), IntPtr.Zero, new IntPtr(&num3));
		sbyte* ptr = stackalloc sbyte[(int)(uint)num];
		sbyte* ptr2 = stackalloc sbyte[(int)(uint)num2];
		sbyte* ptr3 = stackalloc sbyte[(int)(uint)num3];
		CheckCounter(counterDescription, out typeRef, out activeCountersRef, new IntPtr(ptr), new IntPtr(&num), new IntPtr(ptr2), new IntPtr(&num2), new IntPtr(ptr3), new IntPtr(&num3));
		counterMetadata.Type = typeRef;
		counterMetadata.HardwareCounterCount = activeCountersRef;
		counterMetadata.Name = Marshal.PtrToStringAnsi((IntPtr)ptr, num);
		counterMetadata.Units = Marshal.PtrToStringAnsi((IntPtr)ptr2, num2);
		counterMetadata.Description = Marshal.PtrToStringAnsi((IntPtr)ptr3, num3);
		return counterMetadata;
	}

	public T OpenSharedResource<T>(IntPtr resourceHandle) where T : ComObject
	{
		OpenSharedResource(resourceHandle, Utilities.GetGuidFromType(typeof(T)), out var resourceOut);
		return CppObject.FromPointer<T>(resourceOut);
	}

	public unsafe ComputeShaderFormatSupport CheckComputeShaderFormatSupport(Format format)
	{
		FeatureDataFormatSupport2 featureDataFormatSupport = default(FeatureDataFormatSupport2);
		featureDataFormatSupport.InFormat = format;
		if (CheckFeatureSupport(Feature.ComputeShaders, new IntPtr(&featureDataFormatSupport), Utilities.SizeOf<FeatureDataFormatSupport2>()).Failure)
		{
			return ComputeShaderFormatSupport.None;
		}
		return featureDataFormatSupport.OutFormatSupport2;
	}

	public unsafe FeatureDataD3D11Options CheckD3D11Feature()
	{
		FeatureDataD3D11Options result = default(FeatureDataD3D11Options);
		if (CheckFeatureSupport(Feature.D3D11Options, new IntPtr(&result), Utilities.SizeOf<FeatureDataD3D11Options>()).Failure)
		{
			return default(FeatureDataD3D11Options);
		}
		return result;
	}

	public unsafe FeatureDataShaderMinimumPrecisionSupport CheckShaderMinimumPrecisionSupport()
	{
		FeatureDataShaderMinimumPrecisionSupport result = default(FeatureDataShaderMinimumPrecisionSupport);
		if (CheckFeatureSupport(Feature.ShaderMinimumPrecisionSupport, new IntPtr(&result), Utilities.SizeOf<FeatureDataShaderMinimumPrecisionSupport>()).Failure)
		{
			return default(FeatureDataShaderMinimumPrecisionSupport);
		}
		return result;
	}

	public unsafe bool CheckFullNonPow2TextureSupport()
	{
		FeatureDataD3D9Options featureDataD3D9Options = default(FeatureDataD3D9Options);
		Result result = CheckFeatureSupport(Feature.D3D9Options, new IntPtr(&featureDataD3D9Options), Utilities.SizeOf<FeatureDataD3D9Options>());
		if (FeatureLevel <= FeatureLevel.Level_9_3)
		{
			return result.Failure;
		}
		if (result.Failure)
		{
			return false;
		}
		return featureDataD3D9Options.FullNonPow2TextureSupport;
	}

	public unsafe bool CheckTileBasedDeferredRendererSupport()
	{
		FeatureDataArchitectureInformation featureDataArchitectureInformation = default(FeatureDataArchitectureInformation);
		if (CheckFeatureSupport(Feature.ArchitectureInformation, new IntPtr(&featureDataArchitectureInformation), Utilities.SizeOf<FeatureDataArchitectureInformation>()).Failure)
		{
			return false;
		}
		return featureDataArchitectureInformation.TileBasedDeferredRenderer;
	}

	public unsafe FeatureDataD3D11Options1 CheckD3D112Feature()
	{
		FeatureDataD3D11Options1 result = default(FeatureDataD3D11Options1);
		if (CheckFeatureSupport(Feature.D3D11Options1, new IntPtr(&result), Utilities.SizeOf<FeatureDataD3D11Options1>()).Failure)
		{
			return default(FeatureDataD3D11Options1);
		}
		return result;
	}

	public unsafe FeatureDataD3D11Options2 CheckD3D113Features2()
	{
		FeatureDataD3D11Options2 result = default(FeatureDataD3D11Options2);
		if (CheckFeatureSupport(Feature.D3D11Options2, new IntPtr(&result), Utilities.SizeOf<FeatureDataD3D11Options2>()).Failure)
		{
			return default(FeatureDataD3D11Options2);
		}
		return result;
	}

	public unsafe FeatureDataD3D11Options3 CheckD3D113Features3()
	{
		FeatureDataD3D11Options3 result = default(FeatureDataD3D11Options3);
		if (CheckFeatureSupport(Feature.D3D11Options3, new IntPtr(&result), Utilities.SizeOf<FeatureDataD3D11Options3>()).Failure)
		{
			return default(FeatureDataD3D11Options3);
		}
		return result;
	}

	public unsafe FeatureDataD3D11Options4 CheckD3D113Features4()
	{
		FeatureDataD3D11Options4 result = default(FeatureDataD3D11Options4);
		if (CheckFeatureSupport(Feature.D3D11Options4, new IntPtr(&result), Utilities.SizeOf<FeatureDataD3D11Options4>()).Failure)
		{
			return default(FeatureDataD3D11Options4);
		}
		return result;
	}

	public unsafe bool CheckFeatureSupport(Feature feature)
	{
		switch (feature)
		{
		case Feature.ShaderDoubles:
		{
			FeatureDataDoubles featureDataDoubles = default(FeatureDataDoubles);
			if (CheckFeatureSupport(Feature.ShaderDoubles, new IntPtr(&featureDataDoubles), Utilities.SizeOf<FeatureDataDoubles>()).Failure)
			{
				return false;
			}
			return featureDataDoubles.DoublePrecisionFloatShaderOps;
		}
		case Feature.ComputeShaders:
		case Feature.D3D10XHardwareOptions:
		{
			FeatureDataD3D10XHardwareOptions featureDataD3D10XHardwareOptions = default(FeatureDataD3D10XHardwareOptions);
			if (CheckFeatureSupport(Feature.D3D10XHardwareOptions, new IntPtr(&featureDataD3D10XHardwareOptions), Utilities.SizeOf<FeatureDataD3D10XHardwareOptions>()).Failure)
			{
				return false;
			}
			return featureDataD3D10XHardwareOptions.ComputeShadersPlusRawAndStructuredBuffersViaShader4X;
		}
		default:
			throw new SharpDXException("Unsupported Feature. Use specialized CheckXXX methods");
		}
	}

	public unsafe Result CheckThreadingSupport(out bool supportsConcurrentResources, out bool supportsCommandLists)
	{
		FeatureDataThreading featureDataThreading = default(FeatureDataThreading);
		Result result = CheckFeatureSupport(Feature.Threading, new IntPtr(&featureDataThreading), Utilities.SizeOf<FeatureDataThreading>());
		if (result.Failure)
		{
			supportsConcurrentResources = false;
			supportsCommandLists = false;
		}
		else
		{
			supportsConcurrentResources = featureDataThreading.DriverConcurrentCreates;
			supportsCommandLists = featureDataThreading.DriverCommandLists;
		}
		return result;
	}

	public static bool IsSupportedFeatureLevel(FeatureLevel featureLevel)
	{
		Device device = new Device(IntPtr.Zero);
		DeviceContext immediateContextOut = null;
		try
		{
			FeatureLevel featureLevelRef;
			return D3D11.CreateDevice(null, DriverType.Hardware, IntPtr.Zero, DeviceCreationFlags.None, new FeatureLevel[1] { featureLevel }, 1, 7, device, out featureLevelRef, out immediateContextOut).Success && featureLevelRef == featureLevel;
		}
		finally
		{
			immediateContextOut?.Dispose();
			if (device.NativePointer != IntPtr.Zero)
			{
				device.Dispose();
			}
		}
	}

	public static bool IsSupportedFeatureLevel(Adapter adapter, FeatureLevel featureLevel)
	{
		Device device = new Device(IntPtr.Zero);
		DeviceContext immediateContextOut = null;
		try
		{
			FeatureLevel featureLevelRef;
			return D3D11.CreateDevice(adapter, DriverType.Unknown, IntPtr.Zero, DeviceCreationFlags.None, new FeatureLevel[1] { featureLevel }, 1, 7, device, out featureLevelRef, out immediateContextOut).Success && featureLevelRef == featureLevel;
		}
		finally
		{
			immediateContextOut?.Dispose();
			if (device.NativePointer != IntPtr.Zero)
			{
				device.Dispose();
			}
		}
	}

	public static FeatureLevel GetSupportedFeatureLevel()
	{
		Device device = new Device(IntPtr.Zero);
		D3D11.CreateDevice(null, DriverType.Hardware, IntPtr.Zero, DeviceCreationFlags.None, null, 0, 7, device, out var featureLevelRef, out var immediateContextOut).CheckError();
		immediateContextOut.Dispose();
		device.Dispose();
		return featureLevelRef;
	}

	public static FeatureLevel GetSupportedFeatureLevel(Adapter adapter)
	{
		Device device = new Device(IntPtr.Zero);
		D3D11.CreateDevice(adapter, DriverType.Unknown, IntPtr.Zero, DeviceCreationFlags.None, null, 0, 7, device, out var featureLevelRef, out var immediateContextOut).CheckError();
		immediateContextOut.Dispose();
		device.Dispose();
		return featureLevelRef;
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing && ImmediateContext__ != null)
		{
			ImmediateContext__.Dispose();
			ImmediateContext__ = null;
		}
		base.Dispose(disposing);
	}

	private void CreateDevice(Adapter adapter, DriverType driverType, DeviceCreationFlags flags, FeatureLevel[] featureLevels)
	{
		D3D11.CreateDevice(adapter, driverType, IntPtr.Zero, flags, featureLevels, (featureLevels != null) ? featureLevels.Length : 0, 7, this, out var _, out ImmediateContext__).CheckError();
		if (ImmediateContext__ != null)
		{
			((IUnknown)this).AddReference();
			ImmediateContext__.Device__ = this;
		}
	}

	public static Device CreateFromDirect3D12(ComObject d3D12Device, DeviceCreationFlags flags, FeatureLevel[] featureLevels, Adapter adapter, params ComObject[] commandQueues)
	{
		if (d3D12Device == null)
		{
			throw new ArgumentNullException("d3D12Device");
		}
		D3D11.On12CreateDevice(d3D12Device, flags, featureLevels, (featureLevels != null) ? featureLevels.Length : 0, commandQueues, commandQueues.Length, 0, out var deviceOut, out var immediateContextOut, out var _);
		immediateContextOut.Dispose();
		return deviceOut;
	}

	public Device(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator Device(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new Device(nativePtr);
		}
		return null;
	}

	internal unsafe void CreateBuffer(ref BufferDescription descRef, DataBox? initialDataRef, Buffer bufferOut)
	{
		IntPtr zero = IntPtr.Zero;
		DataBox value = default(DataBox);
		if (initialDataRef.HasValue)
		{
			value = initialDataRef.Value;
		}
		Result result;
		fixed (BufferDescription* ptr = &descRef)
		{
			void* ptr2 = ptr;
			void* nativePointer = _nativePointer;
			DataBox* intPtr = ((!initialDataRef.HasValue) ? null : (&value));
			result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)3 * (nint)sizeof(void*))))(nativePointer, ptr2, intPtr, &zero);
		}
		bufferOut.NativePointer = zero;
		result.CheckError();
	}

	internal unsafe void CreateTexture1D(ref Texture1DDescription descRef, DataBox[] initialDataRef, Texture1D texture1DOut)
	{
		IntPtr zero = IntPtr.Zero;
		Result result;
		fixed (DataBox* ptr = initialDataRef)
		{
			void* ptr2 = ptr;
			fixed (Texture1DDescription* ptr3 = &descRef)
			{
				void* ptr4 = ptr3;
				result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)4 * (nint)sizeof(void*))))(_nativePointer, ptr4, ptr2, &zero);
			}
		}
		texture1DOut.NativePointer = zero;
		result.CheckError();
	}

	internal unsafe void CreateTexture2D(ref Texture2DDescription descRef, DataBox[] initialDataRef, Texture2D texture2DOut)
	{
		IntPtr zero = IntPtr.Zero;
		Result result;
		fixed (DataBox* ptr = initialDataRef)
		{
			void* ptr2 = ptr;
			fixed (Texture2DDescription* ptr3 = &descRef)
			{
				void* ptr4 = ptr3;
				result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)5 * (nint)sizeof(void*))))(_nativePointer, ptr4, ptr2, &zero);
			}
		}
		texture2DOut.NativePointer = zero;
		result.CheckError();
	}

	internal unsafe void CreateTexture3D(ref Texture3DDescription descRef, DataBox[] initialDataRef, Texture3D texture3DOut)
	{
		IntPtr zero = IntPtr.Zero;
		Result result;
		fixed (DataBox* ptr = initialDataRef)
		{
			void* ptr2 = ptr;
			fixed (Texture3DDescription* ptr3 = &descRef)
			{
				void* ptr4 = ptr3;
				result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)6 * (nint)sizeof(void*))))(_nativePointer, ptr4, ptr2, &zero);
			}
		}
		texture3DOut.NativePointer = zero;
		result.CheckError();
	}

	internal unsafe void CreateShaderResourceView(Resource resourceRef, ShaderResourceViewDescription? descRef, ShaderResourceView sRViewOut)
	{
		IntPtr zero = IntPtr.Zero;
		IntPtr zero2 = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<Resource>(resourceRef);
		ShaderResourceViewDescription value = default(ShaderResourceViewDescription);
		if (descRef.HasValue)
		{
			value = descRef.Value;
		}
		void* nativePointer = _nativePointer;
		void* intPtr = (void*)zero;
		ShaderResourceViewDescription* intPtr2 = ((!descRef.HasValue) ? null : (&value));
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)7 * (nint)sizeof(void*))))(nativePointer, intPtr, intPtr2, &zero2);
		sRViewOut.NativePointer = zero2;
		result.CheckError();
	}

	internal unsafe void CreateUnorderedAccessView(Resource resourceRef, UnorderedAccessViewDescription? descRef, UnorderedAccessView uAViewOut)
	{
		IntPtr zero = IntPtr.Zero;
		IntPtr zero2 = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<Resource>(resourceRef);
		UnorderedAccessViewDescription value = default(UnorderedAccessViewDescription);
		if (descRef.HasValue)
		{
			value = descRef.Value;
		}
		void* nativePointer = _nativePointer;
		void* intPtr = (void*)zero;
		UnorderedAccessViewDescription* intPtr2 = ((!descRef.HasValue) ? null : (&value));
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)8 * (nint)sizeof(void*))))(nativePointer, intPtr, intPtr2, &zero2);
		uAViewOut.NativePointer = zero2;
		result.CheckError();
	}

	internal unsafe void CreateRenderTargetView(Resource resourceRef, RenderTargetViewDescription? descRef, RenderTargetView rTViewOut)
	{
		IntPtr zero = IntPtr.Zero;
		IntPtr zero2 = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<Resource>(resourceRef);
		RenderTargetViewDescription value = default(RenderTargetViewDescription);
		if (descRef.HasValue)
		{
			value = descRef.Value;
		}
		void* nativePointer = _nativePointer;
		void* intPtr = (void*)zero;
		RenderTargetViewDescription* intPtr2 = ((!descRef.HasValue) ? null : (&value));
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)9 * (nint)sizeof(void*))))(nativePointer, intPtr, intPtr2, &zero2);
		rTViewOut.NativePointer = zero2;
		result.CheckError();
	}

	internal unsafe void CreateDepthStencilView(Resource resourceRef, DepthStencilViewDescription? descRef, DepthStencilView depthStencilViewOut)
	{
		IntPtr zero = IntPtr.Zero;
		IntPtr zero2 = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<Resource>(resourceRef);
		DepthStencilViewDescription value = default(DepthStencilViewDescription);
		if (descRef.HasValue)
		{
			value = descRef.Value;
		}
		void* nativePointer = _nativePointer;
		void* intPtr = (void*)zero;
		DepthStencilViewDescription* intPtr2 = ((!descRef.HasValue) ? null : (&value));
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)10 * (nint)sizeof(void*))))(nativePointer, intPtr, intPtr2, &zero2);
		depthStencilViewOut.NativePointer = zero2;
		result.CheckError();
	}

	internal unsafe void CreateInputLayout(InputElement[] inputElementDescsRef, int numElements, IntPtr shaderBytecodeWithInputSignatureRef, PointerSize bytecodeLength, InputLayout inputLayoutOut)
	{
		InputElement.__Native[] array = new InputElement.__Native[inputElementDescsRef.Length];
		IntPtr zero = IntPtr.Zero;
		for (int i = 0; i < inputElementDescsRef.Length; i++)
		{
			inputElementDescsRef[i].__MarshalTo(ref array[i]);
		}
		Result result;
		fixed (InputElement.__Native* ptr = array)
		{
			void* ptr2 = ptr;
			result = ((delegate* unmanaged[Stdcall]<void*, void*, int, void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)11 * (nint)sizeof(void*))))(_nativePointer, ptr2, numElements, (void*)shaderBytecodeWithInputSignatureRef, bytecodeLength, &zero);
		}
		inputLayoutOut.NativePointer = zero;
		for (int j = 0; j < inputElementDescsRef.Length; j++)
		{
			inputElementDescsRef[j].__MarshalFree(ref array[j]);
		}
		result.CheckError();
	}

	internal unsafe void CreateVertexShader(IntPtr shaderBytecodeRef, PointerSize bytecodeLength, ClassLinkage classLinkageRef, VertexShader vertexShaderOut)
	{
		IntPtr zero = IntPtr.Zero;
		IntPtr zero2 = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<ClassLinkage>(classLinkageRef);
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)12 * (nint)sizeof(void*))))(_nativePointer, (void*)shaderBytecodeRef, bytecodeLength, (void*)zero, &zero2);
		vertexShaderOut.NativePointer = zero2;
		result.CheckError();
	}

	internal unsafe void CreateGeometryShader(IntPtr shaderBytecodeRef, PointerSize bytecodeLength, ClassLinkage classLinkageRef, GeometryShader geometryShaderOut)
	{
		IntPtr zero = IntPtr.Zero;
		IntPtr zero2 = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<ClassLinkage>(classLinkageRef);
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)13 * (nint)sizeof(void*))))(_nativePointer, (void*)shaderBytecodeRef, bytecodeLength, (void*)zero, &zero2);
		geometryShaderOut.NativePointer = zero2;
		result.CheckError();
	}

	internal unsafe void CreateGeometryShaderWithStreamOutput(IntPtr shaderBytecodeRef, PointerSize bytecodeLength, StreamOutputElement[] sODeclarationRef, int numEntries, int[] bufferStridesRef, int numStrides, int rasterizedStream, ClassLinkage classLinkageRef, GeometryShader geometryShaderOut)
	{
		StreamOutputElement.__Native[] array = ((sODeclarationRef == null) ? null : new StreamOutputElement.__Native[sODeclarationRef.Length]);
		IntPtr zero = IntPtr.Zero;
		IntPtr zero2 = IntPtr.Zero;
		if (sODeclarationRef != null)
		{
			for (int i = 0; i < sODeclarationRef.Length; i++)
			{
				sODeclarationRef?[i].__MarshalTo(ref array[i]);
			}
		}
		zero = CppObject.ToCallbackPtr<ClassLinkage>(classLinkageRef);
		Result result;
		fixed (int* ptr = bufferStridesRef)
		{
			void* ptr2 = ptr;
			fixed (StreamOutputElement.__Native* ptr3 = array)
			{
				void* ptr4 = ptr3;
				result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, void*, int, void*, int, int, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)14 * (nint)sizeof(void*))))(_nativePointer, (void*)shaderBytecodeRef, bytecodeLength, ptr4, numEntries, ptr2, numStrides, rasterizedStream, (void*)zero, &zero2);
			}
		}
		geometryShaderOut.NativePointer = zero2;
		if (sODeclarationRef != null)
		{
			for (int j = 0; j < sODeclarationRef.Length; j++)
			{
				sODeclarationRef?[j].__MarshalFree(ref array[j]);
			}
		}
		result.CheckError();
	}

	internal unsafe void CreatePixelShader(IntPtr shaderBytecodeRef, PointerSize bytecodeLength, ClassLinkage classLinkageRef, PixelShader pixelShaderOut)
	{
		IntPtr zero = IntPtr.Zero;
		IntPtr zero2 = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<ClassLinkage>(classLinkageRef);
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)15 * (nint)sizeof(void*))))(_nativePointer, (void*)shaderBytecodeRef, bytecodeLength, (void*)zero, &zero2);
		pixelShaderOut.NativePointer = zero2;
		result.CheckError();
	}

	internal unsafe void CreateHullShader(IntPtr shaderBytecodeRef, PointerSize bytecodeLength, ClassLinkage classLinkageRef, HullShader hullShaderOut)
	{
		IntPtr zero = IntPtr.Zero;
		IntPtr zero2 = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<ClassLinkage>(classLinkageRef);
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)16 * (nint)sizeof(void*))))(_nativePointer, (void*)shaderBytecodeRef, bytecodeLength, (void*)zero, &zero2);
		hullShaderOut.NativePointer = zero2;
		result.CheckError();
	}

	internal unsafe void CreateDomainShader(IntPtr shaderBytecodeRef, PointerSize bytecodeLength, ClassLinkage classLinkageRef, DomainShader domainShaderOut)
	{
		IntPtr zero = IntPtr.Zero;
		IntPtr zero2 = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<ClassLinkage>(classLinkageRef);
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)17 * (nint)sizeof(void*))))(_nativePointer, (void*)shaderBytecodeRef, bytecodeLength, (void*)zero, &zero2);
		domainShaderOut.NativePointer = zero2;
		result.CheckError();
	}

	internal unsafe void CreateComputeShader(IntPtr shaderBytecodeRef, PointerSize bytecodeLength, ClassLinkage classLinkageRef, ComputeShader computeShaderOut)
	{
		IntPtr zero = IntPtr.Zero;
		IntPtr zero2 = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<ClassLinkage>(classLinkageRef);
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)18 * (nint)sizeof(void*))))(_nativePointer, (void*)shaderBytecodeRef, bytecodeLength, (void*)zero, &zero2);
		computeShaderOut.NativePointer = zero2;
		result.CheckError();
	}

	internal unsafe void CreateClassLinkage(ClassLinkage linkageOut)
	{
		IntPtr zero = IntPtr.Zero;
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)19 * (nint)sizeof(void*))))(_nativePointer, &zero);
		linkageOut.NativePointer = zero;
		result.CheckError();
	}

	internal unsafe void CreateBlendState(ref BlendStateDescription blendStateDescRef, BlendState blendStateOut)
	{
		BlendStateDescription.__Native @ref = default(BlendStateDescription.__Native);
		IntPtr zero = IntPtr.Zero;
		blendStateDescRef.__MarshalTo(ref @ref);
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)20 * (nint)sizeof(void*))))(_nativePointer, &@ref, &zero);
		blendStateOut.NativePointer = zero;
		blendStateDescRef.__MarshalFree(ref @ref);
		result.CheckError();
	}

	internal unsafe void CreateDepthStencilState(ref DepthStencilStateDescription depthStencilDescRef, DepthStencilState depthStencilStateOut)
	{
		IntPtr zero = IntPtr.Zero;
		Result result;
		fixed (DepthStencilStateDescription* ptr = &depthStencilDescRef)
		{
			void* ptr2 = ptr;
			result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)21 * (nint)sizeof(void*))))(_nativePointer, ptr2, &zero);
		}
		depthStencilStateOut.NativePointer = zero;
		result.CheckError();
	}

	internal unsafe void CreateRasterizerState(ref RasterizerStateDescription rasterizerDescRef, RasterizerState rasterizerStateOut)
	{
		IntPtr zero = IntPtr.Zero;
		Result result;
		fixed (RasterizerStateDescription* ptr = &rasterizerDescRef)
		{
			void* ptr2 = ptr;
			result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)22 * (nint)sizeof(void*))))(_nativePointer, ptr2, &zero);
		}
		rasterizerStateOut.NativePointer = zero;
		result.CheckError();
	}

	internal unsafe void CreateSamplerState(ref SamplerStateDescription samplerDescRef, SamplerState samplerStateOut)
	{
		IntPtr zero = IntPtr.Zero;
		Result result;
		fixed (SamplerStateDescription* ptr = &samplerDescRef)
		{
			void* ptr2 = ptr;
			result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)23 * (nint)sizeof(void*))))(_nativePointer, ptr2, &zero);
		}
		samplerStateOut.NativePointer = zero;
		result.CheckError();
	}

	internal unsafe void CreateQuery(QueryDescription queryDescRef, Query queryOut)
	{
		IntPtr zero = IntPtr.Zero;
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)24 * (nint)sizeof(void*))))(_nativePointer, &queryDescRef, &zero);
		queryOut.NativePointer = zero;
		result.CheckError();
	}

	internal unsafe void CreatePredicate(QueryDescription predicateDescRef, Predicate predicateOut)
	{
		IntPtr zero = IntPtr.Zero;
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)25 * (nint)sizeof(void*))))(_nativePointer, &predicateDescRef, &zero);
		predicateOut.NativePointer = zero;
		result.CheckError();
	}

	internal unsafe void CreateCounter(CounterDescription counterDescRef, Counter counterOut)
	{
		IntPtr zero = IntPtr.Zero;
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)26 * (nint)sizeof(void*))))(_nativePointer, &counterDescRef, &zero);
		counterOut.NativePointer = zero;
		result.CheckError();
	}

	internal unsafe void CreateDeferredContext(int contextFlags, DeviceContext deferredContextOut)
	{
		IntPtr zero = IntPtr.Zero;
		Result result = ((delegate* unmanaged[Stdcall]<void*, int, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)27 * (nint)sizeof(void*))))(_nativePointer, contextFlags, &zero);
		deferredContextOut.NativePointer = zero;
		result.CheckError();
	}

	internal unsafe void OpenSharedResource(IntPtr hResource, Guid returnedInterface, out IntPtr resourceOut)
	{
		Result result;
		fixed (IntPtr* ptr = &resourceOut)
		{
			void* ptr2 = ptr;
			result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)28 * (nint)sizeof(void*))))(_nativePointer, (void*)hResource, &returnedInterface, ptr2);
		}
		result.CheckError();
	}

	public unsafe FormatSupport CheckFormatSupport(Format format)
	{
		FormatSupport result = default(FormatSupport);
		_ = (Result)((delegate* unmanaged[Stdcall]<void*, int, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)29 * (nint)sizeof(void*))))(_nativePointer, (int)format, &result);
		return result;
	}

	public unsafe int CheckMultisampleQualityLevels(Format format, int sampleCount)
	{
		int result = default(int);
		_ = (Result)((delegate* unmanaged[Stdcall]<void*, int, int, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)30 * (nint)sizeof(void*))))(_nativePointer, (int)format, sampleCount, &result);
		return result;
	}

	public unsafe CounterCapabilities GetCounterCapabilities()
	{
		CounterCapabilities result = default(CounterCapabilities);
		((delegate* unmanaged[Stdcall]<void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)31 * (nint)sizeof(void*))))(_nativePointer, &result);
		return result;
	}

	internal unsafe void CheckCounter(CounterDescription descRef, out CounterType typeRef, out int activeCountersRef, IntPtr szName, IntPtr nameLengthRef, IntPtr szUnits, IntPtr unitsLengthRef, IntPtr szDescription, IntPtr descriptionLengthRef)
	{
		Result result;
		fixed (int* ptr = &activeCountersRef)
		{
			void* ptr2 = ptr;
			fixed (CounterType* ptr3 = &typeRef)
			{
				void* ptr4 = ptr3;
				result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, void*, void*, void*, void*, void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)32 * (nint)sizeof(void*))))(_nativePointer, &descRef, ptr4, ptr2, (void*)szName, (void*)nameLengthRef, (void*)szUnits, (void*)unitsLengthRef, (void*)szDescription, (void*)descriptionLengthRef);
			}
		}
		result.CheckError();
	}

	internal unsafe Result CheckFeatureSupport(Feature feature, IntPtr featureSupportDataRef, int featureSupportDataSize)
	{
		return ((delegate* unmanaged[Stdcall]<void*, int, void*, int, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)33 * (nint)sizeof(void*))))(_nativePointer, (int)feature, (void*)featureSupportDataRef, featureSupportDataSize);
	}

	public unsafe Result GetPrivateData(Guid guid, ref int dataSizeRef, IntPtr dataRef)
	{
		Result result;
		fixed (int* ptr = &dataSizeRef)
		{
			void* ptr2 = ptr;
			result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)34 * (nint)sizeof(void*))))(_nativePointer, &guid, ptr2, (void*)dataRef);
		}
		return result;
	}

	public unsafe void SetPrivateData(Guid guid, int dataSize, IntPtr dataRef)
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, int, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)35 * (nint)sizeof(void*))))(_nativePointer, &guid, dataSize, (void*)dataRef)).CheckError();
	}

	public unsafe void SetPrivateDataInterface(Guid guid, IUnknown dataRef)
	{
		IntPtr zero = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<IUnknown>(dataRef);
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)36 * (nint)sizeof(void*))))(_nativePointer, &guid, (void*)zero)).CheckError();
	}

	internal unsafe FeatureLevel GetFeatureLevel()
	{
		return ((delegate* unmanaged[Stdcall]<void*, FeatureLevel>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)37 * (nint)sizeof(void*))))(_nativePointer);
	}

	internal unsafe DeviceCreationFlags GetCreationFlags()
	{
		return ((delegate* unmanaged[Stdcall]<void*, DeviceCreationFlags>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)38 * (nint)sizeof(void*))))(_nativePointer);
	}

	internal unsafe Result GetDeviceRemovedReason()
	{
		return ((delegate* unmanaged[Stdcall]<void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)39 * (nint)sizeof(void*))))(_nativePointer);
	}

	internal unsafe void GetImmediateContext(out DeviceContext immediateContextOut)
	{
		IntPtr zero = IntPtr.Zero;
		((delegate* unmanaged[Stdcall]<void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)40 * (nint)sizeof(void*))))(_nativePointer, &zero);
		if (zero != IntPtr.Zero)
		{
			immediateContextOut = new DeviceContext(zero);
		}
		else
		{
			immediateContextOut = null;
		}
	}

	internal unsafe void SetExceptionMode(int raiseFlags)
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, int, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)41 * (nint)sizeof(void*))))(_nativePointer, raiseFlags)).CheckError();
	}

	internal unsafe int GetExceptionMode()
	{
		return ((delegate* unmanaged[Stdcall]<void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)42 * (nint)sizeof(void*))))(_nativePointer);
	}
}
