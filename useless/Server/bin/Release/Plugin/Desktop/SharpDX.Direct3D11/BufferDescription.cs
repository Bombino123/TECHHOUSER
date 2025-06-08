using System.Runtime.InteropServices;

namespace SharpDX.Direct3D11;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
public struct BufferDescription
{
	public int SizeInBytes;

	public ResourceUsage Usage;

	public BindFlags BindFlags;

	public CpuAccessFlags CpuAccessFlags;

	public ResourceOptionFlags OptionFlags;

	public int StructureByteStride;

	public BufferDescription(int sizeInBytes, ResourceUsage usage, BindFlags bindFlags, CpuAccessFlags cpuAccessFlags, ResourceOptionFlags optionFlags, int structureByteStride)
	{
		SizeInBytes = sizeInBytes;
		Usage = usage;
		BindFlags = bindFlags;
		CpuAccessFlags = cpuAccessFlags;
		OptionFlags = optionFlags;
		StructureByteStride = structureByteStride;
	}

	public BufferDescription(int sizeInBytes, BindFlags bindFlags, ResourceUsage usage)
	{
		this = default(BufferDescription);
		SizeInBytes = sizeInBytes;
		BindFlags = bindFlags;
		Usage = usage;
	}
}
