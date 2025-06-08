using dnlib.DotNet.MD;

namespace dnlib.DotNet.Writer;

public sealed class Cor20HeaderOptions
{
	public const ushort DEFAULT_MAJOR_RT_VER = 2;

	public const ushort DEFAULT_MINOR_RT_VER = 5;

	public ushort? MajorRuntimeVersion;

	public ushort? MinorRuntimeVersion;

	public ComImageFlags? Flags;

	public uint? EntryPoint;

	public Cor20HeaderOptions()
	{
	}

	public Cor20HeaderOptions(ComImageFlags flags)
	{
		Flags = flags;
	}

	public Cor20HeaderOptions(ushort major, ushort minor, ComImageFlags flags)
	{
		MajorRuntimeVersion = major;
		MinorRuntimeVersion = minor;
		Flags = flags;
	}
}
