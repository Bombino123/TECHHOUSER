using System.Runtime.InteropServices;
using Utilities;

namespace SMBLibrary.SMB1;

[ComVisible(true)]
public class Transaction2SetPathInformationRequest : Transaction2Subcommand
{
	private const ushort SMB_INFO_PASSTHROUGH = 1000;

	public const int ParametersFixedLength = 6;

	public ushort InformationLevel;

	public uint Reserved;

	public string FileName;

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

	public override Transaction2SubcommandName SubcommandName => Transaction2SubcommandName.TRANS2_SET_PATH_INFORMATION;

	public Transaction2SetPathInformationRequest()
	{
	}

	public Transaction2SetPathInformationRequest(byte[] parameters, byte[] data, bool isUnicode)
	{
		InformationLevel = LittleEndianConverter.ToUInt16(parameters, 0);
		Reserved = LittleEndianConverter.ToUInt32(parameters, 2);
		FileName = SMB1Helper.ReadSMBString(parameters, 6, isUnicode);
		InformationBytes = data;
	}

	public override byte[] GetSetup()
	{
		return LittleEndianConverter.GetBytes((ushort)SubcommandName);
	}

	public override byte[] GetParameters(bool isUnicode)
	{
		int num = 6;
		num = ((!isUnicode) ? (num + (FileName.Length + 1)) : (num + (FileName.Length * 2 + 2)));
		byte[] array = new byte[num];
		LittleEndianWriter.WriteUInt16(array, 0, InformationLevel);
		LittleEndianWriter.WriteUInt32(array, 2, Reserved);
		SMB1Helper.WriteSMBString(array, 6, isUnicode, FileName);
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
