using System.Runtime.InteropServices;
using Utilities;

namespace SMBLibrary.SMB2;

[ComVisible(true)]
public class SessionSetupRequest : SMB2Command
{
	public const int FixedSize = 24;

	public const int DeclaredSize = 25;

	private ushort StructureSize;

	public SessionSetupFlags Flags;

	public SecurityMode SecurityMode;

	public Capabilities Capabilities;

	public uint Channel;

	private ushort SecurityBufferOffset;

	private ushort SecurityBufferLength;

	public ulong PreviousSessionId;

	public byte[] SecurityBuffer = new byte[0];

	public override int CommandLength => 24 + SecurityBuffer.Length;

	public SessionSetupRequest()
		: base(SMB2CommandName.SessionSetup)
	{
		StructureSize = 25;
	}

	public SessionSetupRequest(byte[] buffer, int offset)
		: base(buffer, offset)
	{
		StructureSize = LittleEndianConverter.ToUInt16(buffer, offset + 64);
		Flags = (SessionSetupFlags)ByteReader.ReadByte(buffer, offset + 64 + 2);
		SecurityMode = (SecurityMode)ByteReader.ReadByte(buffer, offset + 64 + 3);
		Capabilities = (Capabilities)LittleEndianConverter.ToUInt32(buffer, offset + 64 + 4);
		Channel = LittleEndianConverter.ToUInt32(buffer, offset + 64 + 8);
		SecurityBufferOffset = LittleEndianConverter.ToUInt16(buffer, offset + 64 + 12);
		SecurityBufferLength = LittleEndianConverter.ToUInt16(buffer, offset + 64 + 14);
		PreviousSessionId = LittleEndianConverter.ToUInt64(buffer, offset + 64 + 16);
		if (SecurityBufferLength > 0)
		{
			SecurityBuffer = ByteReader.ReadBytes(buffer, offset + SecurityBufferOffset, SecurityBufferLength);
		}
	}

	public override void WriteCommandBytes(byte[] buffer, int offset)
	{
		SecurityBufferOffset = 0;
		SecurityBufferLength = (ushort)SecurityBuffer.Length;
		if (SecurityBuffer.Length != 0)
		{
			SecurityBufferOffset = 88;
		}
		LittleEndianWriter.WriteUInt16(buffer, offset, StructureSize);
		ByteWriter.WriteByte(buffer, offset + 2, (byte)Flags);
		ByteWriter.WriteByte(buffer, offset + 3, (byte)SecurityMode);
		LittleEndianWriter.WriteUInt32(buffer, offset + 4, (uint)Capabilities);
		LittleEndianWriter.WriteUInt32(buffer, offset + 8, Channel);
		LittleEndianWriter.WriteUInt16(buffer, offset + 12, SecurityBufferOffset);
		LittleEndianWriter.WriteUInt16(buffer, offset + 14, SecurityBufferLength);
		LittleEndianWriter.WriteUInt64(buffer, offset + 16, PreviousSessionId);
		ByteWriter.WriteBytes(buffer, offset + 24, SecurityBuffer);
	}
}
