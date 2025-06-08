using System.Runtime.InteropServices;
using Utilities;

namespace SMBLibrary.SMB1;

[ComVisible(true)]
public class Transaction2SetFSInformationRequest : Transaction2Subcommand
{
	private const ushort SMB_INFO_PASSTHROUGH = 1000;

	public const int ParametersLength = 4;

	public ushort FID;

	public ushort InformationLevel;

	public byte[] InformationBytes;

	public bool IsPassthroughInformationLevel => InformationLevel >= 1000;

	public FileSystemInformationClass FileSystemInformationClass
	{
		get
		{
			return (FileSystemInformationClass)(InformationLevel - 1000);
		}
		set
		{
			InformationLevel = (ushort)((uint)value + 1000u);
		}
	}

	public override Transaction2SubcommandName SubcommandName => Transaction2SubcommandName.TRANS2_SET_FS_INFORMATION;

	public Transaction2SetFSInformationRequest()
	{
	}

	public Transaction2SetFSInformationRequest(byte[] parameters, byte[] data, bool isUnicode)
	{
		FID = LittleEndianConverter.ToUInt16(parameters, 0);
		InformationLevel = LittleEndianConverter.ToUInt16(parameters, 2);
		InformationBytes = data;
	}

	public override byte[] GetSetup()
	{
		return LittleEndianConverter.GetBytes((ushort)SubcommandName);
	}

	public override byte[] GetParameters(bool isUnicode)
	{
		byte[] array = new byte[4];
		LittleEndianWriter.WriteUInt16(array, 0, FID);
		LittleEndianWriter.WriteUInt16(array, 2, InformationLevel);
		return array;
	}

	public override byte[] GetData(bool isUnicode)
	{
		return InformationBytes;
	}

	public void SetFileSystemInformation(FileSystemInformation information)
	{
		FileSystemInformationClass = information.FileSystemInformationClass;
		InformationBytes = information.GetBytes();
	}
}
