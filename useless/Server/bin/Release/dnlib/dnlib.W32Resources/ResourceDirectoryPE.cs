using System;
using System.Collections.Generic;
using dnlib.IO;
using dnlib.PE;
using dnlib.Utils;

namespace dnlib.W32Resources;

public sealed class ResourceDirectoryPE : ResourceDirectory
{
	private readonly struct EntryInfo
	{
		public readonly ResourceName name;

		public readonly uint offset;

		public EntryInfo(ResourceName name, uint offset)
		{
			this.name = name;
			this.offset = offset;
		}

		public override string ToString()
		{
			return $"{offset:X8} {name}";
		}
	}

	private const uint MAX_DIR_DEPTH = 10u;

	private readonly Win32ResourcesPE resources;

	private uint depth;

	private List<EntryInfo> dataInfos;

	private List<EntryInfo> dirInfos;

	public ResourceDirectoryPE(uint depth, ResourceName name, Win32ResourcesPE resources, ref DataReader reader)
		: base(name)
	{
		this.resources = resources;
		this.depth = depth;
		Initialize(ref reader);
	}

	private void Initialize(ref DataReader reader)
	{
		if (depth > 10 || !reader.CanRead(16u))
		{
			InitializeDefault();
			return;
		}
		characteristics = reader.ReadUInt32();
		timeDateStamp = reader.ReadUInt32();
		majorVersion = reader.ReadUInt16();
		minorVersion = reader.ReadUInt16();
		ushort num = reader.ReadUInt16();
		ushort num2 = reader.ReadUInt16();
		int num3 = num + num2;
		if (!reader.CanRead((uint)(num3 * 8)))
		{
			InitializeDefault();
			return;
		}
		dataInfos = new List<EntryInfo>();
		dirInfos = new List<EntryInfo>();
		uint num4 = reader.Position;
		int num5 = 0;
		while (num5 < num3)
		{
			reader.Position = num4;
			uint num6 = reader.ReadUInt32();
			uint num7 = reader.ReadUInt32();
			ResourceName resourceName = (((num6 & 0x80000000u) == 0) ? new ResourceName((int)num6) : new ResourceName(ReadString(ref reader, num6 & 0x7FFFFFFFu) ?? string.Empty));
			if ((num7 & 0x80000000u) == 0)
			{
				dataInfos.Add(new EntryInfo(resourceName, num7));
			}
			else
			{
				dirInfos.Add(new EntryInfo(resourceName, num7 & 0x7FFFFFFFu));
			}
			num5++;
			num4 += 8;
		}
		directories = new LazyList<ResourceDirectory, object>(dirInfos.Count, null, (object ctx, int i) => ReadResourceDirectory(i));
		data = new LazyList<ResourceData, object>(dataInfos.Count, null, (object ctx, int i) => ReadResourceData(i));
	}

	private static string ReadString(ref DataReader reader, uint offset)
	{
		reader.Position = offset;
		if (!reader.CanRead(2u))
		{
			return null;
		}
		int num = reader.ReadUInt16() * 2;
		if (!reader.CanRead((uint)num))
		{
			return null;
		}
		try
		{
			return reader.ReadUtf16String(num / 2);
		}
		catch
		{
			return null;
		}
	}

	private ResourceDirectory ReadResourceDirectory(int i)
	{
		EntryInfo entryInfo = dirInfos[i];
		DataReader reader = resources.GetResourceReader();
		reader.Position = Math.Min(reader.Length, entryInfo.offset);
		return new ResourceDirectoryPE(depth + 1, entryInfo.name, resources, ref reader);
	}

	private ResourceData ReadResourceData(int i)
	{
		EntryInfo entryInfo = dataInfos[i];
		DataReader resourceReader = resources.GetResourceReader();
		resourceReader.Position = Math.Min(resourceReader.Length, entryInfo.offset);
		if (resourceReader.CanRead(16u))
		{
			RVA rva = (RVA)resourceReader.ReadUInt32();
			uint size = resourceReader.ReadUInt32();
			uint codePage = resourceReader.ReadUInt32();
			uint reserved = resourceReader.ReadUInt32();
			resources.GetDataReaderInfo(rva, size, out var dataReaderFactory, out var dataOffset, out var dataLength);
			return new ResourceData(entryInfo.name, dataReaderFactory, dataOffset, dataLength, codePage, reserved);
		}
		return new ResourceData(entryInfo.name);
	}

	private void InitializeDefault()
	{
		directories = new LazyList<ResourceDirectory>();
		data = new LazyList<ResourceData>();
	}
}
