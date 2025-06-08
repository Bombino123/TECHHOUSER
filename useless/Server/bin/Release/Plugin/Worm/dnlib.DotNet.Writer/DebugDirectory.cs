using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using dnlib.IO;
using dnlib.PE;

namespace dnlib.DotNet.Writer;

[ComVisible(true)]
public sealed class DebugDirectory : IReuseChunk, IChunk
{
	public const uint DEFAULT_DEBUGDIRECTORY_ALIGNMENT = 4u;

	internal const int HEADER_SIZE = 28;

	private FileOffset offset;

	private RVA rva;

	private uint length;

	private readonly List<DebugDirectoryEntry> entries;

	private bool isReadonly;

	internal int Count => entries.Count;

	public FileOffset FileOffset => offset;

	public RVA RVA => rva;

	public DebugDirectory()
	{
		entries = new List<DebugDirectoryEntry>();
	}

	public DebugDirectoryEntry Add(byte[] data)
	{
		return Add(new ByteArrayChunk(data));
	}

	public DebugDirectoryEntry Add(IChunk chunk)
	{
		if (isReadonly)
		{
			throw new InvalidOperationException("Can't add a new DebugDirectory entry when the DebugDirectory is read-only!");
		}
		DebugDirectoryEntry debugDirectoryEntry = new DebugDirectoryEntry(chunk);
		entries.Add(debugDirectoryEntry);
		return debugDirectoryEntry;
	}

	public DebugDirectoryEntry Add(byte[] data, ImageDebugType type, ushort majorVersion, ushort minorVersion, uint timeDateStamp)
	{
		return Add(new ByteArrayChunk(data), type, majorVersion, minorVersion, timeDateStamp);
	}

	public DebugDirectoryEntry Add(IChunk chunk, ImageDebugType type, ushort majorVersion, ushort minorVersion, uint timeDateStamp)
	{
		DebugDirectoryEntry debugDirectoryEntry = Add(chunk);
		debugDirectoryEntry.DebugDirectory.Type = type;
		debugDirectoryEntry.DebugDirectory.MajorVersion = majorVersion;
		debugDirectoryEntry.DebugDirectory.MinorVersion = minorVersion;
		debugDirectoryEntry.DebugDirectory.TimeDateStamp = timeDateStamp;
		return debugDirectoryEntry;
	}

	bool IReuseChunk.CanReuse(RVA origRva, uint origSize)
	{
		if (GetLength(entries, (FileOffset)origRva, origRva) > origSize)
		{
			return false;
		}
		isReadonly = true;
		return true;
	}

	public void SetOffset(FileOffset offset, RVA rva)
	{
		isReadonly = true;
		this.offset = offset;
		this.rva = rva;
		length = GetLength(entries, offset, rva);
	}

	private static uint GetLength(List<DebugDirectoryEntry> entries, FileOffset offset, RVA rva)
	{
		uint num = (uint)(28 * entries.Count);
		foreach (DebugDirectoryEntry entry in entries)
		{
			num = Utils.AlignUp(num, 4u);
			entry.Chunk.SetOffset(offset + num, rva + num);
			num += entry.Chunk.GetFileLength();
		}
		return num;
	}

	public uint GetFileLength()
	{
		return length;
	}

	public uint GetVirtualSize()
	{
		return GetFileLength();
	}

	public uint CalculateAlignment()
	{
		return 0u;
	}

	public void WriteTo(DataWriter writer)
	{
		uint offs = 0u;
		foreach (DebugDirectoryEntry entry in entries)
		{
			writer.WriteUInt32(entry.DebugDirectory.Characteristics);
			writer.WriteUInt32(entry.DebugDirectory.TimeDateStamp);
			writer.WriteUInt16(entry.DebugDirectory.MajorVersion);
			writer.WriteUInt16(entry.DebugDirectory.MinorVersion);
			writer.WriteUInt32((uint)entry.DebugDirectory.Type);
			uint fileLength = entry.Chunk.GetFileLength();
			writer.WriteUInt32(fileLength);
			writer.WriteUInt32((uint)((fileLength != 0) ? entry.Chunk.RVA : ((RVA)0u)));
			writer.WriteUInt32((uint)((fileLength != 0) ? entry.Chunk.FileOffset : ((FileOffset)0u)));
			offs += 28;
		}
		foreach (DebugDirectoryEntry entry2 in entries)
		{
			WriteAlign(writer, ref offs);
			entry2.Chunk.VerifyWriteTo(writer);
			offs += entry2.Chunk.GetFileLength();
		}
	}

	private static void WriteAlign(DataWriter writer, ref uint offs)
	{
		uint num = Utils.AlignUp(offs, 4u) - offs;
		offs += num;
		writer.WriteZeroes((int)num);
	}
}
