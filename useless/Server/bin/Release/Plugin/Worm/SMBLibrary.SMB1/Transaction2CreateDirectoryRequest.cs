using System.Runtime.InteropServices;
using Utilities;

namespace SMBLibrary.SMB1;

[ComVisible(true)]
public class Transaction2CreateDirectoryRequest : Transaction2Subcommand
{
	public uint Reserved;

	public string DirectoryName;

	public FullExtendedAttributeList ExtendedAttributeList;

	public override Transaction2SubcommandName SubcommandName => Transaction2SubcommandName.TRANS2_CREATE_DIRECTORY;

	public Transaction2CreateDirectoryRequest()
	{
	}

	public Transaction2CreateDirectoryRequest(byte[] parameters, byte[] data, bool isUnicode)
	{
		Reserved = LittleEndianConverter.ToUInt32(parameters, 0);
		DirectoryName = SMB1Helper.ReadSMBString(parameters, 4, isUnicode);
		ExtendedAttributeList = new FullExtendedAttributeList(data);
	}

	public override byte[] GetSetup()
	{
		return LittleEndianConverter.GetBytes((ushort)SubcommandName);
	}

	public override byte[] GetParameters(bool isUnicode)
	{
		byte[] array = new byte[4 + (isUnicode ? (DirectoryName.Length * 2 + 2) : (DirectoryName.Length + 1 + 1))];
		LittleEndianWriter.WriteUInt32(array, 0, Reserved);
		SMB1Helper.WriteSMBString(array, 4, isUnicode, DirectoryName);
		return array;
	}

	public override byte[] GetData(bool isUnicode)
	{
		return ExtendedAttributeList.GetBytes();
	}
}
