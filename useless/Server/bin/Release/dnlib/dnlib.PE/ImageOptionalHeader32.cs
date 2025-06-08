using System;
using dnlib.IO;

namespace dnlib.PE;

public sealed class ImageOptionalHeader32 : FileSection, IImageOptionalHeader, IFileSection
{
	private readonly ushort magic;

	private readonly byte majorLinkerVersion;

	private readonly byte minorLinkerVersion;

	private readonly uint sizeOfCode;

	private readonly uint sizeOfInitializedData;

	private readonly uint sizeOfUninitializedData;

	private readonly RVA addressOfEntryPoint;

	private readonly RVA baseOfCode;

	private readonly RVA baseOfData;

	private readonly uint imageBase;

	private readonly uint sectionAlignment;

	private readonly uint fileAlignment;

	private readonly ushort majorOperatingSystemVersion;

	private readonly ushort minorOperatingSystemVersion;

	private readonly ushort majorImageVersion;

	private readonly ushort minorImageVersion;

	private readonly ushort majorSubsystemVersion;

	private readonly ushort minorSubsystemVersion;

	private readonly uint win32VersionValue;

	private readonly uint sizeOfImage;

	private readonly uint sizeOfHeaders;

	private readonly uint checkSum;

	private readonly Subsystem subsystem;

	private readonly DllCharacteristics dllCharacteristics;

	private readonly uint sizeOfStackReserve;

	private readonly uint sizeOfStackCommit;

	private readonly uint sizeOfHeapReserve;

	private readonly uint sizeOfHeapCommit;

	private readonly uint loaderFlags;

	private readonly uint numberOfRvaAndSizes;

	private readonly ImageDataDirectory[] dataDirectories = new ImageDataDirectory[16];

	public ushort Magic => magic;

	public byte MajorLinkerVersion => majorLinkerVersion;

	public byte MinorLinkerVersion => minorLinkerVersion;

	public uint SizeOfCode => sizeOfCode;

	public uint SizeOfInitializedData => sizeOfInitializedData;

	public uint SizeOfUninitializedData => sizeOfUninitializedData;

	public RVA AddressOfEntryPoint => addressOfEntryPoint;

	public RVA BaseOfCode => baseOfCode;

	public RVA BaseOfData => baseOfData;

	public ulong ImageBase => imageBase;

	public uint SectionAlignment => sectionAlignment;

	public uint FileAlignment => fileAlignment;

	public ushort MajorOperatingSystemVersion => majorOperatingSystemVersion;

	public ushort MinorOperatingSystemVersion => minorOperatingSystemVersion;

	public ushort MajorImageVersion => majorImageVersion;

	public ushort MinorImageVersion => minorImageVersion;

	public ushort MajorSubsystemVersion => majorSubsystemVersion;

	public ushort MinorSubsystemVersion => minorSubsystemVersion;

	public uint Win32VersionValue => win32VersionValue;

	public uint SizeOfImage => sizeOfImage;

	public uint SizeOfHeaders => sizeOfHeaders;

	public uint CheckSum => checkSum;

	public Subsystem Subsystem => subsystem;

	public DllCharacteristics DllCharacteristics => dllCharacteristics;

	public ulong SizeOfStackReserve => sizeOfStackReserve;

	public ulong SizeOfStackCommit => sizeOfStackCommit;

	public ulong SizeOfHeapReserve => sizeOfHeapReserve;

	public ulong SizeOfHeapCommit => sizeOfHeapCommit;

	public uint LoaderFlags => loaderFlags;

	public uint NumberOfRvaAndSizes => numberOfRvaAndSizes;

	public ImageDataDirectory[] DataDirectories => dataDirectories;

	public ImageOptionalHeader32(ref DataReader reader, uint totalSize, bool verify)
	{
		if (totalSize < 96)
		{
			throw new BadImageFormatException("Invalid optional header size");
		}
		if (verify && (ulong)((long)reader.Position + (long)totalSize) > (ulong)reader.Length)
		{
			throw new BadImageFormatException("Invalid optional header size");
		}
		SetStartOffset(ref reader);
		magic = reader.ReadUInt16();
		majorLinkerVersion = reader.ReadByte();
		minorLinkerVersion = reader.ReadByte();
		sizeOfCode = reader.ReadUInt32();
		sizeOfInitializedData = reader.ReadUInt32();
		sizeOfUninitializedData = reader.ReadUInt32();
		addressOfEntryPoint = (RVA)reader.ReadUInt32();
		baseOfCode = (RVA)reader.ReadUInt32();
		baseOfData = (RVA)reader.ReadUInt32();
		imageBase = reader.ReadUInt32();
		sectionAlignment = reader.ReadUInt32();
		fileAlignment = reader.ReadUInt32();
		majorOperatingSystemVersion = reader.ReadUInt16();
		minorOperatingSystemVersion = reader.ReadUInt16();
		majorImageVersion = reader.ReadUInt16();
		minorImageVersion = reader.ReadUInt16();
		majorSubsystemVersion = reader.ReadUInt16();
		minorSubsystemVersion = reader.ReadUInt16();
		win32VersionValue = reader.ReadUInt32();
		sizeOfImage = reader.ReadUInt32();
		sizeOfHeaders = reader.ReadUInt32();
		checkSum = reader.ReadUInt32();
		subsystem = (Subsystem)reader.ReadUInt16();
		dllCharacteristics = (DllCharacteristics)reader.ReadUInt16();
		sizeOfStackReserve = reader.ReadUInt32();
		sizeOfStackCommit = reader.ReadUInt32();
		sizeOfHeapReserve = reader.ReadUInt32();
		sizeOfHeapCommit = reader.ReadUInt32();
		loaderFlags = reader.ReadUInt32();
		numberOfRvaAndSizes = reader.ReadUInt32();
		for (int i = 0; i < dataDirectories.Length; i++)
		{
			if ((uint)(reader.Position - startOffset + 8) <= totalSize)
			{
				dataDirectories[i] = new ImageDataDirectory(ref reader, verify);
			}
			else
			{
				dataDirectories[i] = new ImageDataDirectory();
			}
		}
		reader.Position = (uint)(startOffset + totalSize);
		SetEndoffset(ref reader);
	}
}
