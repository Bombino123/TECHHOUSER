using System.Runtime.InteropServices;
using Utilities;

namespace SMBLibrary.SMB2;

[ComVisible(true)]
public class SessionSetupResponse : SMB2Command
{
	public const int FixedSize = 8;

	public const int DeclaredSize = 9;

	private ushort StructureSize;

	public SessionFlags SessionFlags;

	private ushort SecurityBufferOffset;

	private ushort SecurityBufferLength;

	public byte[] SecurityBuffer = new byte[0];

	public override int CommandLength => 8 + SecurityBuffer.Length;

	public SessionSetupResponse()
		: base(SMB2CommandName.SessionSetup)
	{
		Header.IsResponse = true;
		StructureSize = 9;
	}

	public SessionSetupResponse(byte[] buffer, int offset)
		: base(buffer, offset)
	{
		StructureSize = LittleEndianConverter.ToUInt16(buffer, offset + 64);
		SessionFlags = (SessionFlags)LittleEndianConverter.ToUInt16(buffer, offset + 64 + 2);
		SecurityBufferOffset = LittleEndianConverter.ToUInt16(buffer, offset + 64 + 4);
		SecurityBufferLength = LittleEndianConverter.ToUInt16(buffer, offset + 64 + 6);
		SecurityBuffer = ByteReader.ReadBytes(buffer, offset + SecurityBufferOffset, SecurityBufferLength);
	}

	public override void WriteCommandBytes(byte[] buffer, int offset)
	{
		SecurityBufferOffset = 0;
		SecurityBufferLength = (ushort)SecurityBuffer.Length;
		if (SecurityBuffer.Length != 0)
		{
			SecurityBufferOffset = 72;
		}
		LittleEndianWriter.WriteUInt16(buffer, offset, StructureSize);
		LittleEndianWriter.WriteUInt16(buffer, offset + 2, (ushort)SessionFlags);
		LittleEndianWriter.WriteUInt16(buffer, offset + 4, SecurityBufferOffset);
		LittleEndianWriter.WriteUInt16(buffer, offset + 6, SecurityBufferLength);
		ByteWriter.WriteBytes(buffer, offset + 8, SecurityBuffer);
	}
}
