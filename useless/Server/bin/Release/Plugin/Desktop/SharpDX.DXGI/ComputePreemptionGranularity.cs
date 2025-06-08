namespace SharpDX.DXGI;

public enum ComputePreemptionGranularity
{
	DmaBufferBoundary,
	DispatchBoundary,
	ThreadGroupBoundary,
	ThreadBoundary,
	InstructionBoundary
}
