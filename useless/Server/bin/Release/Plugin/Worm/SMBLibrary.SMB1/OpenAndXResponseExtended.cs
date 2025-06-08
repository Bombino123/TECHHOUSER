using System;
using System.Runtime.InteropServices;
using Utilities;

namespace SMBLibrary.SMB1;

[ComVisible(true)]
public class OpenAndXResponseExtended : SMBAndXCommand
{
	public const int ParametersLength = 38;

	public ushort FID;

	public SMBFileAttributes FileAttrs;

	public DateTime? LastWriteTime;

	public uint FileDataSize;

	public AccessRights AccessRights;

	public ResourceType ResourceType;

	public NamedPipeStatus NMPipeStatus;

	public OpenResults OpenResults;

	public uint ServerFID;

	public ushort Reserved;

	public AccessMask MaximalAccessRights;

	public AccessMask GuestMaximalAccessRights;

	public override CommandName CommandName => CommandName.SMB_COM_OPEN_ANDX;

	public OpenAndXResponseExtended()
	{
	}

	public OpenAndXResponseExtended(byte[] buffer, int offset)
		: base(buffer, offset, isUnicode: false)
	{
		throw new NotImplementedException();
	}

	public override byte[] GetBytes(bool isUnicode)
	{
		SMBParameters = new byte[38];
		int offset = 4;
		LittleEndianWriter.WriteUInt16(SMBParameters, ref offset, FID);
		LittleEndianWriter.WriteUInt16(SMBParameters, ref offset, (ushort)FileAttrs);
		UTimeHelper.WriteUTime(SMBParameters, ref offset, LastWriteTime);
		LittleEndianWriter.WriteUInt32(SMBParameters, ref offset, FileDataSize);
		LittleEndianWriter.WriteUInt16(SMBParameters, ref offset, (ushort)AccessRights);
		LittleEndianWriter.WriteUInt16(SMBParameters, ref offset, (ushort)ResourceType);
		NMPipeStatus.WriteBytes(SMBParameters, ref offset);
		OpenResults.WriteBytes(SMBParameters, ref offset);
		LittleEndianWriter.WriteUInt32(SMBParameters, ref offset, ServerFID);
		LittleEndianWriter.WriteUInt16(SMBParameters, ref offset, Reserved);
		LittleEndianWriter.WriteUInt32(SMBParameters, ref offset, (uint)MaximalAccessRights);
		LittleEndianWriter.WriteUInt32(SMBParameters, ref offset, (uint)GuestMaximalAccessRights);
		return base.GetBytes(isUnicode);
	}
}
