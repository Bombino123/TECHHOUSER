using System.Runtime.InteropServices;
using Utilities;

namespace SMBLibrary.SMB1;

[ComVisible(true)]
public class Transaction2QueryFileInformationRequest : Transaction2Subcommand
{
	private const ushort SMB_INFO_PASSTHROUGH = 1000;

	public const int ParametersLength = 4;

	public ushort FID;

	public ushort InformationLevel;

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

	public override Transaction2SubcommandName SubcommandName => Transaction2SubcommandName.TRANS2_QUERY_FILE_INFORMATION;

	public Transaction2QueryFileInformationRequest()
	{
		GetExtendedAttributeList = new FullExtendedAttributeList();
	}

	public Transaction2QueryFileInformationRequest(byte[] parameters, byte[] data, bool isUnicode)
	{
		FID = LittleEndianConverter.ToUInt16(parameters, 0);
		InformationLevel = LittleEndianConverter.ToUInt16(parameters, 2);
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
		byte[] array = new byte[4];
		LittleEndianWriter.WriteUInt16(array, 0, FID);
		LittleEndianWriter.WriteUInt16(array, 2, InformationLevel);
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
