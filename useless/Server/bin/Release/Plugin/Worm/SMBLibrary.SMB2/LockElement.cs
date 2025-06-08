using System.Collections.Generic;
using System.Runtime.InteropServices;
using Utilities;

namespace SMBLibrary.SMB2;

[ComVisible(true)]
public struct LockElement
{
	public const int StructureLength = 24;

	public ulong Offset;

	public ulong Length;

	public LockFlags Flags;

	public uint Reserved;

	public bool SharedLock
	{
		get
		{
			return (Flags & LockFlags.SharedLock) != 0;
		}
		set
		{
			if (value)
			{
				Flags |= LockFlags.SharedLock;
			}
			else
			{
				Flags &= ~LockFlags.SharedLock;
			}
		}
	}

	public bool ExclusiveLock
	{
		get
		{
			return (Flags & LockFlags.ExclusiveLock) != 0;
		}
		set
		{
			if (value)
			{
				Flags |= LockFlags.ExclusiveLock;
			}
			else
			{
				Flags &= ~LockFlags.ExclusiveLock;
			}
		}
	}

	public bool Unlock
	{
		get
		{
			return (Flags & LockFlags.Unlock) != 0;
		}
		set
		{
			if (value)
			{
				Flags |= LockFlags.Unlock;
			}
			else
			{
				Flags &= ~LockFlags.Unlock;
			}
		}
	}

	public bool FailImmediately
	{
		get
		{
			return (Flags & LockFlags.FailImmediately) != 0;
		}
		set
		{
			if (value)
			{
				Flags |= LockFlags.FailImmediately;
			}
			else
			{
				Flags &= ~LockFlags.FailImmediately;
			}
		}
	}

	public LockElement(byte[] buffer, int offset)
	{
		Offset = LittleEndianConverter.ToUInt64(buffer, offset);
		Length = LittleEndianConverter.ToUInt64(buffer, offset + 8);
		Flags = (LockFlags)LittleEndianConverter.ToUInt32(buffer, offset + 16);
		Reserved = LittleEndianConverter.ToUInt32(buffer, offset + 20);
	}

	public void WriteBytes(byte[] buffer, int offset)
	{
		LittleEndianWriter.WriteUInt64(buffer, offset, Offset);
		LittleEndianWriter.WriteUInt64(buffer, offset + 8, Length);
		LittleEndianWriter.WriteUInt64(buffer, offset + 16, (ulong)Flags);
		LittleEndianWriter.WriteUInt64(buffer, offset + 20, Reserved);
	}

	public static List<LockElement> ReadLockList(byte[] buffer, int offset, int lockCount)
	{
		List<LockElement> list = new List<LockElement>();
		for (int i = 0; i < lockCount; i++)
		{
			LockElement item = new LockElement(buffer, offset + i * 24);
			list.Add(item);
		}
		return list;
	}

	public static void WriteLockList(byte[] buffer, int offset, List<LockElement> locks)
	{
		for (int i = 0; i < locks.Count; i++)
		{
			locks[i].WriteBytes(buffer, offset + i * 24);
		}
	}
}
