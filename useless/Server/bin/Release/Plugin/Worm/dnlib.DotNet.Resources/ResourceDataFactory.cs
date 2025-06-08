using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace dnlib.DotNet.Resources;

[ComVisible(true)]
public class ResourceDataFactory
{
	private sealed class MyBinder : SerializationBinder
	{
		public class OkException : Exception
		{
			public string AssemblyName { get; set; }

			public string TypeName { get; set; }
		}

		public override Type BindToType(string assemblyName, string typeName)
		{
			throw new OkException
			{
				AssemblyName = assemblyName,
				TypeName = typeName
			};
		}
	}

	private readonly ModuleDef module;

	private readonly ModuleDefMD moduleMD;

	private readonly Dictionary<string, UserResourceType> dict = new Dictionary<string, UserResourceType>(StringComparer.Ordinal);

	private readonly Dictionary<string, string> asmNameToAsmFullName = new Dictionary<string, string>(StringComparer.Ordinal);

	protected ModuleDef Module => module;

	public int Count => dict.Count;

	public ResourceDataFactory(ModuleDef module)
	{
		this.module = module;
		moduleMD = module as ModuleDefMD;
	}

	public BuiltInResourceData CreateNull()
	{
		return new BuiltInResourceData(ResourceTypeCode.Null, null);
	}

	public BuiltInResourceData Create(string value)
	{
		return new BuiltInResourceData(ResourceTypeCode.String, value);
	}

	public BuiltInResourceData Create(bool value)
	{
		return new BuiltInResourceData(ResourceTypeCode.Boolean, value);
	}

	public BuiltInResourceData Create(char value)
	{
		return new BuiltInResourceData(ResourceTypeCode.Char, value);
	}

	public BuiltInResourceData Create(byte value)
	{
		return new BuiltInResourceData(ResourceTypeCode.Byte, value);
	}

	public BuiltInResourceData Create(sbyte value)
	{
		return new BuiltInResourceData(ResourceTypeCode.SByte, value);
	}

	public BuiltInResourceData Create(short value)
	{
		return new BuiltInResourceData(ResourceTypeCode.Int16, value);
	}

	public BuiltInResourceData Create(ushort value)
	{
		return new BuiltInResourceData(ResourceTypeCode.UInt16, value);
	}

	public BuiltInResourceData Create(int value)
	{
		return new BuiltInResourceData(ResourceTypeCode.Int32, value);
	}

	public BuiltInResourceData Create(uint value)
	{
		return new BuiltInResourceData(ResourceTypeCode.UInt32, value);
	}

	public BuiltInResourceData Create(long value)
	{
		return new BuiltInResourceData(ResourceTypeCode.Int64, value);
	}

	public BuiltInResourceData Create(ulong value)
	{
		return new BuiltInResourceData(ResourceTypeCode.UInt64, value);
	}

	public BuiltInResourceData Create(float value)
	{
		return new BuiltInResourceData(ResourceTypeCode.Single, value);
	}

	public BuiltInResourceData Create(double value)
	{
		return new BuiltInResourceData(ResourceTypeCode.Double, value);
	}

	public BuiltInResourceData Create(decimal value)
	{
		return new BuiltInResourceData(ResourceTypeCode.Decimal, value);
	}

	public BuiltInResourceData Create(DateTime value)
	{
		return new BuiltInResourceData(ResourceTypeCode.DateTime, value);
	}

	public BuiltInResourceData Create(TimeSpan value)
	{
		return new BuiltInResourceData(ResourceTypeCode.TimeSpan, value);
	}

	public BuiltInResourceData Create(byte[] value)
	{
		return new BuiltInResourceData(ResourceTypeCode.ByteArray, value);
	}

	public BuiltInResourceData CreateStream(byte[] value)
	{
		return new BuiltInResourceData(ResourceTypeCode.Stream, value);
	}

	public BinaryResourceData CreateSerialized(byte[] value, SerializationFormat format, UserResourceType type)
	{
		return new BinaryResourceData(CreateUserResourceType(type.Name, useFullName: true), value, format);
	}

	public BinaryResourceData CreateBinaryFormatterSerialized(byte[] value)
	{
		if (!GetSerializedTypeAndAssemblyName(value, out var assemblyName, out var typeName))
		{
			throw new ApplicationException("Could not get serialized type name");
		}
		string fullName = typeName + ", " + assemblyName;
		return new BinaryResourceData(CreateUserResourceType(fullName), value, SerializationFormat.BinaryFormatter);
	}

	private bool GetSerializedTypeAndAssemblyName(byte[] value, out string assemblyName, out string typeName)
	{
		try
		{
			BinaryFormatter binaryFormatter = new BinaryFormatter();
			binaryFormatter.Binder = new MyBinder();
			binaryFormatter.Deserialize(new MemoryStream(value));
		}
		catch (MyBinder.OkException ex)
		{
			assemblyName = ex.AssemblyName;
			typeName = ex.TypeName;
			return true;
		}
		catch
		{
		}
		assemblyName = null;
		typeName = null;
		return false;
	}

	public UserResourceType CreateBuiltinResourceType(ResourceTypeCode typeCode)
	{
		string text = typeCode switch
		{
			ResourceTypeCode.String => "System.String", 
			ResourceTypeCode.Boolean => "System.Boolean", 
			ResourceTypeCode.Char => "System.Char", 
			ResourceTypeCode.Byte => "System.Byte", 
			ResourceTypeCode.SByte => "System.SByte", 
			ResourceTypeCode.Int16 => "System.Int16", 
			ResourceTypeCode.UInt16 => "System.UInt16", 
			ResourceTypeCode.Int32 => "System.Int32", 
			ResourceTypeCode.UInt32 => "System.UInt32", 
			ResourceTypeCode.Int64 => "System.Int64", 
			ResourceTypeCode.UInt64 => "System.UInt64", 
			ResourceTypeCode.Single => "System.Single", 
			ResourceTypeCode.Double => "System.Double", 
			ResourceTypeCode.Decimal => "System.Decimal", 
			ResourceTypeCode.DateTime => "System.DateTime", 
			ResourceTypeCode.TimeSpan => "System.TimeSpan", 
			_ => null, 
		};
		if (text == null)
		{
			return null;
		}
		return CreateUserResourceType(text + ", " + module.CorLibTypes.AssemblyRef.FullName, useFullName: true);
	}

	public UserResourceType CreateUserResourceType(string fullName)
	{
		return CreateUserResourceType(fullName, useFullName: false);
	}

	private UserResourceType CreateUserResourceType(string fullName, bool useFullName)
	{
		if (dict.TryGetValue(fullName, out var value))
		{
			return value;
		}
		string text = (useFullName ? fullName : GetRealTypeFullName(fullName));
		value = new UserResourceType(text, (ResourceTypeCode)(64 + dict.Count));
		dict[fullName] = value;
		dict[text] = value;
		return value;
	}

	private string GetRealTypeFullName(string fullName)
	{
		ITypeDefOrRef typeDefOrRef = TypeNameParser.ParseReflection(module, fullName, null);
		if (typeDefOrRef == null)
		{
			return fullName;
		}
		IAssembly definitionAssembly = typeDefOrRef.DefinitionAssembly;
		if (definitionAssembly == null)
		{
			return fullName;
		}
		string result = fullName;
		string realAssemblyName = GetRealAssemblyName(definitionAssembly);
		if (!string.IsNullOrEmpty(realAssemblyName))
		{
			result = typeDefOrRef.ReflectionFullName + ", " + realAssemblyName;
		}
		return result;
	}

	private string GetRealAssemblyName(IAssembly asm)
	{
		string fullName = asm.FullName;
		if (!asmNameToAsmFullName.TryGetValue(fullName, out var value))
		{
			value = (asmNameToAsmFullName[fullName] = TryGetRealAssemblyName(asm));
		}
		return value;
	}

	private string TryGetRealAssemblyName(IAssembly asm)
	{
		UTF8String name = asm.Name;
		if (name == module.CorLibTypes.AssemblyRef.Name)
		{
			return module.CorLibTypes.AssemblyRef.FullName;
		}
		if (moduleMD != null)
		{
			AssemblyRef assemblyRef = moduleMD.GetAssemblyRef(name);
			if (assemblyRef != null)
			{
				return assemblyRef.FullName;
			}
		}
		return GetAssemblyFullName(name);
	}

	protected virtual string GetAssemblyFullName(string simpleName)
	{
		return null;
	}

	public List<UserResourceType> GetSortedTypes()
	{
		List<UserResourceType> list = new List<UserResourceType>(dict.Values);
		list.Sort((UserResourceType a, UserResourceType b) => ((int)a.Code).CompareTo((int)b.Code));
		return list;
	}
}
