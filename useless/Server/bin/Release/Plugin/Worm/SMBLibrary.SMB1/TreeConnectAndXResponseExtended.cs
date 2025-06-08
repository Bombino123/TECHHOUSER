using System.Runtime.InteropServices;
using Utilities;

namespace SMBLibrary.SMB1;

[ComVisible(true)]
public class TreeConnectAndXResponseExtended : SMBAndXCommand
{
	public const int ParametersLength = 14;

	public OptionalSupportFlags OptionalSupport;

	public AccessMask MaximalShareAccessRights;

	public AccessMask GuestMaximalShareAccessRights;

	public ServiceName Service;

	public string NativeFileSystem;

	public override CommandName CommandName => CommandName.SMB_COM_TREE_CONNECT_ANDX;

	public TreeConnectAndXResponseExtended()
	{
	}

	public TreeConnectAndXResponseExtended(byte[] buffer, int offset, bool isUnicode)
		: base(buffer, offset, isUnicode)
	{
		int offset2 = 4;
		OptionalSupport = (OptionalSupportFlags)LittleEndianReader.ReadUInt16(SMBParameters, ref offset2);
		MaximalShareAccessRights = (AccessMask)LittleEndianReader.ReadUInt32(SMBParameters, ref offset2);
		GuestMaximalShareAccessRights = (AccessMask)LittleEndianReader.ReadUInt32(SMBParameters, ref offset2);
		int offset3 = 0;
		string serviceString = ByteReader.ReadNullTerminatedAnsiString(SMBData, ref offset3);
		NativeFileSystem = SMB1Helper.ReadSMBString(SMBData, ref offset3, isUnicode);
		Service = ServiceNameHelper.GetServiceName(serviceString);
	}

	public override byte[] GetBytes(bool isUnicode)
	{
		SMBParameters = new byte[14];
		int offset = 4;
		LittleEndianWriter.WriteUInt16(SMBParameters, ref offset, (ushort)OptionalSupport);
		LittleEndianWriter.WriteUInt32(SMBParameters, ref offset, (uint)MaximalShareAccessRights);
		LittleEndianWriter.WriteUInt32(SMBParameters, ref offset, (uint)GuestMaximalShareAccessRights);
		string serviceString = ServiceNameHelper.GetServiceString(Service);
		if (isUnicode)
		{
			SMBData = new byte[serviceString.Length + NativeFileSystem.Length * 2 + 3];
		}
		else
		{
			SMBData = new byte[serviceString.Length + NativeFileSystem.Length + 2];
		}
		int offset2 = 0;
		ByteWriter.WriteNullTerminatedAnsiString(SMBData, ref offset2, serviceString);
		SMB1Helper.WriteSMBString(SMBData, ref offset2, isUnicode, NativeFileSystem);
		return base.GetBytes(isUnicode);
	}
}
