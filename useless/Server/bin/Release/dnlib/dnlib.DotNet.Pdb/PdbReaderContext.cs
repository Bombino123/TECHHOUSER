using System;
using System.Collections.Generic;
using dnlib.IO;
using dnlib.PE;

namespace dnlib.DotNet.Pdb;

internal readonly struct PdbReaderContext
{
	private readonly IPEImage peImage;

	private readonly ImageDebugDirectory codeViewDebugDir;

	public bool HasDebugInfo => codeViewDebugDir != null;

	public ImageDebugDirectory CodeViewDebugDirectory => codeViewDebugDir;

	public PdbReaderOptions Options { get; }

	public PdbReaderContext(IPEImage peImage, PdbReaderOptions options)
	{
		this.peImage = peImage;
		Options = options;
		codeViewDebugDir = TryGetDebugDirectoryEntry(peImage, ImageDebugType.CodeView);
	}

	public ImageDebugDirectory TryGetDebugDirectoryEntry(ImageDebugType imageDebugType)
	{
		return TryGetDebugDirectoryEntry(peImage, imageDebugType);
	}

	private static ImageDebugDirectory TryGetDebugDirectoryEntry(IPEImage peImage, ImageDebugType imageDebugType)
	{
		IList<ImageDebugDirectory> imageDebugDirectories = peImage.ImageDebugDirectories;
		int count = imageDebugDirectories.Count;
		for (int i = 0; i < count; i++)
		{
			ImageDebugDirectory imageDebugDirectory = imageDebugDirectories[i];
			if (imageDebugDirectory.Type == imageDebugType)
			{
				return imageDebugDirectory;
			}
		}
		return null;
	}

	public bool TryGetCodeViewData(out Guid guid, out uint age)
	{
		string pdbFilename;
		return TryGetCodeViewData(out guid, out age, out pdbFilename);
	}

	public bool TryGetCodeViewData(out Guid guid, out uint age, out string pdbFilename)
	{
		guid = Guid.Empty;
		age = 0u;
		pdbFilename = null;
		DataReader codeViewDataReader = GetCodeViewDataReader();
		if (codeViewDataReader.Length < 25)
		{
			return false;
		}
		if (codeViewDataReader.ReadUInt32() != 1396986706)
		{
			return false;
		}
		guid = codeViewDataReader.ReadGuid();
		age = codeViewDataReader.ReadUInt32();
		pdbFilename = codeViewDataReader.TryReadZeroTerminatedUtf8String();
		return pdbFilename != null;
	}

	private DataReader GetCodeViewDataReader()
	{
		if (codeViewDebugDir == null)
		{
			return default(DataReader);
		}
		return CreateReader(codeViewDebugDir.AddressOfRawData, codeViewDebugDir.SizeOfData);
	}

	public DataReader CreateReader(RVA rva, uint size)
	{
		if (rva == (RVA)0u || size == 0)
		{
			return default(DataReader);
		}
		DataReader result = peImage.CreateReader(rva, size);
		if (result.Length != size)
		{
			return default(DataReader);
		}
		return result;
	}
}
