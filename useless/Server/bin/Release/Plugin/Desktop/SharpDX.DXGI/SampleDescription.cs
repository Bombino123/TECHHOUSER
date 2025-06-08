using System.Runtime.InteropServices;

namespace SharpDX.DXGI;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
public struct SampleDescription
{
	public int Count;

	public int Quality;

	public SampleDescription(int count, int quality)
	{
		Count = count;
		Quality = quality;
	}

	public override string ToString()
	{
		return $"{{{Count}, {Quality}}}";
	}
}
