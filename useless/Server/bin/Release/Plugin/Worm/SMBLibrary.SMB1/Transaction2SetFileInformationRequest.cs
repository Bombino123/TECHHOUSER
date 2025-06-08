using System.Runtime.InteropServices;
using Utilities;

namespace SMBLibrary.SMB1;

[ComVisible(true)]
public class Transaction2SetFileInformationRequest : Transaction2Subcommand
{
	private const ushort SMB_INFO_PASSTHROUGH = 1000;

	public const int ParametersLength = 6;

	public ushort FID;

	public ushort InformationLevel;

	public ushort Reserved;

	public byte[] InformationBytes;

	public bool IsPassthroughInformationLevel => InformationLevel >= 1000;

	public SetInformationLevel SetInformationLevel
	{
		get
		{
			return (SetInformationLevel)InformationLevel;
		}
		set
		{
			InformationLevel = (ushort)value;
		}
	}

	public FileInformationClass FileInformationClass
	{
		get
		{
			return (FileInformationClass)(InformationLevel - 1000);
		}
		set
		{
			InformationLevel = (ushort)((uint)value + 1000u);
		}
	}

	public override Transaction2SubcommandName SubcommandName => Transaction2SubcommandName.TRANS2_SET_FILE_INFORMATION;

	public Transaction2SetFileInformationRequest()
	{
	}

	public Transaction2SetFileInformationRequest(byte[] parameters, byte[] data, bool isUnicode)
	{
		FID = LittleEndianConverter.ToUInt16(parameters, 0);
		InformationLevel = LittleEndianConverter.ToUInt16(parameters, 2);
		Reserved = LittleEndianConverter.ToUInt16(parameters, 4);
		InformationBytes = data;
	}

	public override byte[] GetSetup()
	{
		return LittleEndianConverter.GetBytes((ushort)SubcommandName);
	}

	public override byte[] GetParameters(bool isUnicode)
	{
		byte[] array = new byte[6];
		LittleEndianWriter.WriteUInt16(array, 0, FID);
		LittleEndianWriter.WriteUInt16(array, 2, InformationLevel);
		LittleEndianWriter.WriteUInt16(array, 4, Reserved);
		return array;
	}

	public override byte[] GetData(bool isUnicode)
	{
		return InformationBytes;
	}

	public void SetInformation(SetInformation information)
	{
		SetInformationLevel = information.InformationLevel;
		InformationBytes = information.GetBytes();
	}

	public void SetInformation(FileInformation information)
	{
		FileInformationClass = information.FileInformationClass;
		InformationBytes = information.GetBytes();
	}
}
