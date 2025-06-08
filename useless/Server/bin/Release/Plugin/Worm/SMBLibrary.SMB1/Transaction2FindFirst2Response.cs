using System;
using System.Runtime.InteropServices;
using Utilities;

namespace SMBLibrary.SMB1;

[ComVisible(true)]
public class Transaction2FindFirst2Response : Transaction2Subcommand
{
	public const int ParametersLength = 10;

	public ushort SID;

	private ushort SearchCount;

	public bool EndOfSearch;

	public ushort EaErrorOffset;

	public ushort LastNameOffset;

	private byte[] FindInformationListBytes = new byte[0];

	public override Transaction2SubcommandName SubcommandName => Transaction2SubcommandName.TRANS2_FIND_FIRST2;

	public Transaction2FindFirst2Response()
	{
	}

	public Transaction2FindFirst2Response(byte[] parameters, byte[] data, bool isUnicode)
	{
		SID = LittleEndianConverter.ToUInt16(parameters, 0);
		SearchCount = LittleEndianConverter.ToUInt16(parameters, 2);
		EndOfSearch = LittleEndianConverter.ToUInt16(parameters, 4) != 0;
		EaErrorOffset = LittleEndianConverter.ToUInt16(parameters, 6);
		LastNameOffset = LittleEndianConverter.ToUInt16(parameters, 8);
		FindInformationListBytes = data;
	}

	public override byte[] GetParameters(bool isUnicode)
	{
		byte[] array = new byte[10];
		LittleEndianWriter.WriteUInt16(array, 0, SID);
		LittleEndianWriter.WriteUInt16(array, 2, SearchCount);
		LittleEndianWriter.WriteUInt16(array, 4, Convert.ToUInt16(EndOfSearch));
		LittleEndianWriter.WriteUInt16(array, 6, EaErrorOffset);
		LittleEndianWriter.WriteUInt16(array, 8, LastNameOffset);
		return array;
	}

	public override byte[] GetData(bool isUnicode)
	{
		return FindInformationListBytes;
	}

	public FindInformationList GetFindInformationList(FindInformationLevel findInformationLevel, bool isUnicode)
	{
		return new FindInformationList(FindInformationListBytes, findInformationLevel, isUnicode);
	}

	public void SetFindInformationList(FindInformationList findInformationList, bool isUnicode)
	{
		SearchCount = (ushort)findInformationList.Count;
		FindInformationListBytes = findInformationList.GetBytes(isUnicode);
	}
}
