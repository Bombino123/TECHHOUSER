using System.Runtime.InteropServices;
using Utilities;

namespace SMBLibrary.SMB2;

[ComVisible(true)]
public class SMB2Header
{
	public const int Length = 64;

	public const int SignatureOffset = 48;

	public static readonly byte[] ProtocolSignature = new byte[4] { 254, 83, 77, 66 };

	private byte[] ProtocolId;

	private ushort StructureSize;

	public ushort CreditCharge;

	public NTStatus Status;

	public SMB2CommandName Command;

	public ushort Credits;

	public SMB2PacketHeaderFlags Flags;

	public uint NextCommand;

	public ulong MessageID;

	public uint Reserved;

	public uint TreeID;

	public ulong AsyncID;

	public ulong SessionID;

	public byte[] Signature;

	public bool IsResponse
	{
		get
		{
			return (Flags & SMB2PacketHeaderFlags.ServerToRedir) != 0;
		}
		set
		{
			if (value)
			{
				Flags |= SMB2PacketHeaderFlags.ServerToRedir;
			}
			else
			{
				Flags &= ~SMB2PacketHeaderFlags.ServerToRedir;
			}
		}
	}

	public bool IsAsync
	{
		get
		{
			return (Flags & SMB2PacketHeaderFlags.AsyncCommand) != 0;
		}
		set
		{
			if (value)
			{
				Flags |= SMB2PacketHeaderFlags.AsyncCommand;
			}
			else
			{
				Flags &= ~SMB2PacketHeaderFlags.AsyncCommand;
			}
		}
	}

	public bool IsRelatedOperations
	{
		get
		{
			return (Flags & SMB2PacketHeaderFlags.RelatedOperations) != 0;
		}
		set
		{
			if (value)
			{
				Flags |= SMB2PacketHeaderFlags.RelatedOperations;
			}
			else
			{
				Flags &= ~SMB2PacketHeaderFlags.RelatedOperations;
			}
		}
	}

	public bool IsSigned
	{
		get
		{
			return (Flags & SMB2PacketHeaderFlags.Signed) != 0;
		}
		set
		{
			if (value)
			{
				Flags |= SMB2PacketHeaderFlags.Signed;
			}
			else
			{
				Flags &= ~SMB2PacketHeaderFlags.Signed;
			}
		}
	}

	public SMB2Header(SMB2CommandName commandName)
	{
		ProtocolId = ProtocolSignature;
		StructureSize = 64;
		Command = commandName;
		Signature = new byte[16];
	}

	public SMB2Header(byte[] buffer, int offset)
	{
		ProtocolId = ByteReader.ReadBytes(buffer, offset, 4);
		StructureSize = LittleEndianConverter.ToUInt16(buffer, offset + 4);
		CreditCharge = LittleEndianConverter.ToUInt16(buffer, offset + 6);
		Status = (NTStatus)LittleEndianConverter.ToUInt32(buffer, offset + 8);
		Command = (SMB2CommandName)LittleEndianConverter.ToUInt16(buffer, offset + 12);
		Credits = LittleEndianConverter.ToUInt16(buffer, offset + 14);
		Flags = (SMB2PacketHeaderFlags)LittleEndianConverter.ToUInt32(buffer, offset + 16);
		NextCommand = LittleEndianConverter.ToUInt32(buffer, offset + 20);
		MessageID = LittleEndianConverter.ToUInt64(buffer, offset + 24);
		if ((Flags & SMB2PacketHeaderFlags.AsyncCommand) != 0)
		{
			AsyncID = LittleEndianConverter.ToUInt64(buffer, offset + 32);
		}
		else
		{
			Reserved = LittleEndianConverter.ToUInt32(buffer, offset + 32);
			TreeID = LittleEndianConverter.ToUInt32(buffer, offset + 36);
		}
		SessionID = LittleEndianConverter.ToUInt64(buffer, offset + 40);
		if ((Flags & SMB2PacketHeaderFlags.Signed) != 0)
		{
			Signature = ByteReader.ReadBytes(buffer, offset + 48, 16);
		}
	}

	public void WriteBytes(byte[] buffer, int offset)
	{
		ByteWriter.WriteBytes(buffer, offset, ProtocolId);
		LittleEndianWriter.WriteUInt16(buffer, offset + 4, StructureSize);
		LittleEndianWriter.WriteUInt16(buffer, offset + 6, CreditCharge);
		LittleEndianWriter.WriteUInt32(buffer, offset + 8, (uint)Status);
		LittleEndianWriter.WriteUInt16(buffer, offset + 12, (ushort)Command);
		LittleEndianWriter.WriteUInt16(buffer, offset + 14, Credits);
		LittleEndianWriter.WriteUInt32(buffer, offset + 16, (uint)Flags);
		LittleEndianWriter.WriteUInt32(buffer, offset + 20, NextCommand);
		LittleEndianWriter.WriteUInt64(buffer, offset + 24, MessageID);
		if ((Flags & SMB2PacketHeaderFlags.AsyncCommand) != 0)
		{
			LittleEndianWriter.WriteUInt64(buffer, offset + 32, AsyncID);
		}
		else
		{
			LittleEndianWriter.WriteUInt32(buffer, offset + 32, Reserved);
			LittleEndianWriter.WriteUInt32(buffer, offset + 36, TreeID);
		}
		LittleEndianWriter.WriteUInt64(buffer, offset + 40, SessionID);
		if ((Flags & SMB2PacketHeaderFlags.Signed) != 0)
		{
			ByteWriter.WriteBytes(buffer, offset + 48, Signature);
		}
	}

	public static bool IsValidSMB2Header(byte[] buffer)
	{
		if (buffer.Length >= 4)
		{
			return ByteUtils.AreByteArraysEqual(ByteReader.ReadBytes(buffer, 0, 4), ProtocolSignature);
		}
		return false;
	}
}
