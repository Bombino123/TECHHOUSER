using System.Runtime.InteropServices;
using Utilities;

namespace SMBLibrary.SMB1;

[ComVisible(true)]
public class Transaction2FindFirst2Request : Transaction2Subcommand
{
	public SMBFileAttributes SearchAttributes;

	public ushort SearchCount;

	public FindFlags Flags;

	public FindInformationLevel InformationLevel;

	public SearchStorageType SearchStorageType;

	public string FileName;

	public ExtendedAttributeNameList GetExtendedAttributeList;

	public bool CloseAfterRequest
	{
		get
		{
			return (int)(Flags & FindFlags.SMB_FIND_CLOSE_AFTER_REQUEST) > 0;
		}
		set
		{
			if (value)
			{
				Flags |= FindFlags.SMB_FIND_CLOSE_AFTER_REQUEST;
			}
			else
			{
				Flags &= ~FindFlags.SMB_FIND_CLOSE_AFTER_REQUEST;
			}
		}
	}

	public bool CloseAtEndOfSearch
	{
		get
		{
			return (int)(Flags & FindFlags.SMB_FIND_CLOSE_AT_EOS) > 0;
		}
		set
		{
			if (value)
			{
				Flags |= FindFlags.SMB_FIND_CLOSE_AT_EOS;
			}
			else
			{
				Flags &= ~FindFlags.SMB_FIND_CLOSE_AT_EOS;
			}
		}
	}

	public override Transaction2SubcommandName SubcommandName => Transaction2SubcommandName.TRANS2_FIND_FIRST2;

	public Transaction2FindFirst2Request()
	{
		GetExtendedAttributeList = new ExtendedAttributeNameList();
	}

	public Transaction2FindFirst2Request(byte[] parameters, byte[] data, bool isUnicode)
	{
		SearchAttributes = (SMBFileAttributes)LittleEndianConverter.ToUInt16(parameters, 0);
		SearchCount = LittleEndianConverter.ToUInt16(parameters, 2);
		Flags = (FindFlags)LittleEndianConverter.ToUInt16(parameters, 4);
		InformationLevel = (FindInformationLevel)LittleEndianConverter.ToUInt16(parameters, 6);
		SearchStorageType = (SearchStorageType)LittleEndianConverter.ToUInt32(parameters, 8);
		FileName = SMB1Helper.ReadSMBString(parameters, 12, isUnicode);
		if (InformationLevel == FindInformationLevel.SMB_INFO_QUERY_EAS_FROM_LIST)
		{
			GetExtendedAttributeList = new ExtendedAttributeNameList(data, 0);
		}
	}

	public override byte[] GetSetup()
	{
		return LittleEndianConverter.GetBytes((ushort)SubcommandName);
	}

	public override byte[] GetParameters(bool isUnicode)
	{
		int num = 12;
		num = ((!isUnicode) ? (num + (FileName.Length + 1)) : (num + (FileName.Length * 2 + 2)));
		byte[] array = new byte[num];
		LittleEndianWriter.WriteUInt16(array, 0, (ushort)SearchAttributes);
		LittleEndianWriter.WriteUInt16(array, 2, SearchCount);
		LittleEndianWriter.WriteUInt16(array, 4, (ushort)Flags);
		LittleEndianWriter.WriteUInt16(array, 6, (ushort)InformationLevel);
		LittleEndianWriter.WriteUInt32(array, 8, (uint)SearchStorageType);
		SMB1Helper.WriteSMBString(array, 12, isUnicode, FileName);
		return array;
	}

	public override byte[] GetData(bool isUnicode)
	{
		if (InformationLevel == FindInformationLevel.SMB_INFO_QUERY_EAS_FROM_LIST)
		{
			return GetExtendedAttributeList.GetBytes();
		}
		return new byte[0];
	}
}
