using System.Runtime.InteropServices;
using Utilities;

namespace SMBLibrary.SMB1;

[ComVisible(true)]
public class Transaction2QueryPathInformationRequest : Transaction2Subcommand
{
	private const ushort SMB_INFO_PASSTHROUGH = 1000;

	public const int ParametersFixedLength = 6;

	public ushort InformationLevel;

	public uint Reserved;

	public string FileName;

	public FullExtendedAttributeList GetExtendedAttributeList;

	public bool IsPassthroughInformationLevel => InformationLevel >= 1000;

	public QueryInformationLevel QueryInformationLevel
	{
		get
		{
			return (QueryInformationLevel)InformationLevel;
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

	public override Transaction2SubcommandName SubcommandName => Transaction2SubcommandName.TRANS2_QUERY_PATH_INFORMATION;

	public Transaction2QueryPathInformationRequest()
	{
		GetExtendedAttributeList = new FullExtendedAttributeList();
	}

	public Transaction2QueryPathInformationRequest(byte[] parameters, byte[] data, bool isUnicode)
	{
		InformationLevel = LittleEndianConverter.ToUInt16(parameters, 0);
		Reserved = LittleEndianConverter.ToUInt32(parameters, 4);
		FileName = SMB1Helper.ReadSMBString(parameters, 6, isUnicode);
		if (!IsPassthroughInformationLevel && QueryInformationLevel == QueryInformationLevel.SMB_INFO_QUERY_EAS_FROM_LIST)
		{
			GetExtendedAttributeList = new FullExtendedAttributeList(data, 0);
		}
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
		if (!IsPassthroughInformationLevel && QueryInformationLevel == QueryInformationLevel.SMB_INFO_QUERY_EAS_FROM_LIST)
		{
			return GetExtendedAttributeList.GetBytes();
		}
		return new byte[0];
	}
}
