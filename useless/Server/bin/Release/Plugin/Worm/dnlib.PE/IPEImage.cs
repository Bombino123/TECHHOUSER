using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using dnlib.IO;
using dnlib.W32Resources;

namespace dnlib.PE;

[ComVisible(true)]
public interface IPEImage : IRvaFileOffsetConverter, IDisposable
{
	bool IsFileImageLayout { get; }

	bool MayHaveInvalidAddresses { get; }

	string Filename { get; }

	ImageDosHeader ImageDosHeader { get; }

	ImageNTHeaders ImageNTHeaders { get; }

	IList<ImageSectionHeader> ImageSectionHeaders { get; }

	IList<ImageDebugDirectory> ImageDebugDirectories { get; }

	Win32Resources Win32Resources { get; set; }

	DataReaderFactory DataReaderFactory { get; }

	DataReader CreateReader(FileOffset offset);

	DataReader CreateReader(FileOffset offset, uint length);

	DataReader CreateReader(RVA rva);

	DataReader CreateReader(RVA rva, uint length);

	DataReader CreateReader();
}
