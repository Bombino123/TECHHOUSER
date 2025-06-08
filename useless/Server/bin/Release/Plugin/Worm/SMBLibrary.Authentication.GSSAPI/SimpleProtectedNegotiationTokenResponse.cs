using System.IO;
using System.Runtime.InteropServices;
using Utilities;

namespace SMBLibrary.Authentication.GSSAPI;

[ComVisible(true)]
public class SimpleProtectedNegotiationTokenResponse : SimpleProtectedNegotiationToken
{
	public const byte NegTokenRespTag = 161;

	public const byte NegStateTag = 160;

	public const byte SupportedMechanismTag = 161;

	public const byte ResponseTokenTag = 162;

	public const byte MechanismListMICTag = 163;

	public NegState? NegState;

	public byte[] SupportedMechanism;

	public byte[] ResponseToken;

	public byte[] MechanismListMIC;

	public SimpleProtectedNegotiationTokenResponse()
	{
	}

	public SimpleProtectedNegotiationTokenResponse(byte[] buffer, int offset)
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
				NegState = ReadNegState(buffer, ref offset);
				break;
			case 161:
				SupportedMechanism = ReadSupportedMechanism(buffer, ref offset);
				break;
			case 162:
				ResponseToken = ReadResponseToken(buffer, ref offset);
				break;
			case 163:
				MechanismListMIC = ReadMechanismListMIC(buffer, ref offset);
				break;
			default:
				throw new InvalidDataException("Invalid negTokenResp structure");
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
		ByteWriter.WriteByte(array, ref offset, 161);
		DerEncodingHelper.WriteLength(array, ref offset, length);
		ByteWriter.WriteByte(array, ref offset, 48);
		DerEncodingHelper.WriteLength(array, ref offset, tokenFieldsLength);
		if (NegState.HasValue)
		{
			WriteNegState(array, ref offset, NegState.Value);
		}
		if (SupportedMechanism != null)
		{
			WriteSupportedMechanism(array, ref offset, SupportedMechanism);
		}
		if (ResponseToken != null)
		{
			WriteResponseToken(array, ref offset, ResponseToken);
		}
		if (MechanismListMIC != null)
		{
			WriteMechanismListMIC(array, ref offset, MechanismListMIC);
		}
		return array;
	}

	private int GetTokenFieldsLength()
	{
		int num = 0;
		if (NegState.HasValue)
		{
			int num2 = 5;
			num += num2;
		}
		if (SupportedMechanism != null)
		{
			int lengthFieldSize = DerEncodingHelper.GetLengthFieldSize(SupportedMechanism.Length);
			int lengthFieldSize2 = DerEncodingHelper.GetLengthFieldSize(1 + lengthFieldSize + SupportedMechanism.Length);
			int num3 = 1 + lengthFieldSize2 + 1 + lengthFieldSize + SupportedMechanism.Length;
			num += num3;
		}
		if (ResponseToken != null)
		{
			int lengthFieldSize3 = DerEncodingHelper.GetLengthFieldSize(ResponseToken.Length);
			int lengthFieldSize4 = DerEncodingHelper.GetLengthFieldSize(1 + lengthFieldSize3 + ResponseToken.Length);
			int num4 = 1 + lengthFieldSize4 + 1 + lengthFieldSize3 + ResponseToken.Length;
			num += num4;
		}
		if (MechanismListMIC != null)
		{
			int lengthFieldSize5 = DerEncodingHelper.GetLengthFieldSize(MechanismListMIC.Length);
			int lengthFieldSize6 = DerEncodingHelper.GetLengthFieldSize(1 + lengthFieldSize5 + MechanismListMIC.Length);
			int num5 = 1 + lengthFieldSize6 + 1 + lengthFieldSize5 + MechanismListMIC.Length;
			num += num5;
		}
		return num;
	}

	private static NegState ReadNegState(byte[] buffer, ref int offset)
	{
		DerEncodingHelper.ReadLength(buffer, ref offset);
		if (ByteReader.ReadByte(buffer, ref offset) != 10)
		{
			throw new InvalidDataException();
		}
		DerEncodingHelper.ReadLength(buffer, ref offset);
		return (NegState)ByteReader.ReadByte(buffer, ref offset);
	}

	private static byte[] ReadSupportedMechanism(byte[] buffer, ref int offset)
	{
		DerEncodingHelper.ReadLength(buffer, ref offset);
		if (ByteReader.ReadByte(buffer, ref offset) != 6)
		{
			throw new InvalidDataException();
		}
		int length = DerEncodingHelper.ReadLength(buffer, ref offset);
		return ByteReader.ReadBytes(buffer, ref offset, length);
	}

	private static byte[] ReadResponseToken(byte[] buffer, ref int offset)
	{
		DerEncodingHelper.ReadLength(buffer, ref offset);
		if (ByteReader.ReadByte(buffer, ref offset) != 4)
		{
			throw new InvalidDataException();
		}
		int length = DerEncodingHelper.ReadLength(buffer, ref offset);
		return ByteReader.ReadBytes(buffer, ref offset, length);
	}

	private static byte[] ReadMechanismListMIC(byte[] buffer, ref int offset)
	{
		DerEncodingHelper.ReadLength(buffer, ref offset);
		if (ByteReader.ReadByte(buffer, ref offset) != 4)
		{
			throw new InvalidDataException();
		}
		int length = DerEncodingHelper.ReadLength(buffer, ref offset);
		return ByteReader.ReadBytes(buffer, ref offset, length);
	}

	private static void WriteNegState(byte[] buffer, ref int offset, NegState negState)
	{
		ByteWriter.WriteByte(buffer, ref offset, 160);
		DerEncodingHelper.WriteLength(buffer, ref offset, 3);
		ByteWriter.WriteByte(buffer, ref offset, 10);
		DerEncodingHelper.WriteLength(buffer, ref offset, 1);
		ByteWriter.WriteByte(buffer, ref offset, (byte)negState);
	}

	private static void WriteSupportedMechanism(byte[] buffer, ref int offset, byte[] supportedMechanism)
	{
		int lengthFieldSize = DerEncodingHelper.GetLengthFieldSize(supportedMechanism.Length);
		ByteWriter.WriteByte(buffer, ref offset, 161);
		DerEncodingHelper.WriteLength(buffer, ref offset, 1 + lengthFieldSize + supportedMechanism.Length);
		ByteWriter.WriteByte(buffer, ref offset, 6);
		DerEncodingHelper.WriteLength(buffer, ref offset, supportedMechanism.Length);
		ByteWriter.WriteBytes(buffer, ref offset, supportedMechanism);
	}

	private static void WriteResponseToken(byte[] buffer, ref int offset, byte[] responseToken)
	{
		int lengthFieldSize = DerEncodingHelper.GetLengthFieldSize(responseToken.Length);
		ByteWriter.WriteByte(buffer, ref offset, 162);
		DerEncodingHelper.WriteLength(buffer, ref offset, 1 + lengthFieldSize + responseToken.Length);
		ByteWriter.WriteByte(buffer, ref offset, 4);
		DerEncodingHelper.WriteLength(buffer, ref offset, responseToken.Length);
		ByteWriter.WriteBytes(buffer, ref offset, responseToken);
	}

	private static void WriteMechanismListMIC(byte[] buffer, ref int offset, byte[] mechanismListMIC)
	{
		int lengthFieldSize = DerEncodingHelper.GetLengthFieldSize(mechanismListMIC.Length);
		ByteWriter.WriteByte(buffer, ref offset, 163);
		DerEncodingHelper.WriteLength(buffer, ref offset, 1 + lengthFieldSize + mechanismListMIC.Length);
		ByteWriter.WriteByte(buffer, ref offset, 4);
		DerEncodingHelper.WriteLength(buffer, ref offset, mechanismListMIC.Length);
		ByteWriter.WriteBytes(buffer, ref offset, mechanismListMIC);
	}
}
