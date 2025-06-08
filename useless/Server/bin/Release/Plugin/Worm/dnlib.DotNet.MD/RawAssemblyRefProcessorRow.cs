using System.Runtime.InteropServices;

namespace dnlib.DotNet.MD;

[ComVisible(true)]
public readonly struct RawAssemblyRefProcessorRow
{
	public readonly uint Processor;

	public readonly uint AssemblyRef;

	public uint this[int index] => index switch
	{
		0 => Processor, 
		1 => AssemblyRef, 
		_ => 0u, 
	};

	public RawAssemblyRefProcessorRow(uint Processor, uint AssemblyRef)
	{
		this.Processor = Processor;
		this.AssemblyRef = AssemblyRef;
	}
}
