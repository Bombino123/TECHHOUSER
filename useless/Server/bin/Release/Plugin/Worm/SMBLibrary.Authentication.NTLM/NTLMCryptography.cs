using System;
using System.Globalization;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using Utilities;

namespace SMBLibrary.Authentication.NTLM;

[ComVisible(true)]
public class NTLMCryptography
{
	public static byte[] ComputeLMv1Response(byte[] challenge, string password)
	{
		return DesLongEncrypt(LMOWFv1(password), challenge);
	}

	public static byte[] ComputeNTLMv1Response(byte[] challenge, string password)
	{
		return DesLongEncrypt(NTOWFv1(password), challenge);
	}

	public static byte[] ComputeNTLMv1ExtendedSessionSecurityResponse(byte[] serverChallenge, byte[] clientChallenge, string password)
	{
		byte[] key = NTOWFv1(password);
		byte[] sourceArray = MD5.Create().ComputeHash(ByteUtils.Concatenate(serverChallenge, clientChallenge));
		byte[] array = new byte[8];
		Array.Copy(sourceArray, 0, array, 0, 8);
		return DesLongEncrypt(key, array);
	}

	public static byte[] ComputeLMv2Response(byte[] serverChallenge, byte[] clientChallenge, string password, string user, string domain)
	{
		byte[] key = LMOWFv2(password, user, domain);
		byte[] array = ByteUtils.Concatenate(serverChallenge, clientChallenge);
		return ByteUtils.Concatenate(new HMACMD5(key).ComputeHash(array, 0, array.Length), clientChallenge);
	}

	public static byte[] ComputeNTLMv2Proof(byte[] serverChallenge, byte[] clientChallengeStructurePadded, string password, string user, string domain)
	{
		return new HMACMD5(NTOWFv2(password, user, domain)).ComputeHash(ByteUtils.Concatenate(serverChallenge, clientChallengeStructurePadded), 0, serverChallenge.Length + clientChallengeStructurePadded.Length);
	}

	public static byte[] DesEncrypt(byte[] key, byte[] plainText)
	{
		return DesEncrypt(key, plainText, 0, plainText.Length);
	}

	public static byte[] DesEncrypt(byte[] key, byte[] plainText, int inputOffset, int inputCount)
	{
		ICryptoTransform cryptoTransform = CreateWeakDesEncryptor(CipherMode.ECB, key, new byte[key.Length]);
		byte[] array = new byte[inputCount];
		cryptoTransform.TransformBlock(plainText, inputOffset, inputCount, array, 0);
		return array;
	}

	public static ICryptoTransform CreateWeakDesEncryptor(CipherMode mode, byte[] rgbKey, byte[] rgbIV)
	{
		DES dES = DES.Create();
		dES.Mode = mode;
		if (DES.IsWeakKey(rgbKey) || DES.IsSemiWeakKey(rgbKey))
		{
			DESCryptoServiceProvider dESCryptoServiceProvider = dES as DESCryptoServiceProvider;
			MethodInfo method = dESCryptoServiceProvider.GetType().GetMethod("_NewEncryptor", BindingFlags.Instance | BindingFlags.NonPublic);
			object[] parameters = new object[5] { rgbKey, mode, rgbIV, dESCryptoServiceProvider.FeedbackSize, 0 };
			return method.Invoke(dESCryptoServiceProvider, parameters) as ICryptoTransform;
		}
		return dES.CreateEncryptor(rgbKey, rgbIV);
	}

	public static byte[] DesLongEncrypt(byte[] key, byte[] plainText)
	{
		if (key.Length != 16)
		{
			throw new ArgumentException("Invalid key length");
		}
		if (plainText.Length != 8)
		{
			throw new ArgumentException("Invalid plain-text length");
		}
		byte[] array = new byte[21];
		Array.Copy(key, array, key.Length);
		byte[] array2 = new byte[7];
		byte[] array3 = new byte[7];
		byte[] array4 = new byte[7];
		Array.Copy(array, 0, array2, 0, 7);
		Array.Copy(array, 7, array3, 0, 7);
		Array.Copy(array, 14, array4, 0, 7);
		byte[] sourceArray = DesEncrypt(ExtendDESKey(array2), plainText);
		byte[] sourceArray2 = DesEncrypt(ExtendDESKey(array3), plainText);
		byte[] sourceArray3 = DesEncrypt(ExtendDESKey(array4), plainText);
		byte[] array5 = new byte[24];
		Array.Copy(sourceArray, 0, array5, 0, 8);
		Array.Copy(sourceArray2, 0, array5, 8, 8);
		Array.Copy(sourceArray3, 0, array5, 16, 8);
		return array5;
	}

	public static Encoding GetOEMEncoding()
	{
		return Encoding.GetEncoding(CultureInfo.CurrentCulture.TextInfo.OEMCodePage);
	}

	public static byte[] LMOWFv1(string password)
	{
		byte[] bytes = Encoding.ASCII.GetBytes("KGS!@#$%");
		byte[] bytes2 = GetOEMEncoding().GetBytes(password.ToUpper());
		byte[] array = new byte[14];
		Array.Copy(bytes2, array, Math.Min(bytes2.Length, 14));
		byte[] array2 = new byte[7];
		byte[] array3 = new byte[7];
		Array.Copy(array, 0, array2, 0, 7);
		Array.Copy(array, 7, array3, 0, 7);
		byte[] a = DesEncrypt(ExtendDESKey(array2), bytes);
		byte[] b = DesEncrypt(ExtendDESKey(array3), bytes);
		return ByteUtils.Concatenate(a, b);
	}

	public static byte[] NTOWFv1(string password)
	{
		byte[] bytes = Encoding.Unicode.GetBytes(password);
		return new MD4().GetByteHashFromBytes(bytes);
	}

	public static byte[] LMOWFv2(string password, string user, string domain)
	{
		return NTOWFv2(password, user, domain);
	}

	public static byte[] NTOWFv2(string password, string user, string domain)
	{
		byte[] bytes = Encoding.Unicode.GetBytes(password);
		byte[] byteHashFromBytes = new MD4().GetByteHashFromBytes(bytes);
		string s = user.ToUpper() + domain;
		byte[] bytes2 = Encoding.Unicode.GetBytes(s);
		return new HMACMD5(byteHashFromBytes).ComputeHash(bytes2, 0, bytes2.Length);
	}

	private static byte[] ExtendDESKey(byte[] key)
	{
		byte[] array = new byte[8]
		{
			(byte)((uint)(key[0] >> 1) & 0xFFu),
			(byte)(((uint)((key[0] & 1) << 6) | ((uint)((key[1] & 0xFF) >> 2) & 0xFFu)) & 0xFFu),
			(byte)(((uint)((key[1] & 3) << 5) | ((uint)((key[2] & 0xFF) >> 3) & 0xFFu)) & 0xFFu),
			(byte)(((uint)((key[2] & 7) << 4) | ((uint)((key[3] & 0xFF) >> 4) & 0xFFu)) & 0xFFu),
			(byte)(((uint)((key[3] & 0xF) << 3) | ((uint)((key[4] & 0xFF) >> 5) & 0xFFu)) & 0xFFu),
			(byte)(((uint)((key[4] & 0x1F) << 2) | ((uint)((key[5] & 0xFF) >> 6) & 0xFFu)) & 0xFFu),
			(byte)(((uint)((key[5] & 0x3F) << 1) | ((uint)((key[6] & 0xFF) >> 7) & 0xFFu)) & 0xFFu),
			(byte)(key[6] & 0x7Fu)
		};
		for (int i = 0; i < 8; i++)
		{
			array[i] <<= 1;
		}
		return array;
	}

	public static byte[] KXKey(byte[] sessionBaseKey, NegotiateFlags negotiateFlags, byte[] lmChallengeResponse, byte[] serverChallenge, byte[] lmowf)
	{
		if ((negotiateFlags & NegotiateFlags.ExtendedSessionSecurity) == 0)
		{
			if ((negotiateFlags & NegotiateFlags.LanManagerSessionKey) != 0)
			{
				byte[] key = ByteReader.ReadBytes(lmowf, 0, 7);
				byte[] key2 = ByteUtils.Concatenate(ByteReader.ReadBytes(lmowf, 7, 1), new byte[6] { 189, 189, 189, 189, 189, 189 });
				byte[] a = DesEncrypt(ExtendDESKey(key), ByteReader.ReadBytes(lmChallengeResponse, 0, 8));
				byte[] b = DesEncrypt(ExtendDESKey(key2), ByteReader.ReadBytes(lmChallengeResponse, 0, 8));
				return ByteUtils.Concatenate(a, b);
			}
			if ((negotiateFlags & NegotiateFlags.RequestLMSessionKey) != 0)
			{
				return ByteUtils.Concatenate(ByteReader.ReadBytes(lmowf, 0, 8), new byte[8]);
			}
			return sessionBaseKey;
		}
		byte[] buffer = ByteUtils.Concatenate(serverChallenge, ByteReader.ReadBytes(lmChallengeResponse, 0, 8));
		return new HMACMD5(sessionBaseKey).ComputeHash(buffer);
	}

	public static bool ValidateAuthenticateMessageMIC(byte[] exportedSessionKey, byte[] negotiateMessageBytes, byte[] challengeMessageBytes, byte[] authenticateMessageBytes)
	{
		int micFieldOffset = AuthenticateMessage.GetMicFieldOffset(authenticateMessageBytes);
		byte[] array = ByteReader.ReadBytes(authenticateMessageBytes, micFieldOffset, 16);
		ByteWriter.WriteBytes(authenticateMessageBytes, micFieldOffset, new byte[16]);
		byte[] buffer = ByteUtils.Concatenate(ByteUtils.Concatenate(negotiateMessageBytes, challengeMessageBytes), authenticateMessageBytes);
		return ByteUtils.AreByteArraysEqual(new HMACMD5(exportedSessionKey).ComputeHash(buffer), array);
	}

	public static byte[] ComputeClientSignKey(byte[] exportedSessionKey)
	{
		return ComputeSignKey(exportedSessionKey, isClient: true);
	}

	public static byte[] ComputeServerSignKey(byte[] exportedSessionKey)
	{
		return ComputeSignKey(exportedSessionKey, isClient: false);
	}

	private static byte[] ComputeSignKey(byte[] exportedSessionKey, bool isClient)
	{
		string s = ((!isClient) ? "session key to server-to-client signing key magic constant" : "session key to client-to-server signing key magic constant");
		byte[] b = ByteUtils.Concatenate(Encoding.GetEncoding(28591).GetBytes(s), new byte[1]);
		byte[] buffer = ByteUtils.Concatenate(exportedSessionKey, b);
		return MD5.Create().ComputeHash(buffer);
	}

	public static byte[] ComputeClientSealKey(byte[] exportedSessionKey)
	{
		return ComputeSealKey(exportedSessionKey, isClient: true);
	}

	public static byte[] ComputeServerSealKey(byte[] exportedSessionKey)
	{
		return ComputeSealKey(exportedSessionKey, isClient: false);
	}

	private static byte[] ComputeSealKey(byte[] exportedSessionKey, bool isClient)
	{
		string s = ((!isClient) ? "session key to server-to-client sealing key magic constant" : "session key to client-to-server sealing key magic constant");
		byte[] b = ByteUtils.Concatenate(Encoding.GetEncoding(28591).GetBytes(s), new byte[1]);
		byte[] buffer = ByteUtils.Concatenate(exportedSessionKey, b);
		return MD5.Create().ComputeHash(buffer);
	}

	public static byte[] ComputeMechListMIC(byte[] exportedSessionKey, byte[] message)
	{
		return ComputeMechListMIC(exportedSessionKey, message, 0);
	}

	public static byte[] ComputeMechListMIC(byte[] exportedSessionKey, byte[] message, int seqNum)
	{
		byte[] key = ComputeClientSignKey(exportedSessionKey);
		byte[] bytes = LittleEndianConverter.GetBytes(seqNum);
		byte[] buffer = ByteUtils.Concatenate(bytes, message);
		byte[] data = ByteReader.ReadBytes(new HMACMD5(key).ComputeHash(buffer), 0, 8);
		byte[] b = RC4.Encrypt(ComputeClientSealKey(exportedSessionKey), data);
		return ByteUtils.Concatenate(ByteUtils.Concatenate(new byte[4] { 1, 0, 0, 0 }, b), bytes);
	}
}
