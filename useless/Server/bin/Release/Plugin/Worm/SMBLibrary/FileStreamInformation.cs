using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace SMBLibrary;

[ComVisible(true)]
public class FileStreamInformation : FileInformation
{
	private List<FileStreamEntry> m_entries = new List<FileStreamEntry>();

	public List<FileStreamEntry> Entries => m_entries;

	public override FileInformationClass FileInformationClass => FileInformationClass.FileStreamInformation;

	public override int Length
	{
		get
		{
			int num = 0;
			for (int i = 0; i < m_entries.Count; i++)
			{
				FileStreamEntry fileStreamEntry = m_entries[i];
				int num2 = ((i < m_entries.Count - 1) ? fileStreamEntry.PaddedLength : fileStreamEntry.Length);
				num += num2;
			}
			return num;
		}
	}

	public FileStreamInformation()
	{
	}

	public FileStreamInformation(byte[] buffer, int offset)
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

	public override void WriteBytes(byte[] buffer, int offset)
	{
		for (int i = 0; i < m_entries.Count; i++)
		{
			FileStreamEntry fileStreamEntry = m_entries[i];
			int paddedLength = fileStreamEntry.PaddedLength;
			fileStreamEntry.NextEntryOffset = ((i < m_entries.Count - 1) ? ((uint)paddedLength) : 0u);
			fileStreamEntry.WriteBytes(buffer, offset);
			offset += paddedLength;
		}
	}
}
