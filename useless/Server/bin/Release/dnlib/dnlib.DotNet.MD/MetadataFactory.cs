using System;
using System.Collections.Generic;
using dnlib.IO;
using dnlib.PE;

namespace dnlib.DotNet.MD;

public static class MetadataFactory
{
	private enum MetadataType
	{
		Unknown,
		Compressed,
		ENC
	}

	internal static MetadataBase Load(string fileName, CLRRuntimeReaderKind runtime)
	{
		IPEImage iPEImage = null;
		try
		{
			return Load(iPEImage = new PEImage(fileName), runtime);
		}
		catch
		{
			iPEImage?.Dispose();
			throw;
		}
	}

	internal static MetadataBase Load(byte[] data, CLRRuntimeReaderKind runtime)
	{
		IPEImage iPEImage = null;
		try
		{
			return Load(iPEImage = new PEImage(data), runtime);
		}
		catch
		{
			iPEImage?.Dispose();
			throw;
		}
	}

	internal static MetadataBase Load(IntPtr addr, CLRRuntimeReaderKind runtime)
	{
		IPEImage iPEImage = null;
		try
		{
			return Load(iPEImage = new PEImage(addr, ImageLayout.Memory, verify: true), runtime);
		}
		catch
		{
			iPEImage?.Dispose();
			iPEImage = null;
		}
		try
		{
			return Load(iPEImage = new PEImage(addr, ImageLayout.File, verify: true), runtime);
		}
		catch
		{
			iPEImage?.Dispose();
			throw;
		}
	}

	internal static MetadataBase Load(IntPtr addr, ImageLayout imageLayout, CLRRuntimeReaderKind runtime)
	{
		IPEImage iPEImage = null;
		try
		{
			return Load(iPEImage = new PEImage(addr, imageLayout, verify: true), runtime);
		}
		catch
		{
			iPEImage?.Dispose();
			throw;
		}
	}

	internal static MetadataBase Load(IPEImage peImage, CLRRuntimeReaderKind runtime)
	{
		return Create(peImage, runtime, verify: true);
	}

	public static Metadata CreateMetadata(IPEImage peImage)
	{
		return CreateMetadata(peImage, CLRRuntimeReaderKind.CLR);
	}

	public static Metadata CreateMetadata(IPEImage peImage, CLRRuntimeReaderKind runtime)
	{
		return Create(peImage, runtime, verify: true);
	}

	public static Metadata CreateMetadata(IPEImage peImage, bool verify)
	{
		return CreateMetadata(peImage, CLRRuntimeReaderKind.CLR, verify);
	}

	public static Metadata CreateMetadata(IPEImage peImage, CLRRuntimeReaderKind runtime, bool verify)
	{
		return Create(peImage, runtime, verify);
	}

	private static MetadataBase Create(IPEImage peImage, CLRRuntimeReaderKind runtime, bool verify)
	{
		MetadataBase metadataBase = null;
		try
		{
			ImageDataDirectory imageDataDirectory = peImage.ImageNTHeaders.OptionalHeader.DataDirectories[14];
			if (imageDataDirectory.VirtualAddress == (RVA)0u)
			{
				throw new BadImageFormatException(".NET data directory RVA is 0");
			}
			DataReader reader = peImage.CreateReader(imageDataDirectory.VirtualAddress, 72u);
			ImageCor20Header imageCor20Header = new ImageCor20Header(ref reader, verify && runtime == CLRRuntimeReaderKind.CLR);
			if (imageCor20Header.Metadata.VirtualAddress == (RVA)0u)
			{
				throw new BadImageFormatException(".NET metadata RVA is 0");
			}
			RVA virtualAddress = imageCor20Header.Metadata.VirtualAddress;
			DataReader reader2 = peImage.CreateReader(virtualAddress);
			MetadataHeader metadataHeader = new MetadataHeader(ref reader2, runtime, verify);
			if (verify)
			{
				foreach (StreamHeader streamHeader in metadataHeader.StreamHeaders)
				{
					if ((ulong)((long)streamHeader.Offset + (long)streamHeader.StreamSize) > (ulong)reader2.EndOffset)
					{
						throw new BadImageFormatException("Invalid stream header");
					}
				}
			}
			metadataBase = GetMetadataType(metadataHeader.StreamHeaders, runtime) switch
			{
				MetadataType.Compressed => new CompressedMetadata(peImage, imageCor20Header, metadataHeader, runtime), 
				MetadataType.ENC => new ENCMetadata(peImage, imageCor20Header, metadataHeader, runtime), 
				_ => throw new BadImageFormatException("No #~ or #- stream found"), 
			};
			metadataBase.Initialize(null);
			return metadataBase;
		}
		catch
		{
			metadataBase?.Dispose();
			throw;
		}
	}

	internal static MetadataBase CreateStandalonePortablePDB(DataReaderFactory mdReaderFactory, bool verify)
	{
		MetadataBase metadataBase = null;
		try
		{
			DataReader reader = mdReaderFactory.CreateReader();
			MetadataHeader metadataHeader = new MetadataHeader(ref reader, CLRRuntimeReaderKind.CLR, verify);
			if (verify)
			{
				foreach (StreamHeader streamHeader in metadataHeader.StreamHeaders)
				{
					if (streamHeader.Offset + streamHeader.StreamSize < streamHeader.Offset || streamHeader.Offset + streamHeader.StreamSize > reader.Length)
					{
						throw new BadImageFormatException("Invalid stream header");
					}
				}
			}
			metadataBase = GetMetadataType(metadataHeader.StreamHeaders, CLRRuntimeReaderKind.CLR) switch
			{
				MetadataType.Compressed => new CompressedMetadata(metadataHeader, isStandalonePortablePdb: true, CLRRuntimeReaderKind.CLR), 
				MetadataType.ENC => new ENCMetadata(metadataHeader, isStandalonePortablePdb: true, CLRRuntimeReaderKind.CLR), 
				_ => throw new BadImageFormatException("No #~ or #- stream found"), 
			};
			metadataBase.Initialize(mdReaderFactory);
			return metadataBase;
		}
		catch
		{
			metadataBase?.Dispose();
			throw;
		}
	}

	private static MetadataType GetMetadataType(IList<StreamHeader> streamHeaders, CLRRuntimeReaderKind runtime)
	{
		MetadataType? metadataType = null;
		switch (runtime)
		{
		case CLRRuntimeReaderKind.CLR:
			foreach (StreamHeader streamHeader in streamHeaders)
			{
				if (!metadataType.HasValue)
				{
					if (streamHeader.Name == "#~")
					{
						metadataType = MetadataType.Compressed;
					}
					else if (streamHeader.Name == "#-")
					{
						metadataType = MetadataType.ENC;
					}
				}
				if (streamHeader.Name == "#Schema")
				{
					metadataType = MetadataType.ENC;
				}
			}
			break;
		case CLRRuntimeReaderKind.Mono:
			foreach (StreamHeader streamHeader2 in streamHeaders)
			{
				if (streamHeader2.Name == "#~")
				{
					metadataType = MetadataType.Compressed;
				}
				else if (streamHeader2.Name == "#-")
				{
					metadataType = MetadataType.ENC;
					break;
				}
			}
			break;
		default:
			throw new ArgumentOutOfRangeException("runtime");
		}
		if (!metadataType.HasValue)
		{
			return MetadataType.Unknown;
		}
		return metadataType.Value;
	}
}
