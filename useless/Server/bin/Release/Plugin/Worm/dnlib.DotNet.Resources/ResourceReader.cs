using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using dnlib.IO;

namespace dnlib.DotNet.Resources;

[ComVisible(true)]
public struct ResourceReader
{
	private sealed class ResourceInfo
	{
		public readonly string name;

		public readonly long offset;

		public ResourceInfo(string name, long offset)
		{
			this.name = name;
			this.offset = offset;
		}

		public override string ToString()
		{
			return $"{offset:X8} - {name}";
		}
	}

	private DataReader reader;

	private readonly uint baseFileOffset;

	private readonly ResourceDataFactory resourceDataFactory;

	private readonly CreateResourceDataDelegate createResourceDataDelegate;

	private ResourceReader(ResourceDataFactory resourceDataFactory, ref DataReader reader, CreateResourceDataDelegate createResourceDataDelegate)
	{
		this.reader = reader;
		this.resourceDataFactory = resourceDataFactory;
		this.createResourceDataDelegate = createResourceDataDelegate;
		baseFileOffset = reader.StartOffset;
	}

	public static bool CouldBeResourcesFile(DataReader reader)
	{
		if (reader.CanRead(4u))
		{
			return reader.ReadUInt32() == 3203386062u;
		}
		return false;
	}

	public static ResourceElementSet Read(ModuleDef module, DataReader reader)
	{
		return Read(module, reader, null);
	}

	public static ResourceElementSet Read(ModuleDef module, DataReader reader, CreateResourceDataDelegate createResourceDataDelegate)
	{
		return Read(new ResourceDataFactory(module), reader, createResourceDataDelegate);
	}

	public static ResourceElementSet Read(ResourceDataFactory resourceDataFactory, DataReader reader, CreateResourceDataDelegate createResourceDataDelegate)
	{
		return new ResourceReader(resourceDataFactory, ref reader, createResourceDataDelegate).Read();
	}

	private ResourceElementSet Read()
	{
		uint num = reader.ReadUInt32();
		if (num != 3203386062u)
		{
			throw new ResourceReaderException($"Invalid resource sig: {num:X8}");
		}
		ResourceElementSet resourceElementSet = ReadHeader();
		if (resourceElementSet == null)
		{
			throw new ResourceReaderException("Invalid resource reader");
		}
		resourceElementSet.FormatVersion = reader.ReadInt32();
		if (resourceElementSet.FormatVersion != 2 && resourceElementSet.FormatVersion != 1)
		{
			throw new ResourceReaderException($"Invalid resource version: {resourceElementSet.FormatVersion}");
		}
		int num2 = reader.ReadInt32();
		if (num2 < 0)
		{
			throw new ResourceReaderException($"Invalid number of resources: {num2}");
		}
		int num3 = reader.ReadInt32();
		if (num3 < 0)
		{
			throw new ResourceReaderException($"Invalid number of user types: {num3}");
		}
		List<UserResourceType> list = new List<UserResourceType>();
		for (int i = 0; i < num3; i++)
		{
			list.Add(new UserResourceType(reader.ReadSerializedString(), (ResourceTypeCode)(64 + i)));
		}
		reader.Position = (reader.Position + 7) & 0xFFFFFFF8u;
		int[] array = new int[num2];
		for (int j = 0; j < num2; j++)
		{
			array[j] = reader.ReadInt32();
		}
		int[] array2 = new int[num2];
		for (int k = 0; k < num2; k++)
		{
			array2[k] = reader.ReadInt32();
		}
		_ = reader.Position;
		long num4 = reader.ReadInt32();
		long num5 = reader.Position;
		long num6 = reader.Length;
		List<ResourceInfo> list2 = new List<ResourceInfo>(num2);
		for (int l = 0; l < num2; l++)
		{
			reader.Position = (uint)(num5 + array2[l]);
			string name = reader.ReadSerializedString(Encoding.Unicode);
			long offset = num4 + reader.ReadInt32();
			list2.Add(new ResourceInfo(name, offset));
		}
		list2.Sort(delegate(ResourceInfo a, ResourceInfo b)
		{
			long offset2 = a.offset;
			return offset2.CompareTo(b.offset);
		});
		for (int m = 0; m < list2.Count; m++)
		{
			ResourceInfo resourceInfo = list2[m];
			ResourceElement resourceElement = new ResourceElement();
			resourceElement.Name = resourceInfo.name;
			reader.Position = (uint)resourceInfo.offset;
			int size = (int)(((m == list2.Count - 1) ? num6 : list2[m + 1].offset) - resourceInfo.offset);
			resourceElement.ResourceData = ((resourceElementSet.FormatVersion == 1) ? ReadResourceDataV1(list, resourceElementSet.ReaderType, size) : ReadResourceDataV2(list, resourceElementSet.ReaderType, size));
			resourceElement.ResourceData.StartOffset = (FileOffset)(baseFileOffset + (uint)(int)resourceInfo.offset);
			resourceElement.ResourceData.EndOffset = (FileOffset)(baseFileOffset + reader.Position);
			resourceElementSet.Add(resourceElement);
		}
		return resourceElementSet;
	}

	private IResourceData ReadResourceDataV2(List<UserResourceType> userTypes, ResourceReaderType readerType, int size)
	{
		uint endPos = reader.Position + (uint)size;
		uint num = reader.Read7BitEncodedUInt32();
		switch ((ResourceTypeCode)num)
		{
		case ResourceTypeCode.Null:
			return resourceDataFactory.CreateNull();
		case ResourceTypeCode.String:
			return resourceDataFactory.Create(reader.ReadSerializedString());
		case ResourceTypeCode.Boolean:
			return resourceDataFactory.Create(reader.ReadBoolean());
		case ResourceTypeCode.Char:
			return resourceDataFactory.Create(reader.ReadChar());
		case ResourceTypeCode.Byte:
			return resourceDataFactory.Create(reader.ReadByte());
		case ResourceTypeCode.SByte:
			return resourceDataFactory.Create(reader.ReadSByte());
		case ResourceTypeCode.Int16:
			return resourceDataFactory.Create(reader.ReadInt16());
		case ResourceTypeCode.UInt16:
			return resourceDataFactory.Create(reader.ReadUInt16());
		case ResourceTypeCode.Int32:
			return resourceDataFactory.Create(reader.ReadInt32());
		case ResourceTypeCode.UInt32:
			return resourceDataFactory.Create(reader.ReadUInt32());
		case ResourceTypeCode.Int64:
			return resourceDataFactory.Create(reader.ReadInt64());
		case ResourceTypeCode.UInt64:
			return resourceDataFactory.Create(reader.ReadUInt64());
		case ResourceTypeCode.Single:
			return resourceDataFactory.Create(reader.ReadSingle());
		case ResourceTypeCode.Double:
			return resourceDataFactory.Create(reader.ReadDouble());
		case ResourceTypeCode.Decimal:
			return resourceDataFactory.Create(reader.ReadDecimal());
		case ResourceTypeCode.DateTime:
			return resourceDataFactory.Create(DateTime.FromBinary(reader.ReadInt64()));
		case ResourceTypeCode.TimeSpan:
			return resourceDataFactory.Create(new TimeSpan(reader.ReadInt64()));
		case ResourceTypeCode.ByteArray:
			return resourceDataFactory.Create(reader.ReadBytes(reader.ReadInt32()));
		case ResourceTypeCode.Stream:
			return resourceDataFactory.CreateStream(reader.ReadBytes(reader.ReadInt32()));
		default:
		{
			int num2 = (int)(num - 64);
			if (num2 < 0 || num2 >= userTypes.Count)
			{
				throw new ResourceReaderException($"Invalid resource data code: {num}");
			}
			return ReadSerializedObject(endPos, readerType, userTypes[num2]);
		}
		}
	}

	private IResourceData ReadResourceDataV1(List<UserResourceType> userTypes, ResourceReaderType readerType, int size)
	{
		uint endPos = reader.Position + (uint)size;
		int num = reader.Read7BitEncodedInt32();
		if (num == -1)
		{
			return resourceDataFactory.CreateNull();
		}
		if (num < 0 || num >= userTypes.Count)
		{
			throw new ResourceReaderException($"Invalid resource type index: {num}");
		}
		UserResourceType userResourceType = userTypes[num];
		int num2 = userResourceType.Name.IndexOf(',');
		return ((num2 == -1) ? userResourceType.Name : userResourceType.Name.Remove(num2)) switch
		{
			"System.String" => resourceDataFactory.Create(reader.ReadSerializedString()), 
			"System.Int32" => resourceDataFactory.Create(reader.ReadInt32()), 
			"System.Byte" => resourceDataFactory.Create(reader.ReadByte()), 
			"System.SByte" => resourceDataFactory.Create(reader.ReadSByte()), 
			"System.Int16" => resourceDataFactory.Create(reader.ReadInt16()), 
			"System.Int64" => resourceDataFactory.Create(reader.ReadInt64()), 
			"System.UInt16" => resourceDataFactory.Create(reader.ReadUInt16()), 
			"System.UInt32" => resourceDataFactory.Create(reader.ReadUInt32()), 
			"System.UInt64" => resourceDataFactory.Create(reader.ReadUInt64()), 
			"System.Single" => resourceDataFactory.Create(reader.ReadSingle()), 
			"System.Double" => resourceDataFactory.Create(reader.ReadDouble()), 
			"System.DateTime" => resourceDataFactory.Create(new DateTime(reader.ReadInt64())), 
			"System.TimeSpan" => resourceDataFactory.Create(new TimeSpan(reader.ReadInt64())), 
			"System.Decimal" => resourceDataFactory.Create(reader.ReadDecimal()), 
			_ => ReadSerializedObject(endPos, readerType, userResourceType), 
		};
	}

	private IResourceData ReadSerializedObject(uint endPos, ResourceReaderType readerType, UserResourceType type)
	{
		switch (readerType)
		{
		case ResourceReaderType.ResourceReader:
		{
			byte[] array = reader.ReadBytes((int)(endPos - reader.Position));
			IResourceData resourceData = createResourceDataDelegate?.Invoke(resourceDataFactory, type, array, SerializationFormat.BinaryFormatter);
			return resourceData ?? resourceDataFactory.CreateSerialized(array, SerializationFormat.BinaryFormatter, type);
		}
		case ResourceReaderType.DeserializingResourceReader:
		{
			SerializationFormat serializationFormat = (SerializationFormat)reader.Read7BitEncodedInt32();
			if (serializationFormat < SerializationFormat.BinaryFormatter || serializationFormat > SerializationFormat.ActivatorStream)
			{
				throw new ResourceReaderException($"Invalid serialization format: {serializationFormat}");
			}
			int length = reader.Read7BitEncodedInt32();
			byte[] array = reader.ReadBytes(length);
			IResourceData resourceData = createResourceDataDelegate?.Invoke(resourceDataFactory, type, array, serializationFormat);
			return resourceData ?? resourceDataFactory.CreateSerialized(array, serializationFormat, type);
		}
		default:
			throw new ResourceReaderException($"Invalid reader type: {readerType}");
		}
	}

	private ResourceElementSet ReadHeader()
	{
		int num = reader.ReadInt32();
		if (num != 1)
		{
			throw new ResourceReaderException($"Invalid or unsupported header version: {num}");
		}
		int num2 = reader.ReadInt32();
		if (num2 < 0)
		{
			throw new ResourceReaderException($"Invalid header size: {num2:X8}");
		}
		string text = reader.ReadSerializedString();
		string resourceSetTypeName = reader.ReadSerializedString();
		ResourceReaderType readerType;
		if (Regex.IsMatch(text, "^System\\.Resources\\.ResourceReader,\\s*mscorlib"))
		{
			readerType = ResourceReaderType.ResourceReader;
		}
		else
		{
			if (!Regex.IsMatch(text, "^System\\.Resources\\.Extensions\\.DeserializingResourceReader,\\s*System\\.Resources\\.Extensions"))
			{
				return null;
			}
			readerType = ResourceReaderType.DeserializingResourceReader;
		}
		return new ResourceElementSet(text, resourceSetTypeName, readerType);
	}
}
