using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace dnlib.DotNet.Resources;

public sealed class ResourceWriter
{
	private ModuleDef module;

	private BinaryWriter writer;

	private ResourceElementSet resources;

	private ResourceDataFactory typeCreator;

	private Dictionary<IResourceData, UserResourceType> dataToNewType = new Dictionary<IResourceData, UserResourceType>();

	private ResourceWriter(ModuleDef module, ResourceDataFactory typeCreator, Stream stream, ResourceElementSet resources)
	{
		this.module = module;
		this.typeCreator = typeCreator;
		writer = new BinaryWriter(stream);
		this.resources = resources;
	}

	public static void Write(ModuleDef module, Stream stream, ResourceElementSet resources)
	{
		new ResourceWriter(module, new ResourceDataFactory(module), stream, resources).Write();
	}

	public static void Write(ModuleDef module, ResourceDataFactory typeCreator, Stream stream, ResourceElementSet resources)
	{
		new ResourceWriter(module, typeCreator, stream, resources).Write();
	}

	private void Write()
	{
		if (resources.FormatVersion != 1 && resources.FormatVersion != 2)
		{
			throw new ArgumentException($"Invalid format version: {resources.FormatVersion}", "resources");
		}
		InitializeUserTypes(resources.FormatVersion);
		writer.Write(3203386062u);
		writer.Write(1);
		WriteReaderType();
		writer.Write(resources.FormatVersion);
		writer.Write(resources.Count);
		writer.Write(typeCreator.Count);
		foreach (UserResourceType sortedType in typeCreator.GetSortedTypes())
		{
			writer.Write(sortedType.Name);
		}
		int num = 8 - ((int)writer.BaseStream.Position & 7);
		if (num != 8)
		{
			for (int i = 0; i < num; i++)
			{
				writer.Write((byte)88);
			}
		}
		MemoryStream memoryStream = new MemoryStream();
		BinaryWriter binaryWriter = new BinaryWriter(memoryStream, Encoding.Unicode);
		MemoryStream memoryStream2 = new MemoryStream();
		ResourceBinaryWriter resourceBinaryWriter = new ResourceBinaryWriter(memoryStream2)
		{
			FormatVersion = resources.FormatVersion,
			ReaderType = resources.ReaderType
		};
		int[] array = new int[resources.Count];
		int[] array2 = new int[resources.Count];
		BinaryFormatter formatter = new BinaryFormatter(null, new StreamingContext(StreamingContextStates.File | StreamingContextStates.Persistence));
		int num2 = 0;
		foreach (ResourceElement resourceElement in resources.ResourceElements)
		{
			array2[num2] = (int)binaryWriter.BaseStream.Position;
			array[num2] = (int)Hash(resourceElement.Name);
			num2++;
			binaryWriter.Write(resourceElement.Name);
			binaryWriter.Write((int)resourceBinaryWriter.BaseStream.Position);
			WriteData(resourceBinaryWriter, resourceElement, formatter);
		}
		Array.Sort(array, array2);
		int[] array3 = array;
		foreach (int value in array3)
		{
			writer.Write(value);
		}
		array3 = array2;
		foreach (int value2 in array3)
		{
			writer.Write(value2);
		}
		writer.Write((int)writer.BaseStream.Position + (int)memoryStream.Length + 4);
		writer.Write(memoryStream.ToArray());
		writer.Write(memoryStream2.ToArray());
	}

	private void WriteData(ResourceBinaryWriter writer, ResourceElement info, IFormatter formatter)
	{
		ResourceTypeCode resourceType = GetResourceType(info.ResourceData, writer.FormatVersion);
		writer.Write7BitEncodedInt((int)resourceType);
		info.ResourceData.WriteData(writer, formatter);
	}

	private ResourceTypeCode GetResourceType(IResourceData data, int formatVersion)
	{
		if (formatVersion == 1)
		{
			if (data.Code == ResourceTypeCode.Null)
			{
				return (ResourceTypeCode)(-1);
			}
			return dataToNewType[data].Code - 64;
		}
		if (data is BuiltInResourceData)
		{
			return data.Code;
		}
		return dataToNewType[data].Code;
	}

	private static uint Hash(string key)
	{
		uint num = 5381u;
		foreach (char c in key)
		{
			num = ((num << 5) + num) ^ c;
		}
		return num;
	}

	private void InitializeUserTypes(int formatVersion)
	{
		foreach (ResourceElement resourceElement in resources.ResourceElements)
		{
			UserResourceType userResourceType;
			if (formatVersion == 1 && resourceElement.ResourceData is BuiltInResourceData builtInResourceData)
			{
				userResourceType = typeCreator.CreateBuiltinResourceType(builtInResourceData.Code);
				if (userResourceType == null)
				{
					throw new NotSupportedException($"Unsupported resource type: {builtInResourceData.Code} in format version 1 resource");
				}
			}
			else
			{
				if (!(resourceElement.ResourceData is UserResourceData userResourceData))
				{
					continue;
				}
				userResourceType = typeCreator.CreateUserResourceType(userResourceData.TypeName);
			}
			dataToNewType[resourceElement.ResourceData] = userResourceType;
		}
	}

	private void WriteReaderType()
	{
		MemoryStream memoryStream = new MemoryStream();
		BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
		if (resources.ResourceReaderTypeName != null && resources.ResourceSetTypeName != null)
		{
			binaryWriter.Write(resources.ResourceReaderTypeName);
			binaryWriter.Write(resources.ResourceSetTypeName);
		}
		else
		{
			string mscorlibFullname = GetMscorlibFullname();
			binaryWriter.Write("System.Resources.ResourceReader, " + mscorlibFullname);
			binaryWriter.Write("System.Resources.RuntimeResourceSet");
		}
		writer.Write((int)memoryStream.Position);
		writer.Write(memoryStream.ToArray());
	}

	private string GetMscorlibFullname()
	{
		if (module.CorLibTypes.AssemblyRef.Name == "mscorlib")
		{
			return module.CorLibTypes.AssemblyRef.FullName;
		}
		return "mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089";
	}
}
