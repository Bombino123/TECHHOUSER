using System;
using dnlib.DotNet;
using dnlib.IO;

namespace dnlib.PE;

internal sealed class PEInfo
{
	private readonly ImageDosHeader imageDosHeader;

	private readonly ImageNTHeaders imageNTHeaders;

	private readonly ImageSectionHeader[] imageSectionHeaders;

	public ImageDosHeader ImageDosHeader => imageDosHeader;

	public ImageNTHeaders ImageNTHeaders => imageNTHeaders;

	public ImageSectionHeader[] ImageSectionHeaders => imageSectionHeaders;

	public PEInfo(ref DataReader reader, bool verify)
	{
		reader.Position = 0u;
		imageDosHeader = new ImageDosHeader(ref reader, verify);
		if (verify && imageDosHeader.NTHeadersOffset == 0)
		{
			throw new BadImageFormatException("Invalid NT headers offset");
		}
		reader.Position = imageDosHeader.NTHeadersOffset;
		imageNTHeaders = new ImageNTHeaders(ref reader, verify);
		reader.Position = (uint)(imageNTHeaders.OptionalHeader.StartOffset + imageNTHeaders.FileHeader.SizeOfOptionalHeader);
		int num = imageNTHeaders.FileHeader.NumberOfSections;
		if (num > 0)
		{
			DataReader dataReader = reader;
			dataReader.Position += 20u;
			uint num2 = dataReader.ReadUInt32();
			num = Math.Min(num, (int)((num2 - reader.Position) / 40));
		}
		imageSectionHeaders = new ImageSectionHeader[num];
		for (int i = 0; i < imageSectionHeaders.Length; i++)
		{
			imageSectionHeaders[i] = new ImageSectionHeader(ref reader, verify);
		}
	}

	public ImageSectionHeader ToImageSectionHeader(FileOffset offset)
	{
		ImageSectionHeader[] array = imageSectionHeaders;
		foreach (ImageSectionHeader imageSectionHeader in array)
		{
			if ((uint)offset >= imageSectionHeader.PointerToRawData && (uint)offset < imageSectionHeader.PointerToRawData + imageSectionHeader.SizeOfRawData)
			{
				return imageSectionHeader;
			}
		}
		return null;
	}

	public ImageSectionHeader ToImageSectionHeader(RVA rva)
	{
		uint sectionAlignment = imageNTHeaders.OptionalHeader.SectionAlignment;
		ImageSectionHeader[] array = imageSectionHeaders;
		foreach (ImageSectionHeader imageSectionHeader in array)
		{
			if (rva >= imageSectionHeader.VirtualAddress && rva < imageSectionHeader.VirtualAddress + dnlib.DotNet.Utils.AlignUp(imageSectionHeader.VirtualSize, sectionAlignment))
			{
				return imageSectionHeader;
			}
		}
		return null;
	}

	public RVA ToRVA(FileOffset offset)
	{
		if (imageSectionHeaders.Length == 0)
		{
			return (RVA)offset;
		}
		ImageSectionHeader imageSectionHeader = imageSectionHeaders[imageSectionHeaders.Length - 1];
		if ((uint)offset > imageSectionHeader.PointerToRawData + imageSectionHeader.SizeOfRawData)
		{
			return (RVA)0u;
		}
		ImageSectionHeader imageSectionHeader2 = ToImageSectionHeader(offset);
		if (imageSectionHeader2 != null)
		{
			return (RVA)((uint)(offset - imageSectionHeader2.PointerToRawData) + (uint)imageSectionHeader2.VirtualAddress);
		}
		return (RVA)offset;
	}

	public FileOffset ToFileOffset(RVA rva)
	{
		if ((uint)rva >= imageNTHeaders.OptionalHeader.SizeOfImage)
		{
			return (FileOffset)0u;
		}
		ImageSectionHeader imageSectionHeader = ToImageSectionHeader(rva);
		if (imageSectionHeader != null)
		{
			uint num = rva - imageSectionHeader.VirtualAddress;
			if (num < imageSectionHeader.SizeOfRawData)
			{
				return (FileOffset)(num + imageSectionHeader.PointerToRawData);
			}
			return (FileOffset)0u;
		}
		return (FileOffset)rva;
	}

	private static ulong AlignUp(ulong val, uint alignment)
	{
		return (val + alignment - 1) & ~(ulong)(alignment - 1);
	}

	public uint GetImageSize()
	{
		IImageOptionalHeader optionalHeader = ImageNTHeaders.OptionalHeader;
		uint sectionAlignment = optionalHeader.SectionAlignment;
		if (imageSectionHeaders.Length == 0)
		{
			return (uint)AlignUp(optionalHeader.SizeOfHeaders, sectionAlignment);
		}
		ImageSectionHeader imageSectionHeader = imageSectionHeaders[imageSectionHeaders.Length - 1];
		return (uint)Math.Min(AlignUp((ulong)imageSectionHeader.VirtualAddress + (ulong)imageSectionHeader.VirtualSize, sectionAlignment), 4294967295uL);
	}
}
