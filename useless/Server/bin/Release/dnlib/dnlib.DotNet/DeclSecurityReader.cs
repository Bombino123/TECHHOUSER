using System;
using System.Collections.Generic;
using dnlib.IO;

namespace dnlib.DotNet;

public struct DeclSecurityReader
{
	private DataReader reader;

	private readonly ModuleDef module;

	private readonly GenericParamContext gpContext;

	public static IList<SecurityAttribute> Read(ModuleDefMD module, uint sig)
	{
		return Read(module, module.BlobStream.CreateReader(sig), default(GenericParamContext));
	}

	public static IList<SecurityAttribute> Read(ModuleDefMD module, uint sig, GenericParamContext gpContext)
	{
		return Read(module, module.BlobStream.CreateReader(sig), gpContext);
	}

	public static IList<SecurityAttribute> Read(ModuleDef module, byte[] blob)
	{
		return Read(module, ByteArrayDataReaderFactory.CreateReader(blob), default(GenericParamContext));
	}

	public static IList<SecurityAttribute> Read(ModuleDef module, byte[] blob, GenericParamContext gpContext)
	{
		return Read(module, ByteArrayDataReaderFactory.CreateReader(blob), gpContext);
	}

	public static IList<SecurityAttribute> Read(ModuleDef module, DataReader signature)
	{
		return Read(module, signature, default(GenericParamContext));
	}

	public static IList<SecurityAttribute> Read(ModuleDef module, DataReader signature, GenericParamContext gpContext)
	{
		return new DeclSecurityReader(module, signature, gpContext).Read();
	}

	private DeclSecurityReader(ModuleDef module, DataReader reader, GenericParamContext gpContext)
	{
		this.reader = reader;
		this.module = module;
		this.gpContext = gpContext;
	}

	private IList<SecurityAttribute> Read()
	{
		try
		{
			if (reader.Position >= reader.Length)
			{
				return new List<SecurityAttribute>();
			}
			if (reader.ReadByte() == 46)
			{
				return ReadBinaryFormat();
			}
			reader.Position--;
			return ReadXmlFormat();
		}
		catch
		{
			return new List<SecurityAttribute>();
		}
	}

	private IList<SecurityAttribute> ReadBinaryFormat()
	{
		int num = (int)reader.ReadCompressedUInt32();
		List<SecurityAttribute> list = new List<SecurityAttribute>(num);
		for (int i = 0; i < num; i++)
		{
			UTF8String utf = ReadUTF8String();
			ITypeDefOrRef attrType = TypeNameParser.ParseReflection(module, UTF8String.ToSystemStringOrEmpty(utf), new CAAssemblyRefFinder(module), gpContext);
			reader.ReadCompressedUInt32();
			int numNamedArgs = (int)reader.ReadCompressedUInt32();
			List<CANamedArgument> list2 = CustomAttributeReader.ReadNamedArguments(module, ref reader, numNamedArgs, gpContext);
			if (list2 == null)
			{
				throw new ApplicationException("Could not read named arguments");
			}
			list.Add(new SecurityAttribute(attrType, list2));
		}
		return list;
	}

	private IList<SecurityAttribute> ReadXmlFormat()
	{
		string xml = reader.ReadUtf16String((int)reader.Length / 2);
		SecurityAttribute item = SecurityAttribute.CreateFromXml(module, xml);
		return new List<SecurityAttribute> { item };
	}

	private UTF8String ReadUTF8String()
	{
		uint num = reader.ReadCompressedUInt32();
		if (num != 0)
		{
			return new UTF8String(reader.ReadBytes((int)num));
		}
		return UTF8String.Empty;
	}
}
