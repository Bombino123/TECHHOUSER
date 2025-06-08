using System.Runtime.InteropServices;
using Utilities;

namespace SMBLibrary.RPC;

[ComVisible(true)]
public class RPCHelper
{
	public static string ReadPortAddress(byte[] buffer, int offset)
	{
		ushort num = LittleEndianConverter.ToUInt16(buffer, offset);
		return ByteReader.ReadAnsiString(buffer, offset + 2, num - 1);
	}

	public static string ReadPortAddress(byte[] buffer, ref int offset)
	{
		string text = ReadPortAddress(buffer, offset);
		offset += text.Length + 3;
		return text;
	}

	public static void WritePortAddress(byte[] buffer, int offset, string value)
	{
		ushort value2 = (ushort)(value.Length + 1);
		LittleEndianWriter.WriteUInt16(buffer, offset, value2);
		ByteWriter.WriteNullTerminatedAnsiString(buffer, offset + 2, value);
	}

	public static void WritePortAddress(byte[] buffer, ref int offset, string value)
	{
		WritePortAddress(buffer, offset, value);
		offset += value.Length + 3;
	}
}
