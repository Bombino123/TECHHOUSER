using System.Runtime.InteropServices;
using Utilities;

namespace SMBLibrary.SMB1;

[ComVisible(true)]
public class TreeConnectAndXRequest : SMBAndXCommand
{
	public const int ParametersLength = 8;

	public TreeConnectFlags Flags;

	public byte[] Password;

	public string Path;

	public ServiceName Service;

	public override CommandName CommandName => CommandName.SMB_COM_TREE_CONNECT_ANDX;

	public TreeConnectAndXRequest()
	{
		Password = new byte[0];
	}

	public TreeConnectAndXRequest(byte[] buffer, int offset, bool isUnicode)
		: base(buffer, offset, isUnicode)
	{
		int offset2 = 4;
		Flags = (TreeConnectFlags)LittleEndianReader.ReadUInt16(SMBParameters, ref offset2);
		ushort num = LittleEndianReader.ReadUInt16(SMBParameters, ref offset2);
		int offset3 = 0;
		Password = ByteReader.ReadBytes(SMBData, ref offset3, num);
		if (isUnicode)
		{
			int num2 = (1 + num) % 2;
			offset3 += num2;
		}
		Path = SMB1Helper.ReadSMBString(SMBData, ref offset3, isUnicode);
		string serviceString = ByteReader.ReadNullTerminatedAnsiString(SMBData, ref offset3);
		Service = ServiceNameHelper.GetServiceName(serviceString);
	}

	public override byte[] GetBytes(bool isUnicode)
	{
		ushort num = (ushort)Password.Length;
		SMBParameters = new byte[8];
		int offset = 4;
		LittleEndianWriter.WriteUInt16(SMBParameters, ref offset, (ushort)Flags);
		LittleEndianWriter.WriteUInt16(SMBParameters, ref offset, num);
		string serviceString = ServiceNameHelper.GetServiceString(Service);
		int num2 = Password.Length + serviceString.Length + 1;
		if (isUnicode)
		{
			int num3 = (1 + num) % 2;
			num2 += Path.Length * 2 + 2 + num3;
		}
		else
		{
			num2 += Path.Length + 1;
		}
		SMBData = new byte[num2];
		int offset2 = 0;
		ByteWriter.WriteBytes(SMBData, ref offset2, Password);
		if (isUnicode)
		{
			int num4 = (1 + num) % 2;
			offset2 += num4;
		}
		SMB1Helper.WriteSMBString(SMBData, ref offset2, isUnicode, Path);
		ByteWriter.WriteNullTerminatedAnsiString(SMBData, ref offset2, serviceString);
		return base.GetBytes(isUnicode);
	}
}
