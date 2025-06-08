using System;
using System.Runtime.InteropServices;
using Utilities;

namespace SMBLibrary.SMB1;

[ComVisible(true)]
public class NTTransactNotifyChangeRequest : NTTransactSubcommand
{
	public const int SetupLength = 8;

	public NotifyChangeFilter CompletionFilter;

	public ushort FID;

	public bool WatchTree;

	public byte Reserved;

	public override NTTransactSubcommandName SubcommandName => NTTransactSubcommandName.NT_TRANSACT_NOTIFY_CHANGE;

	public NTTransactNotifyChangeRequest()
	{
	}

	public NTTransactNotifyChangeRequest(byte[] setup)
	{
		CompletionFilter = (NotifyChangeFilter)LittleEndianConverter.ToUInt32(setup, 0);
		FID = LittleEndianConverter.ToUInt16(setup, 4);
		WatchTree = ByteReader.ReadByte(setup, 6) != 0;
		Reserved = ByteReader.ReadByte(setup, 7);
	}

	public override byte[] GetSetup()
	{
		byte[] array = new byte[8];
		LittleEndianWriter.WriteUInt32(array, 0, (uint)CompletionFilter);
		LittleEndianWriter.WriteUInt32(array, 4, FID);
		ByteWriter.WriteByte(array, 6, Convert.ToByte(WatchTree));
		ByteWriter.WriteByte(array, 7, Reserved);
		return array;
	}
}
