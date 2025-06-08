using System;
using System.Runtime.InteropServices;
using Utilities;

namespace SMBLibrary.SMB1;

[ComVisible(true)]
public class Transaction2Open2Response : Transaction2Subcommand
{
	public const int ParametersLength = 30;

	public ushort FID;

	public SMBFileAttributes FileAttributes;

	public DateTime? CreationTime;

	public uint FileDataSize;

	public AccessModeOptions AccessMode;

	public ResourceType ResourceType;

	public NamedPipeStatus NMPipeStatus;

	public ActionTaken ActionTaken;

	public uint Reserved;

	public ushort ExtendedAttributeErrorOffset;

	public uint ExtendedAttributeLength;

	public override Transaction2SubcommandName SubcommandName => Transaction2SubcommandName.TRANS2_OPEN2;

	public Transaction2Open2Response()
	{
	}

	public Transaction2Open2Response(byte[] parameters, byte[] data, bool isUnicode)
	{
		FID = LittleEndianConverter.ToUInt16(parameters, 0);
		FileAttributes = (SMBFileAttributes)LittleEndianConverter.ToUInt16(parameters, 2);
		CreationTime = UTimeHelper.ReadNullableUTime(parameters, 4);
		FileDataSize = LittleEndianConverter.ToUInt32(parameters, 8);
		AccessMode = new AccessModeOptions(parameters, 12);
		ResourceType = (ResourceType)LittleEndianConverter.ToUInt16(parameters, 14);
		NMPipeStatus = new NamedPipeStatus(parameters, 16);
		ActionTaken = new ActionTaken(parameters, 18);
		Reserved = LittleEndianConverter.ToUInt32(parameters, 20);
		ExtendedAttributeErrorOffset = LittleEndianConverter.ToUInt16(parameters, 24);
		ExtendedAttributeLength = LittleEndianConverter.ToUInt32(parameters, 26);
	}

	public override byte[] GetParameters(bool isUnicode)
	{
		byte[] array = new byte[30];
		LittleEndianWriter.WriteUInt16(array, 0, FID);
		LittleEndianWriter.WriteUInt16(array, 2, (ushort)FileAttributes);
		UTimeHelper.WriteUTime(array, 4, CreationTime);
		LittleEndianWriter.WriteUInt32(array, 8, FileDataSize);
		AccessMode.WriteBytes(array, 12);
		LittleEndianWriter.WriteUInt16(array, 14, (ushort)ResourceType);
		NMPipeStatus.WriteBytes(array, 16);
		ActionTaken.WriteBytes(array, 18);
		LittleEndianWriter.WriteUInt32(array, 20, Reserved);
		LittleEndianWriter.WriteUInt16(array, 24, ExtendedAttributeErrorOffset);
		LittleEndianWriter.WriteUInt32(array, 26, ExtendedAttributeLength);
		return array;
	}
}
