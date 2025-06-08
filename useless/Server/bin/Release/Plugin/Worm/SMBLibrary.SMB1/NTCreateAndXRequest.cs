using System.Runtime.InteropServices;
using Utilities;

namespace SMBLibrary.SMB1;

[ComVisible(true)]
public class NTCreateAndXRequest : SMBAndXCommand
{
	public const int ParametersLength = 48;

	public byte Reserved;

	public NTCreateFlags Flags;

	public uint RootDirectoryFID;

	public AccessMask DesiredAccess;

	public long AllocationSize;

	public ExtendedFileAttributes ExtFileAttributes;

	public ShareAccess ShareAccess;

	public CreateDisposition CreateDisposition;

	public CreateOptions CreateOptions;

	public ImpersonationLevel ImpersonationLevel;

	public SecurityFlags SecurityFlags;

	public string FileName;

	public override CommandName CommandName => CommandName.SMB_COM_NT_CREATE_ANDX;

	public NTCreateAndXRequest()
	{
	}

	public NTCreateAndXRequest(byte[] buffer, int offset, bool isUnicode)
		: base(buffer, offset, isUnicode)
	{
		Reserved = ByteReader.ReadByte(SMBParameters, 4);
		LittleEndianConverter.ToUInt16(SMBParameters, 5);
		Flags = (NTCreateFlags)LittleEndianConverter.ToUInt32(SMBParameters, 7);
		RootDirectoryFID = LittleEndianConverter.ToUInt32(SMBParameters, 11);
		DesiredAccess = (AccessMask)LittleEndianConverter.ToUInt32(SMBParameters, 15);
		AllocationSize = LittleEndianConverter.ToInt64(SMBParameters, 19);
		ExtFileAttributes = (ExtendedFileAttributes)LittleEndianConverter.ToUInt32(SMBParameters, 27);
		ShareAccess = (ShareAccess)LittleEndianConverter.ToUInt32(SMBParameters, 31);
		CreateDisposition = (CreateDisposition)LittleEndianConverter.ToUInt32(SMBParameters, 35);
		CreateOptions = (CreateOptions)LittleEndianConverter.ToUInt32(SMBParameters, 39);
		ImpersonationLevel = (ImpersonationLevel)LittleEndianConverter.ToUInt32(SMBParameters, 43);
		SecurityFlags = (SecurityFlags)ByteReader.ReadByte(SMBParameters, 47);
		int offset2 = 0;
		if (isUnicode)
		{
			offset2 = 1;
		}
		FileName = SMB1Helper.ReadSMBString(SMBData, offset2, isUnicode);
	}

	public override byte[] GetBytes(bool isUnicode)
	{
		ushort num = (ushort)FileName.Length;
		if (isUnicode)
		{
			num *= 2;
		}
		SMBParameters = new byte[48];
		ByteWriter.WriteByte(SMBParameters, 4, Reserved);
		LittleEndianWriter.WriteUInt16(SMBParameters, 5, num);
		LittleEndianWriter.WriteUInt32(SMBParameters, 7, (uint)Flags);
		LittleEndianWriter.WriteUInt32(SMBParameters, 11, RootDirectoryFID);
		LittleEndianWriter.WriteUInt32(SMBParameters, 15, (uint)DesiredAccess);
		LittleEndianWriter.WriteInt64(SMBParameters, 19, AllocationSize);
		LittleEndianWriter.WriteUInt32(SMBParameters, 27, (uint)ExtFileAttributes);
		LittleEndianWriter.WriteUInt32(SMBParameters, 31, (uint)ShareAccess);
		LittleEndianWriter.WriteUInt32(SMBParameters, 35, (uint)CreateDisposition);
		LittleEndianWriter.WriteUInt32(SMBParameters, 39, (uint)CreateOptions);
		LittleEndianWriter.WriteUInt32(SMBParameters, 43, (uint)ImpersonationLevel);
		ByteWriter.WriteByte(SMBParameters, 47, (byte)SecurityFlags);
		int num2 = 0;
		if (isUnicode)
		{
			num2 = 1;
			SMBData = new byte[num2 + FileName.Length * 2 + 2];
		}
		else
		{
			SMBData = new byte[FileName.Length + 1];
		}
		SMB1Helper.WriteSMBString(SMBData, num2, isUnicode, FileName);
		return base.GetBytes(isUnicode);
	}
}
