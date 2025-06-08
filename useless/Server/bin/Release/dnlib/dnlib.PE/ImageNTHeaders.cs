using System;
using dnlib.IO;

namespace dnlib.PE;

public sealed class ImageNTHeaders : FileSection
{
	private readonly uint signature;

	private readonly ImageFileHeader imageFileHeader;

	private readonly IImageOptionalHeader imageOptionalHeader;

	public uint Signature => signature;

	public ImageFileHeader FileHeader => imageFileHeader;

	public IImageOptionalHeader OptionalHeader => imageOptionalHeader;

	public ImageNTHeaders(ref DataReader reader, bool verify)
	{
		SetStartOffset(ref reader);
		signature = reader.ReadUInt32();
		if (verify && (ushort)signature != 17744)
		{
			throw new BadImageFormatException("Invalid NT headers signature");
		}
		imageFileHeader = new ImageFileHeader(ref reader, verify);
		imageOptionalHeader = CreateImageOptionalHeader(ref reader, verify);
		SetEndoffset(ref reader);
	}

	private IImageOptionalHeader CreateImageOptionalHeader(ref DataReader reader, bool verify)
	{
		ushort num = reader.ReadUInt16();
		reader.Position -= 2u;
		return num switch
		{
			267 => new ImageOptionalHeader32(ref reader, imageFileHeader.SizeOfOptionalHeader, verify), 
			523 => new ImageOptionalHeader64(ref reader, imageFileHeader.SizeOfOptionalHeader, verify), 
			_ => throw new BadImageFormatException("Invalid optional header magic"), 
		};
	}
}
