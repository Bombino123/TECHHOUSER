using System;
using dnlib.IO;

namespace dnlib.DotNet;

public class EmbeddedResource : Resource
{
	private readonly DataReaderFactory dataReaderFactory;

	private readonly uint resourceStartOffset;

	private readonly uint resourceLength;

	public uint Length => resourceLength;

	public override ResourceType ResourceType => ResourceType.Embedded;

	public EmbeddedResource(UTF8String name, byte[] data, ManifestResourceAttributes flags = ManifestResourceAttributes.Private)
		: this(name, ByteArrayDataReaderFactory.Create(data, null), 0u, (uint)data.Length, flags)
	{
	}

	public EmbeddedResource(UTF8String name, DataReaderFactory dataReaderFactory, uint offset, uint length, ManifestResourceAttributes flags = ManifestResourceAttributes.Private)
		: base(name, flags)
	{
		this.dataReaderFactory = dataReaderFactory ?? throw new ArgumentNullException("dataReaderFactory");
		resourceStartOffset = offset;
		resourceLength = length;
	}

	public DataReader CreateReader()
	{
		return dataReaderFactory.CreateReader(resourceStartOffset, resourceLength);
	}

	public override string ToString()
	{
		return $"{UTF8String.ToSystemStringOrEmpty(base.Name)} - size: {resourceLength}";
	}
}
