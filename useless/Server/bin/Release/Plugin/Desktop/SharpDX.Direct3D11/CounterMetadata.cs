namespace SharpDX.Direct3D11;

public class CounterMetadata
{
	public CounterType Type { get; internal set; }

	public int HardwareCounterCount { get; internal set; }

	public string Name { get; internal set; }

	public string Units { get; internal set; }

	public string Description { get; internal set; }
}
