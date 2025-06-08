using System;

namespace SharpDX.Direct3D11;

[Flags]
public enum ComputeShaderFormatSupport
{
	AtomicAdd = 1,
	AtomicBitwiseOperations = 2,
	AtomicCompareStoreOrCompareExchange = 4,
	AtomicExchange = 8,
	AtomicSignedMinimumOrMaximum = 0x10,
	AtomicUnsignedMinimumOrMaximum = 0x20,
	TypedLoad = 0x40,
	TypedStore = 0x80,
	OutputMergerLogicOperation = 0x100,
	Tiled = 0x200,
	Shareable = 0x400,
	MultiplaneOverlay = 0x4000,
	None = 0
}
