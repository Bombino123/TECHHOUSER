using System.Collections.Generic;
using System.Runtime.InteropServices;
using Utilities;

namespace SMBLibrary.RPC;

[ComVisible(true)]
public class VersionsSupported
{
	public List<Version> Entries = new List<Version>();

	public int Count => Entries.Count;

	public int Length => 1 + Count * 2;

	public VersionsSupported()
	{
	}

	public VersionsSupported(byte[] buffer, int offset)
	{
		byte b = ByteReader.ReadByte(buffer, offset);
		Entries = new List<Version>();
		for (int i = 0; i < b; i++)
		{
			Version item = new Version(buffer, offset + 1 + i * 2);
			Entries.Add(item);
		}
	}

	public void WriteBytes(byte[] buffer, int offset)
	{
		ByteWriter.WriteByte(buffer, offset, (byte)Count);
		for (int i = 0; i < Entries.Count; i++)
		{
			Entries[i].WriteBytes(buffer, offset + 1 + i * 2);
		}
	}
}
