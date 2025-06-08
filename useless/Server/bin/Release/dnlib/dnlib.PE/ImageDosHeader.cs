using System;
using dnlib.IO;

namespace dnlib.PE;

public sealed class ImageDosHeader : FileSection
{
	private readonly uint ntHeadersOffset;

	public uint NTHeadersOffset => ntHeadersOffset;

	public ImageDosHeader(ref DataReader reader, bool verify)
	{
		SetStartOffset(ref reader);
		ushort num = reader.ReadUInt16();
		if (verify && num != 23117)
		{
			throw new BadImageFormatException("Invalid DOS signature");
		}
		reader.Position = (uint)(startOffset + 60);
		ntHeadersOffset = reader.ReadUInt32();
		SetEndoffset(ref reader);
	}
}
