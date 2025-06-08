using System;
using dnlib.IO;

namespace dnlib.PE;

public sealed class ImageFileHeader : FileSection
{
	private readonly Machine machine;

	private readonly ushort numberOfSections;

	private readonly uint timeDateStamp;

	private readonly uint pointerToSymbolTable;

	private readonly uint numberOfSymbols;

	private readonly ushort sizeOfOptionalHeader;

	private readonly Characteristics characteristics;

	public Machine Machine => machine;

	public int NumberOfSections => numberOfSections;

	public uint TimeDateStamp => timeDateStamp;

	public uint PointerToSymbolTable => pointerToSymbolTable;

	public uint NumberOfSymbols => numberOfSymbols;

	public uint SizeOfOptionalHeader => sizeOfOptionalHeader;

	public Characteristics Characteristics => characteristics;

	public ImageFileHeader(ref DataReader reader, bool verify)
	{
		SetStartOffset(ref reader);
		machine = (Machine)reader.ReadUInt16();
		numberOfSections = reader.ReadUInt16();
		timeDateStamp = reader.ReadUInt32();
		pointerToSymbolTable = reader.ReadUInt32();
		numberOfSymbols = reader.ReadUInt32();
		sizeOfOptionalHeader = reader.ReadUInt16();
		characteristics = (Characteristics)reader.ReadUInt16();
		SetEndoffset(ref reader);
		if (verify && sizeOfOptionalHeader == 0)
		{
			throw new BadImageFormatException("Invalid SizeOfOptionalHeader");
		}
	}
}
