using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Utilities;

namespace SMBLibrary;

[ComVisible(true)]
public abstract class QueryDirectoryFileInformation : FileInformation
{
	public uint NextEntryOffset;

	public uint FileIndex;

	public QueryDirectoryFileInformation()
	{
	}

	public QueryDirectoryFileInformation(byte[] buffer, int offset)
	{
		NextEntryOffset = LittleEndianConverter.ToUInt32(buffer, offset);
		FileIndex = LittleEndianConverter.ToUInt32(buffer, offset + 4);
	}

	public override void WriteBytes(byte[] buffer, int offset)
	{
		LittleEndianWriter.WriteUInt32(buffer, offset, NextEntryOffset);
		LittleEndianWriter.WriteUInt32(buffer, offset + 4, FileIndex);
	}

	public static QueryDirectoryFileInformation ReadFileInformation(byte[] buffer, int offset, FileInformationClass fileInformationClass)
	{
		return fileInformationClass switch
		{
			FileInformationClass.FileDirectoryInformation => new FileDirectoryInformation(buffer, offset), 
			FileInformationClass.FileFullDirectoryInformation => new FileFullDirectoryInformation(buffer, offset), 
			FileInformationClass.FileBothDirectoryInformation => new FileBothDirectoryInformation(buffer, offset), 
			FileInformationClass.FileNamesInformation => new FileNamesInformation(buffer, offset), 
			FileInformationClass.FileIdBothDirectoryInformation => new FileIdBothDirectoryInformation(buffer, offset), 
			FileInformationClass.FileIdFullDirectoryInformation => new FileIdFullDirectoryInformation(buffer, offset), 
			_ => throw new NotImplementedException($"File information class {(int)fileInformationClass} is not supported."), 
		};
	}

	public static List<QueryDirectoryFileInformation> ReadFileInformationList(byte[] buffer, int offset, FileInformationClass fileInformationClass)
	{
		List<QueryDirectoryFileInformation> list = new List<QueryDirectoryFileInformation>();
		QueryDirectoryFileInformation queryDirectoryFileInformation;
		do
		{
			queryDirectoryFileInformation = ReadFileInformation(buffer, offset, fileInformationClass);
			list.Add(queryDirectoryFileInformation);
			offset += (int)queryDirectoryFileInformation.NextEntryOffset;
		}
		while (queryDirectoryFileInformation.NextEntryOffset != 0);
		return list;
	}

	public static byte[] GetBytes(List<QueryDirectoryFileInformation> fileInformationList)
	{
		byte[] array = new byte[GetListLength(fileInformationList)];
		int num = 0;
		for (int i = 0; i < fileInformationList.Count; i++)
		{
			QueryDirectoryFileInformation queryDirectoryFileInformation = fileInformationList[i];
			int num2 = (int)Math.Ceiling((double)queryDirectoryFileInformation.Length / 8.0) * 8;
			if (i < fileInformationList.Count - 1)
			{
				queryDirectoryFileInformation.NextEntryOffset = (uint)num2;
			}
			else
			{
				queryDirectoryFileInformation.NextEntryOffset = 0u;
			}
			queryDirectoryFileInformation.WriteBytes(array, num);
			num += num2;
		}
		return array;
	}

	public static int GetListLength(List<QueryDirectoryFileInformation> fileInformationList)
	{
		int num = 0;
		for (int i = 0; i < fileInformationList.Count; i++)
		{
			int length = fileInformationList[i].Length;
			if (i < fileInformationList.Count - 1)
			{
				int num2 = (int)Math.Ceiling((double)length / 8.0) * 8;
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
