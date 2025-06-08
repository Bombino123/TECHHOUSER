using System;
using System.Collections.Generic;
using dnlib.IO;
using dnlib.PE;

namespace dnlib.DotNet.Writer;

public sealed class RelocDirectory : IChunk
{
	private readonly struct RelocInfo
	{
		public readonly IChunk Chunk;

		public readonly uint OffsetOrRva;

		public RelocInfo(IChunk chunk, uint offset)
		{
			Chunk = chunk;
			OffsetOrRva = offset;
		}
	}

	private readonly Machine machine;

	private readonly List<RelocInfo> allRelocRvas = new List<RelocInfo>();

	private readonly List<List<uint>> relocSections = new List<List<uint>>();

	private bool isReadOnly;

	private FileOffset offset;

	private RVA rva;

	private uint totalSize;

	public FileOffset FileOffset => offset;

	public RVA RVA => rva;

	internal bool NeedsRelocSection => allRelocRvas.Count != 0;

	public RelocDirectory(Machine machine)
	{
		this.machine = machine;
	}

	public void SetOffset(FileOffset offset, RVA rva)
	{
		isReadOnly = true;
		this.offset = offset;
		this.rva = rva;
		List<uint> list = new List<uint>(allRelocRvas.Count);
		foreach (RelocInfo allRelocRva in allRelocRvas)
		{
			uint item = ((allRelocRva.Chunk == null) ? allRelocRva.OffsetOrRva : ((uint)(allRelocRva.Chunk.RVA + allRelocRva.OffsetOrRva)));
			list.Add(item);
		}
		list.Sort();
		uint num = uint.MaxValue;
		List<uint> list2 = null;
		foreach (uint item2 in list)
		{
			uint num2 = item2 & 0xFFFFF000u;
			if (num2 != num)
			{
				num = num2;
				if (list2 != null)
				{
					totalSize += (uint)(8 + ((list2.Count + 1) & -2) * 2);
				}
				list2 = new List<uint>();
				relocSections.Add(list2);
			}
			list2.Add(item2);
		}
		if (list2 != null)
		{
			totalSize += (uint)(8 + ((list2.Count + 1) & -2) * 2);
		}
	}

	public uint GetFileLength()
	{
		return totalSize;
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
		uint num = (machine.Is64Bit() ? 40960u : 12288u);
		foreach (List<uint> relocSection in relocSections)
		{
			writer.WriteUInt32(relocSection[0] & 0xFFFFF000u);
			writer.WriteUInt32((uint)(8 + ((relocSection.Count + 1) & -2) * 2));
			foreach (uint item in relocSection)
			{
				writer.WriteUInt16((ushort)(num | (item & 0xFFFu)));
			}
			if (((uint)relocSection.Count & (true ? 1u : 0u)) != 0)
			{
				writer.WriteUInt16(0);
			}
		}
	}

	public void Add(RVA rva)
	{
		if (isReadOnly)
		{
			throw new InvalidOperationException("Can't add a relocation when the relocs section is read-only");
		}
		allRelocRvas.Add(new RelocInfo(null, (uint)rva));
	}

	public void Add(IChunk chunk, uint offset)
	{
		if (isReadOnly)
		{
			throw new InvalidOperationException("Can't add a relocation when the relocs section is read-only");
		}
		allRelocRvas.Add(new RelocInfo(chunk, offset));
	}
}
