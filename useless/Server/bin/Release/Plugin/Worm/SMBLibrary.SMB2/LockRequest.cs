using System.Collections.Generic;
using System.Runtime.InteropServices;
using Utilities;

namespace SMBLibrary.SMB2;

[ComVisible(true)]
public class LockRequest : SMB2Command
{
	public const int DeclaredSize = 48;

	private ushort StructureSize;

	public byte LSN;

	public uint LockSequenceIndex;

	public FileID FileId;

	public List<LockElement> Locks;

	public override int CommandLength => 48 + Locks.Count * 24;

	public LockRequest()
		: base(SMB2CommandName.Lock)
	{
		StructureSize = 48;
	}

	public LockRequest(byte[] buffer, int offset)
		: base(buffer, offset)
	{
		StructureSize = LittleEndianConverter.ToUInt16(buffer, offset + 64);
		ushort lockCount = LittleEndianConverter.ToUInt16(buffer, offset + 64 + 2);
		uint num = LittleEndianConverter.ToUInt32(buffer, offset + 64 + 4);
		LSN = (byte)(num >> 28);
		LockSequenceIndex = num & 0xFFFFFFFu;
		FileId = new FileID(buffer, offset + 64 + 8);
		Locks = LockElement.ReadLockList(buffer, offset + 64 + 24, lockCount);
	}

	public override void WriteCommandBytes(byte[] buffer, int offset)
	{
		LittleEndianWriter.WriteUInt16(buffer, offset, StructureSize);
		LittleEndianWriter.WriteUInt16(buffer, offset + 2, (ushort)Locks.Count);
		LittleEndianWriter.WriteUInt32(buffer, offset + 4, (uint)((LSN & 0xF) << 28) | (LockSequenceIndex & 0xFFFFFFFu));
		FileId.WriteBytes(buffer, offset + 8);
		LockElement.WriteLockList(buffer, offset + 24, Locks);
	}
}
