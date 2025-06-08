using System.Runtime.InteropServices;
using Utilities;

namespace SMBLibrary.SMB1;

[ComVisible(true)]
public class TreeConnectAndXResponse : SMBAndXCommand
{
	public const int ParametersLength = 6;

	public OptionalSupportFlags OptionalSupport;

	public ServiceName Service;

	public string NativeFileSystem;

	public override CommandName CommandName => CommandName.SMB_COM_TREE_CONNECT_ANDX;

	public TreeConnectAndXResponse()
	{
	}

	public TreeConnectAndXResponse(byte[] buffer, int offset, bool isUnicode)
		: base(buffer, offset, isUnicode)
	{
		OptionalSupport = (OptionalSupportFlags)LittleEndianConverter.ToUInt16(SMBParameters, 4);
		int offset2 = 0;
		string serviceString = ByteReader.ReadNullTerminatedAnsiString(SMBData, ref offset2);
		NativeFileSystem = SMB1Helper.ReadSMBString(SMBData, ref offset2, isUnicode);
		Service = ServiceNameHelper.GetServiceName(serviceString);
	}

	public override byte[] GetBytes(bool isUnicode)
	{
		SMBParameters = new byte[6];
		LittleEndianWriter.WriteUInt16(SMBParameters, 4, (ushort)OptionalSupport);
		string serviceString = ServiceNameHelper.GetServiceString(Service);
		if (isUnicode)
		{
			SMBData = new byte[serviceString.Length + NativeFileSystem.Length * 2 + 3];
		}
		else
		{
			SMBData = new byte[serviceString.Length + NativeFileSystem.Length + 2];
		}
		int offset = 0;
		ByteWriter.WriteNullTerminatedAnsiString(SMBData, ref offset, serviceString);
		SMB1Helper.WriteSMBString(SMBData, ref offset, isUnicode, NativeFileSystem);
		return base.GetBytes(isUnicode);
	}
}
