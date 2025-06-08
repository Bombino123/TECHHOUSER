using System.Diagnostics;
using dnlib.IO;

namespace dnlib.PE;

[DebuggerDisplay("{type}: TS:{timeDateStamp,h} V:{majorVersion,d}.{minorVersion,d} SZ:{sizeOfData} RVA:{addressOfRawData,h} FO:{pointerToRawData,h}")]
public sealed class ImageDebugDirectory : FileSection
{
	private readonly uint characteristics;

	private readonly uint timeDateStamp;

	private readonly ushort majorVersion;

	private readonly ushort minorVersion;

	private readonly ImageDebugType type;

	private readonly uint sizeOfData;

	private readonly uint addressOfRawData;

	private readonly uint pointerToRawData;

	public uint Characteristics => characteristics;

	public uint TimeDateStamp => timeDateStamp;

	public ushort MajorVersion => majorVersion;

	public ushort MinorVersion => minorVersion;

	public ImageDebugType Type => type;

	public uint SizeOfData => sizeOfData;

	public RVA AddressOfRawData => (RVA)addressOfRawData;

	public FileOffset PointerToRawData => (FileOffset)pointerToRawData;

	public ImageDebugDirectory(ref DataReader reader, bool verify)
	{
		SetStartOffset(ref reader);
		characteristics = reader.ReadUInt32();
		timeDateStamp = reader.ReadUInt32();
		majorVersion = reader.ReadUInt16();
		minorVersion = reader.ReadUInt16();
		type = (ImageDebugType)reader.ReadUInt32();
		sizeOfData = reader.ReadUInt32();
		addressOfRawData = reader.ReadUInt32();
		pointerToRawData = reader.ReadUInt32();
		SetEndoffset(ref reader);
	}
}
