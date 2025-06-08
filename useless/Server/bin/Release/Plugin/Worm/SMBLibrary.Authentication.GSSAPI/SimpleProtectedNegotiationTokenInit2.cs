using System;
using System.IO;
using System.Runtime.InteropServices;
using Utilities;

namespace SMBLibrary.Authentication.GSSAPI;

[ComVisible(true)]
public class SimpleProtectedNegotiationTokenInit2 : SimpleProtectedNegotiationTokenInit
{
	public const byte NegHintsTag = 163;

	public new const byte MechanismListMICTag = 164;

	public const byte HintNameTag = 160;

	public const byte HintAddressTag = 161;

	public string HintName;

	public byte[] HintAddress;

	public SimpleProtectedNegotiationTokenInit2()
	{
		HintName = "not_defined_in_RFC4178@please_ignore";
	}

	public SimpleProtectedNegotiationTokenInit2(byte[] buffer, int offset)
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
				MechanismTypeList = SimpleProtectedNegotiationTokenInit.ReadMechanismTypeList(buffer, ref offset);
				break;
			case 161:
				throw new NotImplementedException("negTokenInit.ReqFlags is not implemented");
			case 162:
				MechanismToken = SimpleProtectedNegotiationTokenInit.ReadMechanismToken(buffer, ref offset);
				break;
			case 163:
				HintName = ReadHints(buffer, ref offset, out HintAddress);
				break;
			case 164:
				MechanismListMIC = SimpleProtectedNegotiationTokenInit.ReadMechanismListMIC(buffer, ref offset);
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
			SimpleProtectedNegotiationTokenInit.WriteMechanismTypeList(array, ref offset, MechanismTypeList);
		}
		if (MechanismToken != null)
		{
			SimpleProtectedNegotiationTokenInit.WriteMechanismToken(array, ref offset, MechanismToken);
		}
		if (HintName != null || HintAddress != null)
		{
			WriteHints(array, ref offset, HintName, HintAddress);
		}
		if (MechanismListMIC != null)
		{
			WriteMechanismListMIC(array, ref offset, MechanismListMIC);
		}
		return array;
	}

	protected override int GetTokenFieldsLength()
	{
		int num = base.GetTokenFieldsLength();
		if (HintName != null || HintAddress != null)
		{
			int hintsSequenceLength = GetHintsSequenceLength(HintName, HintAddress);
			int lengthFieldSize = DerEncodingHelper.GetLengthFieldSize(hintsSequenceLength);
			int lengthFieldSize2 = DerEncodingHelper.GetLengthFieldSize(1 + lengthFieldSize + hintsSequenceLength);
			int num2 = 1 + lengthFieldSize2 + 1 + lengthFieldSize + hintsSequenceLength;
			num += num2;
		}
		return num;
	}

	protected static string ReadHints(byte[] buffer, ref int offset, out byte[] hintAddress)
	{
		string result = null;
		hintAddress = null;
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
				result = ReadHintName(buffer, ref offset);
				break;
			case 161:
				hintAddress = ReadHintAddress(buffer, ref offset);
				break;
			default:
				throw new InvalidDataException();
			}
		}
		return result;
	}

	protected static string ReadHintName(byte[] buffer, ref int offset)
	{
		DerEncodingHelper.ReadLength(buffer, ref offset);
		if (ByteReader.ReadByte(buffer, ref offset) != 27)
		{
			throw new InvalidDataException();
		}
		int length = DerEncodingHelper.ReadLength(buffer, ref offset);
		return DerEncodingHelper.DecodeGeneralString(ByteReader.ReadBytes(buffer, ref offset, length));
	}

	protected static byte[] ReadHintAddress(byte[] buffer, ref int offset)
	{
		DerEncodingHelper.ReadLength(buffer, ref offset);
		if (ByteReader.ReadByte(buffer, ref offset) != 4)
		{
			throw new InvalidDataException();
		}
		int length = DerEncodingHelper.ReadLength(buffer, ref offset);
		return ByteReader.ReadBytes(buffer, ref offset, length);
	}

	protected static int GetHintsSequenceLength(string hintName, byte[] hintAddress)
	{
		int num = 0;
		if (hintName != null)
		{
			byte[] array = DerEncodingHelper.EncodeGeneralString(hintName);
			int lengthFieldSize = DerEncodingHelper.GetLengthFieldSize(array.Length);
			int lengthFieldSize2 = DerEncodingHelper.GetLengthFieldSize(1 + lengthFieldSize + array.Length);
			int num2 = 1 + lengthFieldSize2 + 1 + lengthFieldSize + array.Length;
			num += num2;
		}
		if (hintAddress != null)
		{
			int lengthFieldSize3 = DerEncodingHelper.GetLengthFieldSize(hintAddress.Length);
			int lengthFieldSize4 = DerEncodingHelper.GetLengthFieldSize(1 + lengthFieldSize3 + hintAddress.Length);
			int num3 = 1 + lengthFieldSize4 + 1 + lengthFieldSize3 + hintAddress.Length;
			num += num3;
		}
		return num;
	}

	private static void WriteHints(byte[] buffer, ref int offset, string hintName, byte[] hintAddress)
	{
		int hintsSequenceLength = GetHintsSequenceLength(hintName, hintAddress);
		int lengthFieldSize = DerEncodingHelper.GetLengthFieldSize(hintsSequenceLength);
		int length = 1 + lengthFieldSize + hintsSequenceLength;
		ByteWriter.WriteByte(buffer, ref offset, 163);
		DerEncodingHelper.WriteLength(buffer, ref offset, length);
		ByteWriter.WriteByte(buffer, ref offset, 48);
		DerEncodingHelper.WriteLength(buffer, ref offset, hintsSequenceLength);
		if (hintName != null)
		{
			WriteHintName(buffer, ref offset, hintName);
		}
		if (hintAddress != null)
		{
			WriteHintAddress(buffer, ref offset, hintAddress);
		}
	}

	private static void WriteHintName(byte[] buffer, ref int offset, string hintName)
	{
		byte[] array = DerEncodingHelper.EncodeGeneralString(hintName);
		int length = 1 + DerEncodingHelper.GetLengthFieldSize(array.Length) + array.Length;
		ByteWriter.WriteByte(buffer, ref offset, 160);
		DerEncodingHelper.WriteLength(buffer, ref offset, length);
		ByteWriter.WriteByte(buffer, ref offset, 27);
		DerEncodingHelper.WriteLength(buffer, ref offset, array.Length);
		ByteWriter.WriteBytes(buffer, ref offset, array);
	}

	private static void WriteHintAddress(byte[] buffer, ref int offset, byte[] hintAddress)
	{
		int length = 1 + DerEncodingHelper.GetLengthFieldSize(hintAddress.Length) + hintAddress.Length;
		ByteWriter.WriteByte(buffer, ref offset, 161);
		DerEncodingHelper.WriteLength(buffer, ref offset, length);
		ByteWriter.WriteByte(buffer, ref offset, 4);
		DerEncodingHelper.WriteLength(buffer, ref offset, hintAddress.Length);
		ByteWriter.WriteBytes(buffer, ref offset, hintAddress);
	}

	protected new static void WriteMechanismListMIC(byte[] buffer, ref int offset, byte[] mechanismListMIC)
	{
		int lengthFieldSize = DerEncodingHelper.GetLengthFieldSize(mechanismListMIC.Length);
		ByteWriter.WriteByte(buffer, ref offset, 164);
		DerEncodingHelper.WriteLength(buffer, ref offset, 1 + lengthFieldSize + mechanismListMIC.Length);
		ByteWriter.WriteByte(buffer, ref offset, 4);
		DerEncodingHelper.WriteLength(buffer, ref offset, mechanismListMIC.Length);
		ByteWriter.WriteBytes(buffer, ref offset, mechanismListMIC);
	}
}
