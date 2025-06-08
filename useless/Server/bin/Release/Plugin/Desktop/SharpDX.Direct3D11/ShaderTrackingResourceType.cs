namespace SharpDX.Direct3D11;

public enum ShaderTrackingResourceType
{
	None,
	UnorderedAccessViewDevicememory,
	NonUnorderedAccessViewDevicememory,
	AllDevicememory,
	GroupsharedMemory,
	AllSharedMemory,
	GroupsharedNonUnorderedAccessView,
	All
}
