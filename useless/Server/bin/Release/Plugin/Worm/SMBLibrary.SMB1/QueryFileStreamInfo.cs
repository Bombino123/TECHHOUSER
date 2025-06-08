using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace SMBLibrary.SMB1;

[ComVisible(true)]
public class QueryFileStreamInfo : QueryInformation
{
	private List<FileStreamEntry> m_entries = new List<FileStreamEntry>();

	public List<FileStreamEntry> Entries => m_entries;

	public override QueryInformationLevel InformationLevel => QueryInformationLevel.SMB_QUERY_FILE_STREAM_INFO;

	public int Length
	{
		get
		{
			int num = 0;
			for (int i = 0; i < m_entries.Count; i++)
			{
				int length = m_entries[i].Length;
				num += length;
				if (i < m_entries.Count - 1)
				{
					int num2 = (8 - length % 8) % 8;
					num += num2;
				}
			}
			return num;
		}
	}

	public QueryFileStreamInfo()
	{
	}

	public QueryFileStreamInfo(byte[] buffer, int offset)
	{
		if (offset < buffer.Length)
		{
			FileStreamEntry fileStreamEntry;
			do
			{
				fileStreamEntry = new FileStreamEntry(buffer, offset);
				m_entries.Add(fileStreamEntry);
				offset += (int)fileStreamEntry.NextEntryOffset;
			}
			while (fileStreamEntry.NextEntryOffset != 0);
		}
	}

	public override byte[] GetBytes()
	{
		byte[] array = new byte[Length];
		int num = 0;
		for (int i = 0; i < m_entries.Count; i++)
		{
			FileStreamEntry fileStreamEntry = m_entries[i];
			fileStreamEntry.WriteBytes(array, num);
			int length = fileStreamEntry.Length;
			num += length;
			if (i < m_entries.Count - 1)
			{
				int num2 = (8 - length % 8) % 8;
				num += num2;
			}
		}
		return array;
	}
}
