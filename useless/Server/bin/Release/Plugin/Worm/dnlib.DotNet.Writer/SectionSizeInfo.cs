namespace dnlib.DotNet.Writer;

internal readonly struct SectionSizeInfo
{
	public readonly uint length;

	public readonly uint characteristics;

	public SectionSizeInfo(uint length, uint characteristics)
	{
		this.length = length;
		this.characteristics = characteristics;
	}
}
