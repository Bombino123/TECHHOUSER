using System.Runtime.InteropServices;

namespace dnlib.DotNet.MD;

[ComVisible(true)]
public readonly struct RawAssemblyProcessorRow
{
	public readonly uint Processor;

	public uint this[int index]
	{
		get
		{
			if (index == 0)
			{
				return Processor;
			}
			return 0u;
		}
	}

	public RawAssemblyProcessorRow(uint Processor)
	{
		this.Processor = Processor;
	}
}
