using System.Runtime.InteropServices;
using Utilities;

namespace SMBLibrary.Authentication.NTLM;

[ComVisible(true)]
public class ChallengeMessage
{
	public string Signature;

	public MessageTypeName MessageType;

	public string TargetName;

	public NegotiateFlags NegotiateFlags;

	public byte[] ServerChallenge;

	public KeyValuePairList<AVPairKey, byte[]> TargetInfo = new KeyValuePairList<AVPairKey, byte[]>();

	public NTLMVersion Version;

	public ChallengeMessage()
	{
		Signature = "NTLMSSP\0";
		MessageType = MessageTypeName.Challenge;
	}

	public ChallengeMessage(byte[] buffer)
	{
		Signature = ByteReader.ReadAnsiString(buffer, 0, 8);
		MessageType = (MessageTypeName)LittleEndianConverter.ToUInt32(buffer, 8);
		TargetName = AuthenticationMessageUtils.ReadUnicodeStringBufferPointer(buffer, 12);
		NegotiateFlags = (NegotiateFlags)LittleEndianConverter.ToUInt32(buffer, 20);
		ServerChallenge = ByteReader.ReadBytes(buffer, 24, 8);
		byte[] array = AuthenticationMessageUtils.ReadBufferPointer(buffer, 40);
		if (array.Length != 0)
		{
			TargetInfo = AVPairUtils.ReadAVPairSequence(array, 0);
		}
		if ((NegotiateFlags & NegotiateFlags.Version) != 0)
		{
			Version = new NTLMVersion(buffer, 48);
		}
	}

	public byte[] GetBytes()
	{
		if ((NegotiateFlags & NegotiateFlags.TargetNameSupplied) == 0)
		{
			TargetName = string.Empty;
		}
		byte[] array = AVPairUtils.GetAVPairSequenceBytes(TargetInfo);
		if ((NegotiateFlags & NegotiateFlags.TargetInfo) == 0)
		{
			array = new byte[0];
		}
		int num = 48;
		if ((NegotiateFlags & NegotiateFlags.Version) != 0)
		{
			num += 8;
		}
		int num2 = TargetName.Length * 2 + array.Length;
		byte[] array2 = new byte[num + num2];
		ByteWriter.WriteAnsiString(array2, 0, "NTLMSSP\0", 8);
		LittleEndianWriter.WriteUInt32(array2, 8, (uint)MessageType);
		LittleEndianWriter.WriteUInt32(array2, 20, (uint)NegotiateFlags);
		ByteWriter.WriteBytes(array2, 24, ServerChallenge);
		if ((NegotiateFlags & NegotiateFlags.Version) != 0)
		{
			Version.WriteBytes(array2, 48);
		}
		int offset = num;
		AuthenticationMessageUtils.WriteBufferPointer(array2, 12, (ushort)(TargetName.Length * 2), (uint)offset);
		ByteWriter.WriteUTF16String(array2, ref offset, TargetName);
		AuthenticationMessageUtils.WriteBufferPointer(array2, 40, (ushort)array.Length, (uint)offset);
		ByteWriter.WriteBytes(array2, ref offset, array);
		return array2;
	}
}
