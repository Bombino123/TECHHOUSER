using System.Diagnostics;
using dnlib.IO;

namespace dnlib.PE;

[DebuggerDisplay("{virtualAddress} {dataSize}")]
public sealed class ImageDataDirectory : FileSection
{
	private readonly RVA virtualAddress;

	private readonly uint dataSize;

	public RVA VirtualAddress => virtualAddress;

	public uint Size => dataSize;

	public ImageDataDirectory()
	{
	}

	public ImageDataDirectory(ref DataReader reader, bool verify)
	{
		SetStartOffset(ref reader);
		virtualAddress = (RVA)reader.ReadUInt32();
		dataSize = reader.ReadUInt32();
		SetEndoffset(ref reader);
	}
}
