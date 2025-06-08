using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using dnlib.IO;
using dnlib.PE;
using dnlib.W32Resources;

namespace dnlib.DotNet.Writer;

[ComVisible(true)]
public sealed class Win32ResourcesChunk : IReuseChunk, IChunk
{
	private readonly Win32Resources win32Resources;

	private FileOffset offset;

	private RVA rva;

	private uint length;

	private readonly Dictionary<ResourceDirectory, uint> dirDict = new Dictionary<ResourceDirectory, uint>();

	private readonly List<ResourceDirectory> dirList = new List<ResourceDirectory>();

	private readonly Dictionary<ResourceData, uint> dataHeaderDict = new Dictionary<ResourceData, uint>();

	private readonly List<ResourceData> dataHeaderList = new List<ResourceData>();

	private readonly Dictionary<string, uint> stringsDict = new Dictionary<string, uint>(StringComparer.Ordinal);

	private readonly List<string> stringsList = new List<string>();

	private readonly Dictionary<ResourceData, uint> dataDict = new Dictionary<ResourceData, uint>();

	private readonly List<ResourceData> dataList = new List<ResourceData>();

	private const uint RESOURCE_DIR_ALIGNMENT = 4u;

	private const uint RESOURCE_DATA_HEADER_ALIGNMENT = 4u;

	private const uint RESOURCE_STRING_ALIGNMENT = 2u;

	private const uint RESOURCE_DATA_ALIGNMENT = 4u;

	public FileOffset FileOffset => offset;

	public RVA RVA => rva;

	public Win32ResourcesChunk(Win32Resources win32Resources)
	{
		this.win32Resources = win32Resources;
	}

	public bool GetFileOffsetAndRvaOf(ResourceDirectoryEntry dirEntry, out FileOffset fileOffset, out RVA rva)
	{
		if (dirEntry is ResourceDirectory dir)
		{
			return GetFileOffsetAndRvaOf(dir, out fileOffset, out rva);
		}
		if (dirEntry is ResourceData dataHeader)
		{
			return GetFileOffsetAndRvaOf(dataHeader, out fileOffset, out rva);
		}
		fileOffset = (FileOffset)0u;
		rva = (RVA)0u;
		return false;
	}

	public FileOffset GetFileOffset(ResourceDirectoryEntry dirEntry)
	{
		GetFileOffsetAndRvaOf(dirEntry, out var fileOffset, out var _);
		return fileOffset;
	}

	public RVA GetRVA(ResourceDirectoryEntry dirEntry)
	{
		GetFileOffsetAndRvaOf(dirEntry, out var _, out var result);
		return result;
	}

	public bool GetFileOffsetAndRvaOf(ResourceDirectory dir, out FileOffset fileOffset, out RVA rva)
	{
		if (dir == null || !dirDict.TryGetValue(dir, out var value))
		{
			fileOffset = (FileOffset)0u;
			rva = (RVA)0u;
			return false;
		}
		fileOffset = offset + value;
		rva = this.rva + value;
		return true;
	}

	public FileOffset GetFileOffset(ResourceDirectory dir)
	{
		GetFileOffsetAndRvaOf(dir, out var fileOffset, out var _);
		return fileOffset;
	}

	public RVA GetRVA(ResourceDirectory dir)
	{
		GetFileOffsetAndRvaOf(dir, out var _, out var result);
		return result;
	}

	public bool GetFileOffsetAndRvaOf(ResourceData dataHeader, out FileOffset fileOffset, out RVA rva)
	{
		if (dataHeader == null || !dataHeaderDict.TryGetValue(dataHeader, out var value))
		{
			fileOffset = (FileOffset)0u;
			rva = (RVA)0u;
			return false;
		}
		fileOffset = offset + value;
		rva = this.rva + value;
		return true;
	}

	public FileOffset GetFileOffset(ResourceData dataHeader)
	{
		GetFileOffsetAndRvaOf(dataHeader, out var fileOffset, out var _);
		return fileOffset;
	}

	public RVA GetRVA(ResourceData dataHeader)
	{
		GetFileOffsetAndRvaOf(dataHeader, out var _, out var result);
		return result;
	}

	public bool GetFileOffsetAndRvaOf(string name, out FileOffset fileOffset, out RVA rva)
	{
		if (name == null || !stringsDict.TryGetValue(name, out var value))
		{
			fileOffset = (FileOffset)0u;
			rva = (RVA)0u;
			return false;
		}
		fileOffset = offset + value;
		rva = this.rva + value;
		return true;
	}

	public FileOffset GetFileOffset(string name)
	{
		GetFileOffsetAndRvaOf(name, out var fileOffset, out var _);
		return fileOffset;
	}

	public RVA GetRVA(string name)
	{
		GetFileOffsetAndRvaOf(name, out var _, out var result);
		return result;
	}

	bool IReuseChunk.CanReuse(RVA origRva, uint origSize)
	{
		if (rva == (RVA)0u)
		{
			throw new InvalidOperationException();
		}
		return length <= origSize;
	}

	internal bool CheckValidOffset(FileOffset offset)
	{
		GetMaxAlignment(offset, out var error);
		return error == null;
	}

	private static uint GetMaxAlignment(FileOffset offset, out string error)
	{
		error = null;
		uint val = 1u;
		val = Math.Max(val, 4u);
		val = Math.Max(val, 4u);
		val = Math.Max(val, 2u);
		val = Math.Max(val, 4u);
		if (((uint)offset & (val - 1)) != 0)
		{
			error = $"Win32 resources section isn't {val}-byte aligned";
		}
		else if (val > 8)
		{
			error = "maxAlignment > DEFAULT_WIN32_RESOURCES_ALIGNMENT";
		}
		return val;
	}

	public void SetOffset(FileOffset offset, RVA rva)
	{
		bool flag = this.offset == (FileOffset)0u;
		this.offset = offset;
		this.rva = rva;
		if (win32Resources == null)
		{
			return;
		}
		if (!flag)
		{
			dirDict.Clear();
			dirList.Clear();
			dataHeaderDict.Clear();
			dataHeaderList.Clear();
			stringsDict.Clear();
			stringsList.Clear();
			dataDict.Clear();
			dataList.Clear();
		}
		FindDirectoryEntries();
		uint num = 0u;
		GetMaxAlignment(offset, out var error);
		if (error != null)
		{
			throw new ModuleWriterException(error);
		}
		foreach (ResourceDirectory dir in dirList)
		{
			num = Utils.AlignUp(num, 4u);
			dirDict[dir] = num;
			if (dir != dirList[0])
			{
				AddString(dir.Name);
			}
			num += (uint)(16 + (dir.Directories.Count + dir.Data.Count) * 8);
		}
		foreach (ResourceData dataHeader in dataHeaderList)
		{
			num = Utils.AlignUp(num, 4u);
			dataHeaderDict[dataHeader] = num;
			AddString(dataHeader.Name);
			AddData(dataHeader);
			num += 16;
		}
		foreach (string strings in stringsList)
		{
			num = Utils.AlignUp(num, 2u);
			stringsDict[strings] = num;
			num += (uint)(2 + strings.Length * 2);
		}
		foreach (ResourceData data in dataList)
		{
			num = Utils.AlignUp(num, 4u);
			dataDict[data] = num;
			num += data.CreateReader().Length;
		}
		length = num;
	}

	private void AddData(ResourceData data)
	{
		if (!dataDict.ContainsKey(data))
		{
			dataList.Add(data);
			dataDict.Add(data, 0u);
		}
	}

	private void AddString(ResourceName name)
	{
		if (name.HasName && !stringsDict.ContainsKey(name.Name))
		{
			stringsList.Add(name.Name);
			stringsDict.Add(name.Name, 0u);
		}
	}

	private void FindDirectoryEntries()
	{
		FindDirectoryEntries(win32Resources.Root);
	}

	private void FindDirectoryEntries(ResourceDirectory dir)
	{
		if (dirDict.ContainsKey(dir))
		{
			return;
		}
		dirList.Add(dir);
		dirDict[dir] = 0u;
		IList<ResourceDirectory> directories = dir.Directories;
		int count = directories.Count;
		for (int i = 0; i < count; i++)
		{
			FindDirectoryEntries(directories[i]);
		}
		IList<ResourceData> data = dir.Data;
		count = data.Count;
		for (int j = 0; j < count; j++)
		{
			ResourceData resourceData = data[j];
			if (!dataHeaderDict.ContainsKey(resourceData))
			{
				dataHeaderList.Add(resourceData);
				dataHeaderDict[resourceData] = 0u;
			}
		}
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
		uint num = 0u;
		foreach (ResourceDirectory dir in dirList)
		{
			uint num2 = Utils.AlignUp(num, 4u) - num;
			writer.WriteZeroes((int)num2);
			num += num2;
			if (dirDict[dir] != num)
			{
				throw new ModuleWriterException("Invalid Win32 resource directory offset");
			}
			num += WriteTo(writer, dir);
		}
		foreach (ResourceData dataHeader in dataHeaderList)
		{
			uint num3 = Utils.AlignUp(num, 4u) - num;
			writer.WriteZeroes((int)num3);
			num += num3;
			if (dataHeaderDict[dataHeader] != num)
			{
				throw new ModuleWriterException("Invalid Win32 resource data header offset");
			}
			num += WriteTo(writer, dataHeader);
		}
		foreach (string strings in stringsList)
		{
			uint num4 = Utils.AlignUp(num, 2u) - num;
			writer.WriteZeroes((int)num4);
			num += num4;
			if (stringsDict[strings] != num)
			{
				throw new ModuleWriterException("Invalid Win32 resource string offset");
			}
			byte[] bytes = Encoding.Unicode.GetBytes(strings);
			if (bytes.Length / 2 > 65535)
			{
				throw new ModuleWriterException("Win32 resource entry name is too long");
			}
			writer.WriteUInt16((ushort)(bytes.Length / 2));
			writer.WriteBytes(bytes);
			num += (uint)(2 + bytes.Length);
		}
		byte[] dataBuffer = new byte[8192];
		foreach (ResourceData data in dataList)
		{
			uint num5 = Utils.AlignUp(num, 4u) - num;
			writer.WriteZeroes((int)num5);
			num += num5;
			if (dataDict[data] != num)
			{
				throw new ModuleWriterException("Invalid Win32 resource data offset");
			}
			DataReader dataReader = data.CreateReader();
			num += dataReader.BytesLeft;
			dataReader.CopyTo(writer, dataBuffer);
		}
	}

	private uint WriteTo(DataWriter writer, ResourceDirectory dir)
	{
		writer.WriteUInt32(dir.Characteristics);
		writer.WriteUInt32(dir.TimeDateStamp);
		writer.WriteUInt16(dir.MajorVersion);
		writer.WriteUInt16(dir.MinorVersion);
		GetNamedAndIds(dir, out var named, out var ids);
		if (named.Count > 65535 || ids.Count > 65535)
		{
			throw new ModuleWriterException("Too many named/id Win32 resource entries");
		}
		writer.WriteUInt16((ushort)named.Count);
		writer.WriteUInt16((ushort)ids.Count);
		named.Sort((ResourceDirectoryEntry a, ResourceDirectoryEntry b) => a.Name.Name.ToUpperInvariant().CompareTo(b.Name.Name.ToUpperInvariant()));
		ids.Sort((ResourceDirectoryEntry a, ResourceDirectoryEntry b) => a.Name.Id.CompareTo(b.Name.Id));
		foreach (ResourceDirectoryEntry item in named)
		{
			writer.WriteUInt32(0x80000000u | stringsDict[item.Name.Name]);
			writer.WriteUInt32(GetDirectoryEntryOffset(item));
		}
		foreach (ResourceDirectoryEntry item2 in ids)
		{
			writer.WriteInt32(item2.Name.Id);
			writer.WriteUInt32(GetDirectoryEntryOffset(item2));
		}
		return (uint)(16 + (named.Count + ids.Count) * 8);
	}

	private uint GetDirectoryEntryOffset(ResourceDirectoryEntry e)
	{
		if (e is ResourceData)
		{
			return dataHeaderDict[(ResourceData)e];
		}
		return 0x80000000u | dirDict[(ResourceDirectory)e];
	}

	private static void GetNamedAndIds(ResourceDirectory dir, out List<ResourceDirectoryEntry> named, out List<ResourceDirectoryEntry> ids)
	{
		named = new List<ResourceDirectoryEntry>();
		ids = new List<ResourceDirectoryEntry>();
		IList<ResourceDirectory> directories = dir.Directories;
		int count = directories.Count;
		for (int i = 0; i < count; i++)
		{
			ResourceDirectory resourceDirectory = directories[i];
			if (resourceDirectory.Name.HasId)
			{
				ids.Add(resourceDirectory);
			}
			else
			{
				named.Add(resourceDirectory);
			}
		}
		IList<ResourceData> data = dir.Data;
		count = data.Count;
		for (int j = 0; j < count; j++)
		{
			ResourceData resourceData = data[j];
			if (resourceData.Name.HasId)
			{
				ids.Add(resourceData);
			}
			else
			{
				named.Add(resourceData);
			}
		}
	}

	private uint WriteTo(DataWriter writer, ResourceData dataHeader)
	{
		writer.WriteUInt32((uint)(rva + dataDict[dataHeader]));
		writer.WriteUInt32(dataHeader.CreateReader().Length);
		writer.WriteUInt32(dataHeader.CodePage);
		writer.WriteUInt32(dataHeader.Reserved);
		return 16u;
	}
}
