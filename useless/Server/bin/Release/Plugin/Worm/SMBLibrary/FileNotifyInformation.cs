using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Utilities;

namespace SMBLibrary;

[ComVisible(true)]
public class FileNotifyInformation
{
	public const int FixedLength = 12;

	public uint NextEntryOffset;

	public FileAction Action;

	private uint FileNameLength;

	public string FileName;

	public int Length => 12 + FileName.Length * 2;

	public FileNotifyInformation()
	{
		FileName = string.Empty;
	}

	public FileNotifyInformation(byte[] buffer, int offset)
	{
		NextEntryOffset = LittleEndianConverter.ToUInt32(buffer, offset);
		Action = (FileAction)LittleEndianConverter.ToUInt32(buffer, offset + 4);
		FileNameLength = LittleEndianConverter.ToUInt32(buffer, offset + 8);
		FileName = ByteReader.ReadUTF16String(buffer, offset + 12, (int)(FileNameLength / 2));
	}

	public void WriteBytes(byte[] buffer, int offset)
	{
		FileNameLength = (uint)(FileName.Length * 2);
		LittleEndianWriter.WriteUInt32(buffer, offset, NextEntryOffset);
		LittleEndianWriter.WriteUInt32(buffer, offset + 4, (uint)Action);
		LittleEndianWriter.WriteUInt32(buffer, offset + 8, FileNameLength);
		ByteWriter.WriteUTF16String(buffer, offset + 12, FileName);
	}

	public static List<FileNotifyInformation> ReadList(byte[] buffer, int offset)
	{
		List<FileNotifyInformation> list = new List<FileNotifyInformation>();
		FileNotifyInformation fileNotifyInformation;
		do
		{
			fileNotifyInformation = new FileNotifyInformation(buffer, offset);
			list.Add(fileNotifyInformation);
			offset += (int)fileNotifyInformation.NextEntryOffset;
		}
		while (fileNotifyInformation.NextEntryOffset != 0);
		return list;
	}

	public static byte[] GetBytes(List<FileNotifyInformation> notifyInformationList)
	{
		byte[] array = new byte[GetListLength(notifyInformationList)];
		int num = 0;
		for (int i = 0; i < notifyInformationList.Count; i++)
		{
			FileNotifyInformation fileNotifyInformation = notifyInformationList[i];
			int num2 = (int)Math.Ceiling((double)fileNotifyInformation.Length / 4.0) * 4;
			if (i < notifyInformationList.Count - 1)
			{
				fileNotifyInformation.NextEntryOffset = (uint)num2;
			}
			else
			{
				fileNotifyInformation.NextEntryOffset = 0u;
			}
			fileNotifyInformation.WriteBytes(array, num);
			num += num2;
		}
		return array;
	}

	public static int GetListLength(List<FileNotifyInformation> notifyInformationList)
	{
		int num = 0;
		for (int i = 0; i < notifyInformationList.Count; i++)
		{
			int length = notifyInformationList[i].Length;
			if (i < notifyInformationList.Count - 1)
			{
				int num2 = (int)Math.Ceiling((double)length / 4.0) * 4;
				num += num2;
			}
			else
			{
				num += length;
			}
		}
		return num;
	}
}
