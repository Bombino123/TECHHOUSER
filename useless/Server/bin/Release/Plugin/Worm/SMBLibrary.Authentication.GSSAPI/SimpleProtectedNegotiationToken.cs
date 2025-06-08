using System.Runtime.InteropServices;
using Utilities;

namespace SMBLibrary.Authentication.GSSAPI;

[ComVisible(true)]
public abstract class SimpleProtectedNegotiationToken
{
	public const byte ApplicationTag = 96;

	public static readonly byte[] SPNEGOIdentifier = new byte[6] { 43, 6, 1, 5, 5, 2 };

	public abstract byte[] GetBytes();

	public byte[] GetBytes(bool includeHeader)
	{
		byte[] bytes = GetBytes();
		if (includeHeader)
		{
			int lengthFieldSize = DerEncodingHelper.GetLengthFieldSize(SPNEGOIdentifier.Length);
			int length = 1 + lengthFieldSize + SPNEGOIdentifier.Length + bytes.Length;
			int lengthFieldSize2 = DerEncodingHelper.GetLengthFieldSize(length);
			byte[] array = new byte[1 + lengthFieldSize2 + 1 + lengthFieldSize + SPNEGOIdentifier.Length + bytes.Length];
			int offset = 0;
			ByteWriter.WriteByte(array, ref offset, 96);
			DerEncodingHelper.WriteLength(array, ref offset, length);
			ByteWriter.WriteByte(array, ref offset, 6);
			DerEncodingHelper.WriteLength(array, ref offset, SPNEGOIdentifier.Length);
			ByteWriter.WriteBytes(array, ref offset, SPNEGOIdentifier);
			ByteWriter.WriteBytes(array, ref offset, bytes);
			return array;
		}
		return bytes;
	}

	public static SimpleProtectedNegotiationToken ReadToken(byte[] tokenBytes, int offset, bool serverInitiatedNegotiation)
	{
		switch (ByteReader.ReadByte(tokenBytes, ref offset))
		{
		case 96:
		{
			DerEncodingHelper.ReadLength(tokenBytes, ref offset);
			byte b = ByteReader.ReadByte(tokenBytes, ref offset);
			if (b != 6)
			{
				break;
			}
			int length = DerEncodingHelper.ReadLength(tokenBytes, ref offset);
			if (!ByteUtils.AreByteArraysEqual(ByteReader.ReadBytes(tokenBytes, ref offset, length), SPNEGOIdentifier))
			{
				break;
			}
			switch (ByteReader.ReadByte(tokenBytes, ref offset))
			{
			case 160:
				if (serverInitiatedNegotiation)
				{
					return new SimpleProtectedNegotiationTokenInit2(tokenBytes, offset);
				}
				return new SimpleProtectedNegotiationTokenInit(tokenBytes, offset);
			case 161:
				return new SimpleProtectedNegotiationTokenResponse(tokenBytes, offset);
			}
			break;
		}
		case 161:
			return new SimpleProtectedNegotiationTokenResponse(tokenBytes, offset);
		}
		return null;
	}
}
