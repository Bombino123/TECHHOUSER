using System;
using System.Runtime.InteropServices;
using Utilities;

namespace SMBLibrary.SMB1;

[ComVisible(true)]
public class Transaction2Open2Request : Transaction2Subcommand
{
	public Open2Flags Flags;

	public AccessModeOptions AccessMode;

	public ushort Reserved1;

	public SMBFileAttributes FileAttributes;

	public DateTime? CreationTime;

	public OpenMode OpenMode;

	public uint AllocationSize;

	public byte[] Reserved;

	public string FileName;

	public FullExtendedAttributeList ExtendedAttributeList;

	public override Transaction2SubcommandName SubcommandName => Transaction2SubcommandName.TRANS2_OPEN2;

	public Transaction2Open2Request()
	{
		Reserved = new byte[10];
	}

	public Transaction2Open2Request(byte[] parameters, byte[] data, bool isUnicode)
	{
		Flags = (Open2Flags)LittleEndianConverter.ToUInt16(parameters, 0);
		AccessMode = new AccessModeOptions(parameters, 2);
		Reserved1 = LittleEndianConverter.ToUInt16(parameters, 4);
		FileAttributes = (SMBFileAttributes)LittleEndianConverter.ToUInt16(parameters, 6);
		CreationTime = UTimeHelper.ReadNullableUTime(parameters, 8);
		OpenMode = new OpenMode(parameters, 12);
		AllocationSize = LittleEndianConverter.ToUInt32(parameters, 14);
		Reserved = ByteReader.ReadBytes(parameters, 18, 10);
		FileName = SMB1Helper.ReadSMBString(parameters, 28, isUnicode);
		ExtendedAttributeList = new FullExtendedAttributeList(data, 0);
	}

	public override byte[] GetSetup()
	{
		return LittleEndianConverter.GetBytes((ushort)SubcommandName);
	}

	public override byte[] GetParameters(bool isUnicode)
	{
		int num = 28;
		num = ((!isUnicode) ? (num + (FileName.Length + 1)) : (num + (FileName.Length * 2 + 2)));
		byte[] array = new byte[num];
		LittleEndianWriter.WriteUInt16(array, 0, (ushort)Flags);
		AccessMode.WriteBytes(array, 2);
		LittleEndianWriter.WriteUInt16(array, 4, Reserved1);
		LittleEndianWriter.WriteUInt16(array, 6, (ushort)FileAttributes);
		UTimeHelper.WriteUTime(array, 8, CreationTime);
		OpenMode.WriteBytes(array, 12);
		LittleEndianWriter.WriteUInt32(array, 14, AllocationSize);
		ByteWriter.WriteBytes(array, 18, Reserved, 10);
		SMB1Helper.WriteSMBString(array, 28, isUnicode, FileName);
		return array;
	}

	public override byte[] GetData(bool isUnicode)
	{
		return ExtendedAttributeList.GetBytes();
	}
}
