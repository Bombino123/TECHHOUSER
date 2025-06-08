using System.Runtime.InteropServices;
using System.Text;
using Utilities;

namespace SMBLibrary.Authentication.NTLM;

[ComVisible(true)]
public class AuthenticationMessageUtils
{
	public static string ReadAnsiStringBufferPointer(byte[] buffer, int offset)
	{
		byte[] bytes = ReadBufferPointer(buffer, offset);
		return Encoding.Default.GetString(bytes);
	}

	public static string ReadUnicodeStringBufferPointer(byte[] buffer, int offset)
	{
		byte[] bytes = ReadBufferPointer(buffer, offset);
		return Encoding.Unicode.GetString(bytes);
	}

	public static byte[] ReadBufferPointer(byte[] buffer, int offset)
	{
		ushort num = LittleEndianConverter.ToUInt16(buffer, offset);
		LittleEndianConverter.ToUInt16(buffer, offset + 2);
		uint offset2 = LittleEndianConverter.ToUInt32(buffer, offset + 4);
		if (num == 0)
		{
			return new byte[0];
		}
		return ByteReader.ReadBytes(buffer, (int)offset2, num);
	}

	public static void WriteBufferPointer(byte[] buffer, int offset, ushort bufferLength, uint bufferOffset)
	{
		LittleEndianWriter.WriteUInt16(buffer, offset, bufferLength);
		LittleEndianWriter.WriteUInt16(buffer, offset + 2, bufferLength);
		LittleEndianWriter.WriteUInt32(buffer, offset + 4, bufferOffset);
	}

	public static bool IsSignatureValid(byte[] messageBytes)
	{
		if (messageBytes.Length < 8)
		{
			return false;
		}
		return ByteReader.ReadAnsiString(messageBytes, 0, 8) == "NTLMSSP\0";
	}

	public static bool IsNTLMv1ExtendedSessionSecurity(byte[] lmResponse)
	{
		if (lmResponse.Length == 24)
		{
			if (ByteUtils.AreByteArraysEqual(ByteReader.ReadBytes(lmResponse, 0, 8), new byte[8]))
			{
				return false;
			}
			return ByteUtils.AreByteArraysEqual(ByteReader.ReadBytes(lmResponse, 8, 16), new byte[16]);
		}
		return false;
	}

	public static bool IsNTLMv2NTResponse(byte[] ntResponse)
	{
		if (ntResponse.Length >= 48 && ntResponse[16] == 1)
		{
			return ntResponse[17] == 1;
		}
		return false;
	}

	public static MessageTypeName GetMessageType(byte[] messageBytes)
	{
		return (MessageTypeName)LittleEndianConverter.ToUInt32(messageBytes, 8);
	}
}
