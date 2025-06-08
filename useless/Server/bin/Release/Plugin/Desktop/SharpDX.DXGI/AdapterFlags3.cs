namespace SharpDX.DXGI;

public enum AdapterFlags3
{
	None = 0,
	Remote = 1,
	Software = 2,
	AcgCompatible = 4,
	SupportMonitoredFences = 8,
	SupportNonMonitoredFences = 0x10,
	KeyedMutexConformance = 0x20
}
