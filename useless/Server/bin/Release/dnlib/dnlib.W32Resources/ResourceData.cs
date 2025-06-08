using System;
using dnlib.IO;

namespace dnlib.W32Resources;

public sealed class ResourceData : ResourceDirectoryEntry
{
	private readonly DataReaderFactory dataReaderFactory;

	private readonly uint resourceStartOffset;

	private readonly uint resourceLength;

	private uint codePage;

	private uint reserved;

	public uint CodePage
	{
		get
		{
			return codePage;
		}
		set
		{
			codePage = value;
		}
	}

	public uint Reserved
	{
		get
		{
			return reserved;
		}
		set
		{
			reserved = value;
		}
	}

	public DataReader CreateReader()
	{
		return dataReaderFactory.CreateReader(resourceStartOffset, resourceLength);
	}

	public ResourceData(ResourceName name)
		: this(name, ByteArrayDataReaderFactory.Create(Array2.Empty<byte>(), null), 0u, 0u)
	{
	}

	public ResourceData(ResourceName name, DataReaderFactory dataReaderFactory, uint offset, uint length)
		: this(name, dataReaderFactory, offset, length, 0u, 0u)
	{
	}

	public ResourceData(ResourceName name, DataReaderFactory dataReaderFactory, uint offset, uint length, uint codePage, uint reserved)
		: base(name)
	{
		this.dataReaderFactory = dataReaderFactory ?? throw new ArgumentNullException("dataReaderFactory");
		resourceStartOffset = offset;
		resourceLength = length;
		this.codePage = codePage;
		this.reserved = reserved;
	}
}
