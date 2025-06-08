using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Utilities;

namespace SMBLibrary.NetBios;

[ComVisible(true)]
public class NetBiosUtils
{
	public static string GetMSNetBiosName(string name, NetBiosSuffix suffix)
	{
		if (name.Length > 15)
		{
			name = name.Substring(0, 15);
		}
		else if (name.Length < 15)
		{
			name = name.PadRight(15);
		}
		string text = name;
		char c = (char)suffix;
		return text + c;
	}

	public static string GetNameFromMSNetBiosName(string netBiosName)
	{
		if (netBiosName.Length != 16)
		{
			throw new ArgumentException("Invalid MS NetBIOS name");
		}
		netBiosName = netBiosName.Substring(0, 15);
		return netBiosName.TrimEnd(new char[1] { ' ' });
	}

	public static NetBiosSuffix GetSuffixFromMSNetBiosName(string netBiosName)
	{
		if (netBiosName.Length != 16)
		{
			throw new ArgumentException("Invalid MS NetBIOS name");
		}
		return (NetBiosSuffix)netBiosName[15];
	}

	public static byte[] EncodeName(string name, NetBiosSuffix suffix, string scopeID)
	{
		return EncodeName(GetMSNetBiosName(name, suffix), scopeID);
	}

	public static byte[] EncodeName(string netBiosName, string scopeID)
	{
		return SecondLevelEncoding(FirstLevelEncoding(netBiosName, scopeID));
	}

	public static string FirstLevelEncoding(string netBiosName, string scopeID)
	{
		if (netBiosName.Length != 16)
		{
			throw new ArgumentException("Invalid MS NetBIOS name");
		}
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < netBiosName.Length; i++)
		{
			byte b = (byte)netBiosName[i];
			byte value = (byte)(65 + (b >> 4));
			byte value2 = (byte)(65 + (b & 0xF));
			stringBuilder.Append((char)value);
			stringBuilder.Append((char)value2);
		}
		if (scopeID.Length > 0)
		{
			stringBuilder.Append(".");
			stringBuilder.Append(scopeID);
		}
		return stringBuilder.ToString();
	}

	public static byte[] SecondLevelEncoding(string domainName)
	{
		string[] array = domainName.Split(new char[1] { '.' });
		int num = 1;
		for (int i = 0; i < array.Length; i++)
		{
			num += 1 + array[i].Length;
			if (array[i].Length > 63)
			{
				throw new ArgumentException("Invalid NetBIOS label length");
			}
		}
		byte[] array2 = new byte[num];
		int num2 = 0;
		string[] array3 = array;
		foreach (string text in array3)
		{
			array2[num2] = (byte)text.Length;
			num2++;
			ByteWriter.WriteAnsiString(array2, num2, text, text.Length);
			num2 += text.Length;
		}
		array2[num2] = 0;
		return array2;
	}

	public static string DecodeName(byte[] buffer, ref int offset)
	{
		return FirstLevelDecoding(SecondLevelDecoding(buffer, ref offset).Split(new char[1] { '.' })[0]);
	}

	public static string SecondLevelDecoding(byte[] buffer, ref int offset)
	{
		StringBuilder stringBuilder = new StringBuilder();
		for (byte b = ByteReader.ReadByte(buffer, ref offset); b > 0; b = ByteReader.ReadByte(buffer, ref offset))
		{
			if (stringBuilder.Length > 0)
			{
				stringBuilder.Append(".");
			}
			if (b > 63)
			{
				throw new ArgumentException("Invalid NetBIOS label length");
			}
			string value = ByteReader.ReadAnsiString(buffer, offset, b);
			stringBuilder.Append(value);
			offset += b;
		}
		return stringBuilder.ToString();
	}

	public static string FirstLevelDecoding(string name)
	{
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < name.Length; i += 2)
		{
			byte num = (byte)name[i];
			byte b = (byte)name[i + 1];
			byte num2 = (byte)(((num - 65) & 0xF) << 4);
			byte b2 = (byte)((uint)(b - 65) & 0xFu);
			byte value = (byte)(num2 | b2);
			stringBuilder.Append((char)value);
		}
		return stringBuilder.ToString();
	}

	public static void WriteNamePointer(byte[] buffer, ref int offset, int nameOffset)
	{
		WriteNamePointer(buffer, offset, nameOffset);
		offset += 2;
	}

	public static void WriteNamePointer(byte[] buffer, int offset, int nameOffset)
	{
		ushort value = (ushort)(0xC000u | ((uint)nameOffset & 0x3FFFu));
		BigEndianWriter.WriteUInt16(buffer, offset, value);
	}

	public static void WriteNamePointer(Stream stream, int nameOffset)
	{
		ushort value = (ushort)(0xC000u | ((uint)nameOffset & 0x3FFFu));
		BigEndianWriter.WriteUInt16(stream, value);
	}
}
