using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using Utilities;

namespace SMBLibrary.Authentication.NTLM;

[ComVisible(true)]
public class AVPairUtils
{
	public static KeyValuePairList<AVPairKey, byte[]> GetAVPairSequence(string domainName, string computerName)
	{
		return new KeyValuePairList<AVPairKey, byte[]>
		{
			{
				AVPairKey.NbDomainName,
				Encoding.Unicode.GetBytes(domainName)
			},
			{
				AVPairKey.NbComputerName,
				Encoding.Unicode.GetBytes(computerName)
			}
		};
	}

	public static byte[] GetAVPairSequenceBytes(KeyValuePairList<AVPairKey, byte[]> pairs)
	{
		byte[] array = new byte[GetAVPairSequenceLength(pairs)];
		int offset = 0;
		WriteAVPairSequence(array, ref offset, pairs);
		return array;
	}

	public static int GetAVPairSequenceLength(KeyValuePairList<AVPairKey, byte[]> pairs)
	{
		int num = 0;
		foreach (KeyValuePair<AVPairKey, byte[]> pair in pairs)
		{
			num += 4 + pair.Value.Length;
		}
		return num + 4;
	}

	public static void WriteAVPairSequence(byte[] buffer, ref int offset, KeyValuePairList<AVPairKey, byte[]> pairs)
	{
		foreach (KeyValuePair<AVPairKey, byte[]> pair in pairs)
		{
			WriteAVPair(buffer, ref offset, pair.Key, pair.Value);
		}
		LittleEndianWriter.WriteUInt16(buffer, ref offset, 0);
		LittleEndianWriter.WriteUInt16(buffer, ref offset, 0);
	}

	private static void WriteAVPair(byte[] buffer, ref int offset, AVPairKey key, byte[] value)
	{
		LittleEndianWriter.WriteUInt16(buffer, ref offset, (ushort)key);
		LittleEndianWriter.WriteUInt16(buffer, ref offset, (ushort)value.Length);
		ByteWriter.WriteBytes(buffer, ref offset, value);
	}

	public static KeyValuePairList<AVPairKey, byte[]> ReadAVPairSequence(byte[] buffer, int offset)
	{
		KeyValuePairList<AVPairKey, byte[]> keyValuePairList = new KeyValuePairList<AVPairKey, byte[]>();
		for (AVPairKey aVPairKey = (AVPairKey)LittleEndianConverter.ToUInt16(buffer, offset); aVPairKey != 0; aVPairKey = (AVPairKey)LittleEndianConverter.ToUInt16(buffer, offset))
		{
			KeyValuePair<AVPairKey, byte[]> item = ReadAVPair(buffer, ref offset);
			keyValuePairList.Add(item);
		}
		return keyValuePairList;
	}

	private static KeyValuePair<AVPairKey, byte[]> ReadAVPair(byte[] buffer, ref int offset)
	{
		ushort key = LittleEndianReader.ReadUInt16(buffer, ref offset);
		ushort length = LittleEndianReader.ReadUInt16(buffer, ref offset);
		byte[] value = ByteReader.ReadBytes(buffer, ref offset, length);
		return new KeyValuePair<AVPairKey, byte[]>((AVPairKey)key, value);
	}
}
