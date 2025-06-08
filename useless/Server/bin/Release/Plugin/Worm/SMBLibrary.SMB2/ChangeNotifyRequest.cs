using System.Runtime.InteropServices;
using Utilities;

namespace SMBLibrary.SMB2;

[ComVisible(true)]
public class ChangeNotifyRequest : SMB2Command
{
	public const int DeclaredSize = 32;

	private ushort StructureSize;

	public ChangeNotifyFlags Flags;

	public uint OutputBufferLength;

	public FileID FileId;

	public NotifyChangeFilter CompletionFilter;

	public uint Reserved;

	public bool WatchTree
	{
		get
		{
			return (int)(Flags & ChangeNotifyFlags.WatchTree) > 0;
		}
		set
		{
			if (value)
			{
				Flags |= ChangeNotifyFlags.WatchTree;
			}
			else
			{
				Flags &= ~ChangeNotifyFlags.WatchTree;
			}
		}
	}

	public override int CommandLength => 32;

	public ChangeNotifyRequest()
		: base(SMB2CommandName.ChangeNotify)
	{
		StructureSize = 32;
	}

	public ChangeNotifyRequest(byte[] buffer, int offset)
		: base(buffer, offset)
	{
		StructureSize = LittleEndianConverter.ToUInt16(buffer, offset + 64);
		Flags = (ChangeNotifyFlags)LittleEndianConverter.ToUInt16(buffer, offset + 64 + 2);
		OutputBufferLength = LittleEndianConverter.ToUInt32(buffer, offset + 64 + 4);
		FileId = new FileID(buffer, offset + 64 + 8);
		CompletionFilter = (NotifyChangeFilter)LittleEndianConverter.ToUInt32(buffer, offset + 64 + 24);
		Reserved = LittleEndianConverter.ToUInt32(buffer, offset + 64 + 28);
	}

	public override void WriteCommandBytes(byte[] buffer, int offset)
	{
		LittleEndianWriter.WriteUInt16(buffer, offset, StructureSize);
		LittleEndianWriter.WriteUInt16(buffer, offset + 2, (ushort)Flags);
		LittleEndianWriter.WriteUInt32(buffer, offset + 4, OutputBufferLength);
		FileId.WriteBytes(buffer, offset + 8);
		LittleEndianWriter.WriteUInt32(buffer, offset + 24, (uint)CompletionFilter);
		LittleEndianWriter.WriteUInt32(buffer, offset + 28, Reserved);
	}
}
