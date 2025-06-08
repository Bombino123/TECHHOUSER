using System;
using System.Runtime.InteropServices;
using Utilities;

namespace SMBLibrary.SMB1;

[ComVisible(true)]
public class NTTransactIOCTLRequest : NTTransactSubcommand
{
	public const int SetupLength = 8;

	public uint FunctionCode;

	public ushort FID;

	public bool IsFsctl;

	public bool IsFlags;

	public byte[] Data;

	public override NTTransactSubcommandName SubcommandName => NTTransactSubcommandName.NT_TRANSACT_IOCTL;

	public NTTransactIOCTLRequest()
	{
		Data = new byte[0];
	}

	public NTTransactIOCTLRequest(byte[] setup, byte[] data)
	{
		FunctionCode = LittleEndianConverter.ToUInt32(setup, 0);
		FID = LittleEndianConverter.ToUInt16(setup, 4);
		IsFsctl = ByteReader.ReadByte(setup, 6) != 0;
		IsFlags = ByteReader.ReadByte(setup, 7) != 0;
		Data = data;
	}

	public override byte[] GetSetup()
	{
		byte[] array = new byte[8];
		LittleEndianWriter.WriteUInt32(array, 0, FunctionCode);
		LittleEndianWriter.WriteUInt32(array, 4, FID);
		ByteWriter.WriteByte(array, 6, Convert.ToByte(IsFsctl));
		ByteWriter.WriteByte(array, 7, Convert.ToByte(IsFlags));
		return array;
	}

	public override byte[] GetData()
	{
		return Data;
	}
}
