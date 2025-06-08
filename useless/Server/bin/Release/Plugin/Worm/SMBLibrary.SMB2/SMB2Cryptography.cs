using System;
using System.Security.Cryptography;
using Utilities;

namespace SMBLibrary.SMB2;

internal class SMB2Cryptography
{
	private const int AesCcmNonceLength = 11;

	public static byte[] CalculateSignature(byte[] signingKey, SMB2Dialect dialect, byte[] buffer, int offset, int paddedLength)
	{
		if (dialect == SMB2Dialect.SMB202 || dialect == SMB2Dialect.SMB210)
		{
			return new HMACSHA256(signingKey).ComputeHash(buffer, offset, paddedLength);
		}
		return AesCmac.CalculateAesCmac(signingKey, buffer, offset, paddedLength);
	}

	public static byte[] GenerateSigningKey(byte[] sessionKey, SMB2Dialect dialect, byte[] preauthIntegrityHashValue)
	{
		switch (dialect)
		{
		case SMB2Dialect.SMB202:
		case SMB2Dialect.SMB210:
			return sessionKey;
		case SMB2Dialect.SMB311:
			if (preauthIntegrityHashValue == null)
			{
				throw new ArgumentNullException("preauthIntegrityHashValue");
			}
			break;
		}
		byte[] nullTerminatedAnsiString = GetNullTerminatedAnsiString((dialect == SMB2Dialect.SMB311) ? "SMBSigningKey" : "SMB2AESCMAC");
		byte[] context = ((dialect == SMB2Dialect.SMB311) ? preauthIntegrityHashValue : GetNullTerminatedAnsiString("SmbSign"));
		return SP800_1008.DeriveKey(new HMACSHA256(sessionKey), nullTerminatedAnsiString, context, 128);
	}

	public static byte[] GenerateClientEncryptionKey(byte[] sessionKey, SMB2Dialect dialect, byte[] preauthIntegrityHashValue)
	{
		if (dialect == SMB2Dialect.SMB311 && preauthIntegrityHashValue == null)
		{
			throw new ArgumentNullException("preauthIntegrityHashValue");
		}
		byte[] nullTerminatedAnsiString = GetNullTerminatedAnsiString((dialect == SMB2Dialect.SMB311) ? "SMBC2SCipherKey" : "SMB2AESCCM");
		byte[] context = ((dialect == SMB2Dialect.SMB311) ? preauthIntegrityHashValue : GetNullTerminatedAnsiString("ServerIn "));
		return SP800_1008.DeriveKey(new HMACSHA256(sessionKey), nullTerminatedAnsiString, context, 128);
	}

	public static byte[] GenerateClientDecryptionKey(byte[] sessionKey, SMB2Dialect dialect, byte[] preauthIntegrityHashValue)
	{
		if (dialect == SMB2Dialect.SMB311 && preauthIntegrityHashValue == null)
		{
			throw new ArgumentNullException("preauthIntegrityHashValue");
		}
		byte[] nullTerminatedAnsiString = GetNullTerminatedAnsiString((dialect == SMB2Dialect.SMB311) ? "SMBS2CCipherKey" : "SMB2AESCCM");
		byte[] context = ((dialect == SMB2Dialect.SMB311) ? preauthIntegrityHashValue : GetNullTerminatedAnsiString("ServerOut"));
		return SP800_1008.DeriveKey(new HMACSHA256(sessionKey), nullTerminatedAnsiString, context, 128);
	}

	public static byte[] TransformMessage(byte[] key, byte[] message, ulong sessionID)
	{
		byte[] nonce = GenerateAesCcmNonce();
		byte[] signature;
		byte[] bytes = EncryptMessage(key, nonce, message, sessionID, out signature);
		SMB2TransformHeader sMB2TransformHeader = CreateTransformHeader(nonce, message.Length, sessionID);
		sMB2TransformHeader.Signature = signature;
		byte[] array = new byte[52 + message.Length];
		sMB2TransformHeader.WriteBytes(array, 0);
		ByteWriter.WriteBytes(array, 52, bytes);
		return array;
	}

	public static byte[] EncryptMessage(byte[] key, byte[] nonce, byte[] message, ulong sessionID, out byte[] signature)
	{
		byte[] associatedData = CreateTransformHeader(nonce, message.Length, sessionID).GetAssociatedData();
		return Utilities.AesCcm.Encrypt(key, nonce, message, associatedData, 16, out signature);
	}

	public static byte[] DecryptMessage(byte[] key, SMB2TransformHeader transformHeader, byte[] encryptedMessage)
	{
		byte[] associatedData = transformHeader.GetAssociatedData();
		byte[] nonce = ByteReader.ReadBytes(transformHeader.Nonce, 0, 11);
		return Utilities.AesCcm.DecryptAndAuthenticate(key, nonce, encryptedMessage, associatedData, transformHeader.Signature);
	}

	private static SMB2TransformHeader CreateTransformHeader(byte[] nonce, int originalMessageLength, ulong sessionID)
	{
		byte[] array = new byte[16];
		Array.Copy(nonce, array, nonce.Length);
		return new SMB2TransformHeader
		{
			Nonce = array,
			OriginalMessageSize = (uint)originalMessageLength,
			Flags = SMB2TransformHeaderFlags.Encrypted,
			SessionId = sessionID
		};
	}

	private static byte[] GenerateAesCcmNonce()
	{
		byte[] array = new byte[11];
		new Random().NextBytes(array);
		return array;
	}

	private static byte[] GetNullTerminatedAnsiString(string value)
	{
		byte[] array = new byte[value.Length + 1];
		ByteWriter.WriteNullTerminatedAnsiString(array, 0, value);
		return array;
	}
}
