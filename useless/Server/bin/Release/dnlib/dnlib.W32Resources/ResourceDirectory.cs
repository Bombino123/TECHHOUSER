using System.Collections.Generic;
using dnlib.Utils;

namespace dnlib.W32Resources;

public abstract class ResourceDirectory : ResourceDirectoryEntry
{
	protected uint characteristics;

	protected uint timeDateStamp;

	protected ushort majorVersion;

	protected ushort minorVersion;

	private protected LazyList<ResourceDirectory> directories;

	private protected LazyList<ResourceData> data;

	public uint Characteristics
	{
		get
		{
			return characteristics;
		}
		set
		{
			characteristics = value;
		}
	}

	public uint TimeDateStamp
	{
		get
		{
			return timeDateStamp;
		}
		set
		{
			timeDateStamp = value;
		}
	}

	public ushort MajorVersion
	{
		get
		{
			return majorVersion;
		}
		set
		{
			majorVersion = value;
		}
	}

	public ushort MinorVersion
	{
		get
		{
			return minorVersion;
		}
		set
		{
			minorVersion = value;
		}
	}

	public IList<ResourceDirectory> Directories => directories;

	public IList<ResourceData> Data => data;

	protected ResourceDirectory(ResourceName name)
		: base(name)
	{
	}

	public ResourceDirectory FindDirectory(ResourceName name)
	{
		foreach (ResourceDirectory directory in directories)
		{
			if (directory.Name == name)
			{
				return directory;
			}
		}
		return null;
	}

	public ResourceData FindData(ResourceName name)
	{
		foreach (ResourceData datum in data)
		{
			if (datum.Name == name)
			{
				return datum;
			}
		}
		return null;
	}
}
