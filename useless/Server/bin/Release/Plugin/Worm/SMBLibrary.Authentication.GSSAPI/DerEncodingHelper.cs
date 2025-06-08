using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using Utilities;

namespace SMBLibrary.Authentication.GSSAPI;

[ComVisible(true)]
public class DerEncodingHelper
{
	public static int ReadLength(byte[] buffer, ref int offset)
	{
		int num = ByteReader.ReadByte(buffer, ref offset);
		if (num >= 128)
		{
			int length = num & 0x7F;
			byte[] array = ByteReader.ReadBytes(buffer, ref offset, length);
			num = 0;
			byte[] array2 = array;
			foreach (byte b in array2)
			{
				num *= 256;
				num += b;
			}
		}
		return num;
	}

	public static void WriteLength(byte[] buffer, ref int offset, int length)
	{
		if (length >= 128)
		{
			List<byte> list = new List<byte>();
			do
			{
				byte item = (byte)(length % 256);
				list.Add(item);
				length /= 256;
			}
			while (length > 0);
			list.Reverse();
			byte[] array = list.ToArray();
			ByteWriter.WriteByte(buffer, ref offset, (byte)(0x80u | (uint)array.Length));
			ByteWriter.WriteBytes(buffer, ref offset, array);
		}
		else
		{
			ByteWriter.WriteByte(buffer, ref offset, (byte)length);
		}
	}

	public static int GetLengthFieldSize(int length)
	{
		if (length >= 128)
		{
			int num = 1;
			do
			{
				length /= 256;
				num++;
			}
			while (length > 0);
			return num;
		}
		return 1;
	}

	public static byte[] EncodeGeneralString(string value)
	{
		return Encoding.ASCII.GetBytes(value);
	}

	public static string DecodeGeneralString(byte[] bytes)
	{
		return Encoding.ASCII.GetString(bytes);
	}
}
