using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using dnlib.IO;

namespace dnlib.PE;

[DebuggerDisplay("RVA:{virtualAddress} VS:{virtualSize} FO:{pointerToRawData} FS:{sizeOfRawData} {displayName}")]
[ComVisible(true)]
public sealed class ImageSectionHeader : FileSection
{
	private readonly string displayName;

	private readonly byte[] name;

	private readonly uint virtualSize;

	private readonly RVA virtualAddress;

	private readonly uint sizeOfRawData;

	private readonly uint pointerToRawData;

	private readonly uint pointerToRelocations;

	private readonly uint pointerToLinenumbers;

	private readonly ushort numberOfRelocations;

	private readonly ushort numberOfLinenumbers;

	private readonly uint characteristics;

	public string DisplayName => displayName;

	public byte[] Name => name;

	public uint VirtualSize => virtualSize;

	public RVA VirtualAddress => virtualAddress;

	public uint SizeOfRawData => sizeOfRawData;

	public uint PointerToRawData => pointerToRawData;

	public uint PointerToRelocations => pointerToRelocations;

	public uint PointerToLinenumbers => pointerToLinenumbers;

	public ushort NumberOfRelocations => numberOfRelocations;

	public ushort NumberOfLinenumbers => numberOfLinenumbers;

	public uint Characteristics => characteristics;

	public ImageSectionHeader(ref DataReader reader, bool verify)
	{
		SetStartOffset(ref reader);
		name = reader.ReadBytes(8);
		virtualSize = reader.ReadUInt32();
		virtualAddress = (RVA)reader.ReadUInt32();
		sizeOfRawData = reader.ReadUInt32();
		pointerToRawData = reader.ReadUInt32();
		pointerToRelocations = reader.ReadUInt32();
		pointerToLinenumbers = reader.ReadUInt32();
		numberOfRelocations = reader.ReadUInt16();
		numberOfLinenumbers = reader.ReadUInt16();
		characteristics = reader.ReadUInt32();
		SetEndoffset(ref reader);
		displayName = ToString(name);
	}

	private static string ToString(byte[] name)
	{
		StringBuilder stringBuilder = new StringBuilder(name.Length);
		foreach (byte b in name)
		{
			if (b == 0)
			{
				break;
			}
			stringBuilder.Append((char)b);
		}
		return stringBuilder.ToString();
	}
}
