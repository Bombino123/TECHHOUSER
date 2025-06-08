using System.Collections.Generic;
using System.Runtime.InteropServices;
using Utilities;

namespace SMBLibrary.SMB1;

[ComVisible(true)]
public class LockingAndXRequest : SMBAndXCommand
{
	public const int ParametersLength = 12;

	public ushort FID;

	public LockType TypeOfLock;

	public byte NewOpLockLevel;

	public uint Timeout;

	public List<LockingRange> Unlocks = new List<LockingRange>();

	public List<LockingRange> Locks = new List<LockingRange>();

	public override CommandName CommandName => CommandName.SMB_COM_LOCKING_ANDX;

	public LockingAndXRequest()
	{
	}

	public LockingAndXRequest(byte[] buffer, int offset)
		: base(buffer, offset, isUnicode: false)
	{
		FID = LittleEndianConverter.ToUInt16(SMBParameters, 4);
		TypeOfLock = (LockType)ByteReader.ReadByte(SMBParameters, 6);
		NewOpLockLevel = ByteReader.ReadByte(SMBParameters, 7);
		Timeout = LittleEndianConverter.ToUInt32(SMBParameters, 8);
		ushort num = LittleEndianConverter.ToUInt16(SMBParameters, 12);
		ushort num2 = LittleEndianConverter.ToUInt16(SMBParameters, 14);
		int offset2 = 0;
		if ((int)(TypeOfLock & LockType.LARGE_FILES) > 0)
		{
			for (int i = 0; i < num; i++)
			{
				LockingRange item = LockingRange.Read64(SMBData, ref offset2);
				Unlocks.Add(item);
			}
			for (int j = 0; j < num2; j++)
			{
				LockingRange item2 = LockingRange.Read64(SMBData, ref offset2);
				Locks.Add(item2);
			}
		}
		else
		{
			for (int k = 0; k < num; k++)
			{
				LockingRange item3 = LockingRange.Read32(SMBData, ref offset2);
				Unlocks.Add(item3);
			}
			for (int l = 0; l < num2; l++)
			{
				LockingRange item4 = LockingRange.Read32(SMBData, ref offset2);
				Locks.Add(item4);
			}
		}
	}

	public override byte[] GetBytes(bool isUnicode)
	{
		SMBParameters = new byte[12];
		LittleEndianWriter.WriteUInt16(SMBParameters, 4, FID);
		ByteWriter.WriteByte(SMBParameters, 6, (byte)TypeOfLock);
		ByteWriter.WriteByte(SMBParameters, 7, NewOpLockLevel);
		LittleEndianWriter.WriteUInt32(SMBParameters, 8, Timeout);
		LittleEndianWriter.WriteUInt16(SMBParameters, 12, (ushort)Unlocks.Count);
		LittleEndianWriter.WriteUInt16(SMBParameters, 14, (ushort)Locks.Count);
		bool flag = (int)(TypeOfLock & LockType.LARGE_FILES) > 0;
		int num = ((!flag) ? ((Unlocks.Count + Locks.Count) * 10) : ((Unlocks.Count + Locks.Count) * 20));
		int offset = 0;
		SMBData = new byte[num];
		for (int i = 0; i < Unlocks.Count; i++)
		{
			if (flag)
			{
				Unlocks[i].Write64(SMBData, ref offset);
			}
			else
			{
				Unlocks[i].Write32(SMBData, ref offset);
			}
		}
		for (int j = 0; j < Locks.Count; j++)
		{
			if (flag)
			{
				Locks[j].Write64(SMBData, ref offset);
			}
			else
			{
				Locks[j].Write32(SMBData, ref offset);
			}
		}
		return base.GetBytes(isUnicode);
	}
}
