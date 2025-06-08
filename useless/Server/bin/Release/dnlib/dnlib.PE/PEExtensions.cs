using System;
using System.IO;
using dnlib.W32Resources;

namespace dnlib.PE;

public static class PEExtensions
{
	public static ResourceData FindWin32ResourceData(this IPEImage self, ResourceName type, ResourceName name, ResourceName langId)
	{
		return self.Win32Resources?.Find(type, name, langId);
	}

	internal static uint CalculatePECheckSum(this Stream stream, long length, long checkSumOffset)
	{
		if ((length & 1) != 0L)
		{
			ThrowInvalidOperationException("Invalid PE length");
		}
		byte[] buffer = new byte[(int)Math.Min(length, 8192L)];
		uint checkSum = 0u;
		checkSum = CalculatePECheckSum(stream, checkSumOffset, checkSum, buffer);
		stream.Position += 4L;
		checkSum = CalculatePECheckSum(stream, length - checkSumOffset - 4, checkSum, buffer);
		ulong num = (ulong)(checkSum + length);
		return (uint)((int)num + (int)(num >> 32));
	}

	private static uint CalculatePECheckSum(Stream stream, long length, uint checkSum, byte[] buffer)
	{
		int num3;
		for (long num = 0L; num < length; num += num3)
		{
			int num2 = (int)Math.Min(length - num, buffer.Length);
			num3 = stream.Read(buffer, 0, num2);
			if (num3 != num2)
			{
				ThrowInvalidOperationException("Couldn't read all bytes");
			}
			int num4 = 0;
			while (num4 < num3)
			{
				checkSum += (uint)(buffer[num4++] | (buffer[num4++] << 8));
				checkSum = (ushort)(checkSum + (checkSum >> 16));
			}
		}
		return checkSum;
	}

	private static void ThrowInvalidOperationException(string message)
	{
		throw new InvalidOperationException(message);
	}

	public static RVA AlignUp(this RVA rva, uint alignment)
	{
		return (RVA)((uint)(rva + alignment - 1) & ~(alignment - 1));
	}

	public static RVA AlignUp(this RVA rva, int alignment)
	{
		return (RVA)(((long)rva + (long)alignment - 1) & ~(alignment - 1));
	}
}
