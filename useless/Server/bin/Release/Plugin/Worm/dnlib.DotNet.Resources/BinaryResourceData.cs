using System;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace dnlib.DotNet.Resources;

[ComVisible(true)]
public sealed class BinaryResourceData : UserResourceData
{
	private byte[] data;

	private SerializationFormat format;

	public byte[] Data => data;

	public SerializationFormat Format => format;

	public BinaryResourceData(UserResourceType type, byte[] data, SerializationFormat format)
		: base(type)
	{
		this.data = data;
		this.format = format;
	}

	public override void WriteData(ResourceBinaryWriter writer, IFormatter formatter)
	{
		if (writer.ReaderType == ResourceReaderType.ResourceReader && format != SerializationFormat.BinaryFormatter)
		{
			throw new NotSupportedException($"Unsupported serialization format: {format} for {writer.ReaderType}");
		}
		if (writer.ReaderType == ResourceReaderType.DeserializingResourceReader)
		{
			writer.Write7BitEncodedInt((int)format);
			writer.Write7BitEncodedInt(data.Length);
		}
		writer.Write(data);
	}

	public override string ToString()
	{
		return $"Binary: Length: {data.Length} Format: {format}";
	}
}
