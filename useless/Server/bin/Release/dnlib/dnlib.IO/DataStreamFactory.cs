using System;
using dnlib.PE;

namespace dnlib.IO;

public static class DataStreamFactory
{
	private static bool supportsUnalignedAccesses = CalculateSupportsUnalignedAccesses();

	private static bool CalculateSupportsUnalignedAccesses()
	{
		switch (ProcessorArchUtils.GetProcessCpuArchitecture())
		{
		case Machine.I386:
		case Machine.AMD64:
			return true;
		case Machine.ARMNT:
		case Machine.ARM64:
			return false;
		default:
			return true;
		}
	}

	public unsafe static DataStream Create(byte* data)
	{
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		if (supportsUnalignedAccesses)
		{
			return new UnalignedNativeMemoryDataStream(data);
		}
		return new AlignedNativeMemoryDataStream(data);
	}

	public static DataStream Create(byte[] data)
	{
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		if (supportsUnalignedAccesses)
		{
			return new UnalignedByteArrayDataStream(data);
		}
		return new AlignedByteArrayDataStream(data);
	}
}
