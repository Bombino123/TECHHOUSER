using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace SMBLibrary;

[ComVisible(true)]
public class FileFullEAInformation : FileInformation
{
	private List<FileFullEAEntry> m_entries = new List<FileFullEAEntry>();

	public List<FileFullEAEntry> Entries => m_entries;

	public override FileInformationClass FileInformationClass => FileInformationClass.FileFullEaInformation;

	public override int Length
	{
		get
		{
			int num = 0;
			for (int i = 0; i < m_entries.Count; i++)
			{
				num += m_entries[i].Length;
				if (i < m_entries.Count - 1)
				{
					int num2 = (4 - num % 4) % 4;
					num += num2;
				}
			}
			return num;
		}
	}

	public FileFullEAInformation()
	{
	}

	public FileFullEAInformation(byte[] buffer, int offset)
	{
		m_entries = ReadList(buffer, offset);
	}

	public override void WriteBytes(byte[] buffer, int offset)
	{
		WriteList(buffer, offset, m_entries);
	}

	public static List<FileFullEAEntry> ReadList(byte[] buffer, int offset)
	{
		List<FileFullEAEntry> list = new List<FileFullEAEntry>();
		if (offset < buffer.Length)
		{
			FileFullEAEntry fileFullEAEntry;
			do
			{
				fileFullEAEntry = new FileFullEAEntry(buffer, offset);
				list.Add(fileFullEAEntry);
				offset += (int)fileFullEAEntry.NextEntryOffset;
			}
			while (fileFullEAEntry.NextEntryOffset != 0);
		}
		return list;
	}

	public static void WriteList(byte[] buffer, int offset, List<FileFullEAEntry> list)
	{
		for (int i = 0; i < list.Count; i++)
		{
			FileFullEAEntry fileFullEAEntry = list[i];
			fileFullEAEntry.WriteBytes(buffer, offset);
			int length = fileFullEAEntry.Length;
			offset += length;
			if (i < list.Count - 1)
			{
				int num = (4 - length % 4) % 4;
				offset += num;
			}
		}
	}
}
