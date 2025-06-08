using System;
using System.IO;

namespace dnlib.IO;

internal static class DataReaderFactoryFactory
{
	private static readonly bool isUnix;

	static DataReaderFactoryFactory()
	{
		int platform = (int)Environment.OSVersion.Platform;
		if (platform == 4 || platform == 6 || platform == 128)
		{
			isUnix = true;
		}
	}

	public static DataReaderFactory Create(string fileName, bool mapAsImage)
	{
		DataReaderFactory dataReaderFactory = CreateDataReaderFactory(fileName, mapAsImage);
		if (dataReaderFactory != null)
		{
			return dataReaderFactory;
		}
		return ByteArrayDataReaderFactory.Create(File.ReadAllBytes(fileName), fileName);
	}

	private static DataReaderFactory CreateDataReaderFactory(string fileName, bool mapAsImage)
	{
		if (!isUnix)
		{
			return MemoryMappedDataReaderFactory.CreateWindows(fileName, mapAsImage);
		}
		return MemoryMappedDataReaderFactory.CreateUnix(fileName, mapAsImage);
	}
}
