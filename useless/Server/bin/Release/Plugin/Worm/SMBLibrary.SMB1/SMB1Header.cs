using System.Runtime.InteropServices;
using Utilities;

namespace SMBLibrary.SMB1;

[ComVisible(true)]
public class SMB1Header
{
	public const int Length = 32;

	public static readonly byte[] ProtocolSignature = new byte[4] { 255, 83, 77, 66 };

	private byte[] Protocol;

	public CommandName Command;

	public NTStatus Status;

	public HeaderFlags Flags;

	public HeaderFlags2 Flags2;

	public ulong SecurityFeatures;

	public ushort TID;

	public ushort UID;

	public ushort MID;

	public uint PID;

	public bool ReplyFlag => (int)(Flags & HeaderFlags.Reply) > 0;

	public bool ExtendedSecurityFlag
	{
		get
		{
			return (int)(Flags2 & HeaderFlags2.ExtendedSecurity) > 0;
		}
		set
		{
			if (value)
			{
				Flags2 |= HeaderFlags2.ExtendedSecurity;
			}
			else
			{
				Flags2 &= ~HeaderFlags2.ExtendedSecurity;
			}
		}
	}

	public bool UnicodeFlag
	{
		get
		{
			return (int)(Flags2 & HeaderFlags2.Unicode) > 0;
		}
		set
		{
			if (value)
			{
				Flags2 |= HeaderFlags2.Unicode;
			}
			else
			{
				Flags2 &= ~HeaderFlags2.Unicode;
			}
		}
	}

	public SMB1Header()
	{
		Protocol = ProtocolSignature;
	}

	public SMB1Header(byte[] buffer)
	{
		Protocol = ByteReader.ReadBytes(buffer, 0, 4);
		Command = (CommandName)ByteReader.ReadByte(buffer, 4);
		Status = (NTStatus)LittleEndianConverter.ToUInt32(buffer, 5);
		Flags = (HeaderFlags)ByteReader.ReadByte(buffer, 9);
		Flags2 = (HeaderFlags2)LittleEndianConverter.ToUInt16(buffer, 10);
		ushort num = LittleEndianConverter.ToUInt16(buffer, 12);
		SecurityFeatures = LittleEndianConverter.ToUInt64(buffer, 14);
		TID = LittleEndianConverter.ToUInt16(buffer, 24);
		ushort num2 = LittleEndianConverter.ToUInt16(buffer, 26);
		UID = LittleEndianConverter.ToUInt16(buffer, 28);
		MID = LittleEndianConverter.ToUInt16(buffer, 30);
		PID = (uint)((num << 16) | num2);
	}

	public void WriteBytes(byte[] buffer, int offset)
	{
		ushort value = (ushort)(PID >> 16);
		ushort value2 = (ushort)(PID & 0xFFFFu);
		ByteWriter.WriteBytes(buffer, offset, Protocol);
		ByteWriter.WriteByte(buffer, offset + 4, (byte)Command);
		LittleEndianWriter.WriteUInt32(buffer, offset + 5, (uint)Status);
		ByteWriter.WriteByte(buffer, offset + 9, (byte)Flags);
		LittleEndianWriter.WriteUInt16(buffer, offset + 10, (ushort)Flags2);
		LittleEndianWriter.WriteUInt16(buffer, offset + 12, value);
		LittleEndianWriter.WriteUInt64(buffer, offset + 14, SecurityFeatures);
		LittleEndianWriter.WriteUInt16(buffer, offset + 24, TID);
		LittleEndianWriter.WriteUInt16(buffer, offset + 26, value2);
		LittleEndianWriter.WriteUInt16(buffer, offset + 28, UID);
		LittleEndianWriter.WriteUInt16(buffer, offset + 30, MID);
	}

	public byte[] GetBytes()
	{
		byte[] array = new byte[32];
		WriteBytes(array, 0);
		return array;
	}

	public static bool IsValidSMB1Header(byte[] buffer)
	{
		if (buffer.Length >= 4)
		{
			return ByteUtils.AreByteArraysEqual(ByteReader.ReadBytes(buffer, 0, 4), ProtocolSignature);
		}
		return false;
	}
}
