using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using Utilities;

namespace SMBLibrary.Authentication.GSSAPI;

[ComVisible(true)]
public class SimpleProtectedNegotiationTokenInit : SimpleProtectedNegotiationToken
{
	public const byte NegTokenInitTag = 160;

	public const byte MechanismTypeListTag = 160;

	public const byte RequiredFlagsTag = 161;

	public const byte MechanismTokenTag = 162;

	public const byte MechanismListMICTag = 163;

	public List<byte[]> MechanismTypeList;

	public byte[] MechanismToken;

	public byte[] MechanismListMIC;

	public SimpleProtectedNegotiationTokenInit()
	{
	}

	public SimpleProtectedNegotiationTokenInit(byte[] buffer, int offset)
	{
		DerEncodingHelper.ReadLength(buffer, ref offset);
		byte b = ByteReader.ReadByte(buffer, ref offset);
		if (b != 48)
		{
			throw new InvalidDataException();
		}
		int num = DerEncodingHelper.ReadLength(buffer, ref offset);
		int num2 = offset + num;
		while (offset < num2)
		{
			switch (ByteReader.ReadByte(buffer, ref offset))
			{
			case 160:
				MechanismTypeList = ReadMechanismTypeList(buffer, ref offset);
				break;
			case 161:
				throw new NotImplementedException("negTokenInit.ReqFlags is not implemented");
			case 162:
				MechanismToken = ReadMechanismToken(buffer, ref offset);
				break;
			case 163:
				MechanismListMIC = ReadMechanismListMIC(buffer, ref offset);
				break;
			default:
				throw new InvalidDataException("Invalid negTokenInit structure");
			}
		}
	}

	public override byte[] GetBytes()
	{
		int tokenFieldsLength = GetTokenFieldsLength();
		int lengthFieldSize = DerEncodingHelper.GetLengthFieldSize(tokenFieldsLength);
		int length = 1 + lengthFieldSize + tokenFieldsLength;
		int lengthFieldSize2 = DerEncodingHelper.GetLengthFieldSize(length);
		byte[] array = new byte[1 + lengthFieldSize2 + 1 + lengthFieldSize + tokenFieldsLength];
		int offset = 0;
		ByteWriter.WriteByte(array, ref offset, 160);
		DerEncodingHelper.WriteLength(array, ref offset, length);
		ByteWriter.WriteByte(array, ref offset, 48);
		DerEncodingHelper.WriteLength(array, ref offset, tokenFieldsLength);
		if (MechanismTypeList != null)
		{
			WriteMechanismTypeList(array, ref offset, MechanismTypeList);
		}
		if (MechanismToken != null)
		{
			WriteMechanismToken(array, ref offset, MechanismToken);
		}
		if (MechanismListMIC != null)
		{
			WriteMechanismListMIC(array, ref offset, MechanismListMIC);
		}
		return array;
	}

	protected virtual int GetTokenFieldsLength()
	{
		int num = GetEncodedMechanismTypeListLength(MechanismTypeList);
		if (MechanismToken != null)
		{
			int lengthFieldSize = DerEncodingHelper.GetLengthFieldSize(MechanismToken.Length);
			int lengthFieldSize2 = DerEncodingHelper.GetLengthFieldSize(1 + lengthFieldSize + MechanismToken.Length);
			int num2 = 1 + lengthFieldSize2 + 1 + lengthFieldSize + MechanismToken.Length;
			num += num2;
		}
		if (MechanismListMIC != null)
		{
			int lengthFieldSize3 = DerEncodingHelper.GetLengthFieldSize(MechanismListMIC.Length);
			int lengthFieldSize4 = DerEncodingHelper.GetLengthFieldSize(1 + lengthFieldSize3 + MechanismListMIC.Length);
			int num3 = 1 + lengthFieldSize4 + 1 + lengthFieldSize3 + MechanismListMIC.Length;
			num += num3;
		}
		return num;
	}

	protected static List<byte[]> ReadMechanismTypeList(byte[] buffer, ref int offset)
	{
		List<byte[]> list = new List<byte[]>();
		DerEncodingHelper.ReadLength(buffer, ref offset);
		if (ByteReader.ReadByte(buffer, ref offset) != 48)
		{
			throw new InvalidDataException();
		}
		int num = DerEncodingHelper.ReadLength(buffer, ref offset);
		int num2 = offset + num;
		while (offset < num2)
		{
			if (ByteReader.ReadByte(buffer, ref offset) != 6)
			{
				throw new InvalidDataException();
			}
			int length = DerEncodingHelper.ReadLength(buffer, ref offset);
			byte[] item = ByteReader.ReadBytes(buffer, ref offset, length);
			list.Add(item);
		}
		return list;
	}

	protected static byte[] ReadMechanismToken(byte[] buffer, ref int offset)
	{
		DerEncodingHelper.ReadLength(buffer, ref offset);
		if (ByteReader.ReadByte(buffer, ref offset) != 4)
		{
			throw new InvalidDataException();
		}
		int length = DerEncodingHelper.ReadLength(buffer, ref offset);
		return ByteReader.ReadBytes(buffer, ref offset, length);
	}

	protected static byte[] ReadMechanismListMIC(byte[] buffer, ref int offset)
	{
		DerEncodingHelper.ReadLength(buffer, ref offset);
		if (ByteReader.ReadByte(buffer, ref offset) != 4)
		{
			throw new InvalidDataException();
		}
		int length = DerEncodingHelper.ReadLength(buffer, ref offset);
		return ByteReader.ReadBytes(buffer, ref offset, length);
	}

	protected static int GetMechanismTypeListSequenceLength(List<byte[]> mechanismTypeList)
	{
		int num = 0;
		foreach (byte[] mechanismType in mechanismTypeList)
		{
			int lengthFieldSize = DerEncodingHelper.GetLengthFieldSize(mechanismType.Length);
			int num2 = 1 + lengthFieldSize + mechanismType.Length;
			num += num2;
		}
		return num;
	}

	protected static void WriteMechanismTypeList(byte[] buffer, ref int offset, List<byte[]> mechanismTypeList)
	{
		int mechanismTypeListSequenceLength = GetMechanismTypeListSequenceLength(mechanismTypeList);
		int lengthFieldSize = DerEncodingHelper.GetLengthFieldSize(mechanismTypeListSequenceLength);
		int length = 1 + lengthFieldSize + mechanismTypeListSequenceLength;
		ByteWriter.WriteByte(buffer, ref offset, 160);
		DerEncodingHelper.WriteLength(buffer, ref offset, length);
		WriteMechanismTypeListSequence(buffer, ref offset, mechanismTypeList, mechanismTypeListSequenceLength);
	}

	protected static void WriteMechanismTypeListSequence(byte[] buffer, ref int offset, List<byte[]> mechanismTypeList, int sequenceLength)
	{
		ByteWriter.WriteByte(buffer, ref offset, 48);
		DerEncodingHelper.WriteLength(buffer, ref offset, sequenceLength);
		foreach (byte[] mechanismType in mechanismTypeList)
		{
			ByteWriter.WriteByte(buffer, ref offset, 6);
			DerEncodingHelper.WriteLength(buffer, ref offset, mechanismType.Length);
			ByteWriter.WriteBytes(buffer, ref offset, mechanismType);
		}
	}

	protected static void WriteMechanismToken(byte[] buffer, ref int offset, byte[] mechanismToken)
	{
		int length = 1 + DerEncodingHelper.GetLengthFieldSize(mechanismToken.Length) + mechanismToken.Length;
		ByteWriter.WriteByte(buffer, ref offset, 162);
		DerEncodingHelper.WriteLength(buffer, ref offset, length);
		ByteWriter.WriteByte(buffer, ref offset, 4);
		DerEncodingHelper.WriteLength(buffer, ref offset, mechanismToken.Length);
		ByteWriter.WriteBytes(buffer, ref offset, mechanismToken);
	}

	protected static void WriteMechanismListMIC(byte[] buffer, ref int offset, byte[] mechanismListMIC)
	{
		int lengthFieldSize = DerEncodingHelper.GetLengthFieldSize(mechanismListMIC.Length);
		ByteWriter.WriteByte(buffer, ref offset, 163);
		DerEncodingHelper.WriteLength(buffer, ref offset, 1 + lengthFieldSize + mechanismListMIC.Length);
		ByteWriter.WriteByte(buffer, ref offset, 4);
		DerEncodingHelper.WriteLength(buffer, ref offset, mechanismListMIC.Length);
		ByteWriter.WriteBytes(buffer, ref offset, mechanismListMIC);
	}

	public static byte[] GetMechanismTypeListBytes(List<byte[]> mechanismTypeList)
	{
		int mechanismTypeListSequenceLength = GetMechanismTypeListSequenceLength(mechanismTypeList);
		int lengthFieldSize = DerEncodingHelper.GetLengthFieldSize(mechanismTypeListSequenceLength);
		byte[] array = new byte[1 + lengthFieldSize + mechanismTypeListSequenceLength];
		int offset = 0;
		WriteMechanismTypeListSequence(array, ref offset, mechanismTypeList, mechanismTypeListSequenceLength);
		return array;
	}

	private static int GetEncodedMechanismTypeListLength(List<byte[]> mechanismTypeList)
	{
		if (mechanismTypeList == null)
		{
			return 0;
		}
		int mechanismTypeListSequenceLength = GetMechanismTypeListSequenceLength(mechanismTypeList);
		int lengthFieldSize = DerEncodingHelper.GetLengthFieldSize(mechanismTypeListSequenceLength);
		int lengthFieldSize2 = DerEncodingHelper.GetLengthFieldSize(1 + lengthFieldSize + mechanismTypeListSequenceLength);
		return 1 + lengthFieldSize2 + 1 + lengthFieldSize + mechanismTypeListSequenceLength;
	}
}
