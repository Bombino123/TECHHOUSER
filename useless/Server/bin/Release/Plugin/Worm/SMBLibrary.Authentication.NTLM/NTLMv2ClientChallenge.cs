using System;
using System.Runtime.InteropServices;
using System.Text;
using Utilities;

namespace SMBLibrary.Authentication.NTLM;

[ComVisible(true)]
public class NTLMv2ClientChallenge
{
	public const int MinimumLength = 32;

	public const byte StructureVersion = 1;

	public static readonly DateTime EpochTime = DateTime.FromFileTimeUtc(0L);

	public byte CurrentVersion;

	public byte MaximumSupportedVersion;

	public ushort Reserved1;

	public uint Reserved2;

	public DateTime TimeStamp;

	public uint Reserved3;

	public byte[] ClientChallenge;

	public KeyValuePairList<AVPairKey, byte[]> AVPairs;

	public NTLMv2ClientChallenge()
	{
	}

	public NTLMv2ClientChallenge(DateTime timeStamp, byte[] clientChallenge, string domainName, string computerName)
	{
		CurrentVersion = 1;
		MaximumSupportedVersion = 1;
		TimeStamp = timeStamp;
		ClientChallenge = clientChallenge;
		AVPairs = new KeyValuePairList<AVPairKey, byte[]>();
		AVPairs.Add(AVPairKey.NbDomainName, Encoding.Unicode.GetBytes(domainName));
		AVPairs.Add(AVPairKey.NbComputerName, Encoding.Unicode.GetBytes(computerName));
	}

	public NTLMv2ClientChallenge(DateTime timeStamp, byte[] clientChallenge, KeyValuePairList<AVPairKey, byte[]> targetInfo)
		: this(timeStamp, clientChallenge, targetInfo, null)
	{
	}

	public NTLMv2ClientChallenge(DateTime timeStamp, byte[] clientChallenge, KeyValuePairList<AVPairKey, byte[]> targetInfo, string spn)
	{
		CurrentVersion = 1;
		MaximumSupportedVersion = 1;
		TimeStamp = timeStamp;
		ClientChallenge = clientChallenge;
		AVPairs = targetInfo;
		if (!string.IsNullOrEmpty(spn))
		{
			AVPairs.Add(AVPairKey.TargetName, Encoding.Unicode.GetBytes(spn));
		}
	}

	public NTLMv2ClientChallenge(byte[] buffer)
		: this(buffer, 0)
	{
	}

	public NTLMv2ClientChallenge(byte[] buffer, int offset)
	{
		CurrentVersion = ByteReader.ReadByte(buffer, offset);
		MaximumSupportedVersion = ByteReader.ReadByte(buffer, offset + 1);
		Reserved1 = LittleEndianConverter.ToUInt16(buffer, offset + 2);
		Reserved2 = LittleEndianConverter.ToUInt32(buffer, offset + 4);
		TimeStamp = FileTimeHelper.ReadFileTime(buffer, offset + 8);
		ClientChallenge = ByteReader.ReadBytes(buffer, offset + 16, 8);
		Reserved3 = LittleEndianConverter.ToUInt32(buffer, offset + 24);
		AVPairs = AVPairUtils.ReadAVPairSequence(buffer, offset + 28);
	}

	public byte[] GetBytes()
	{
		byte[] aVPairSequenceBytes = AVPairUtils.GetAVPairSequenceBytes(AVPairs);
		byte[] array = new byte[28 + aVPairSequenceBytes.Length];
		ByteWriter.WriteByte(array, 0, CurrentVersion);
		ByteWriter.WriteByte(array, 1, MaximumSupportedVersion);
		LittleEndianWriter.WriteUInt16(array, 2, Reserved1);
		LittleEndianWriter.WriteUInt32(array, 4, Reserved2);
		FileTimeHelper.WriteFileTime(array, 8, TimeStamp);
		ByteWriter.WriteBytes(array, 16, ClientChallenge, 8);
		LittleEndianWriter.WriteUInt32(array, 24, Reserved3);
		ByteWriter.WriteBytes(array, 28, aVPairSequenceBytes);
		return array;
	}

	public byte[] GetBytesPadded()
	{
		return ByteUtils.Concatenate(GetBytes(), new byte[4]);
	}
}
