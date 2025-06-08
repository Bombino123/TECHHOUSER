using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace dnlib.DotNet.Writer;

public readonly struct DeclSecurityWriter : ICustomAttributeWriterHelper, IWriterError, IFullNameFactoryHelper
{
	private readonly ModuleDef module;

	private readonly IWriterError helper;

	private readonly DataWriterContext context;

	private readonly bool optimizeCustomAttributeSerializedTypeNames;

	public static byte[] Write(ModuleDef module, IList<SecurityAttribute> secAttrs, IWriterError helper)
	{
		return Write(module, secAttrs, helper, optimizeCustomAttributeSerializedTypeNames: false);
	}

	public static byte[] Write(ModuleDef module, IList<SecurityAttribute> secAttrs, IWriterError helper, bool optimizeCustomAttributeSerializedTypeNames)
	{
		return new DeclSecurityWriter(module, helper, optimizeCustomAttributeSerializedTypeNames, null).Write(secAttrs);
	}

	internal static byte[] Write(ModuleDef module, IList<SecurityAttribute> secAttrs, IWriterError helper, bool optimizeCustomAttributeSerializedTypeNames, DataWriterContext context)
	{
		return new DeclSecurityWriter(module, helper, optimizeCustomAttributeSerializedTypeNames, context).Write(secAttrs);
	}

	private DeclSecurityWriter(ModuleDef module, IWriterError helper, bool optimizeCustomAttributeSerializedTypeNames, DataWriterContext context)
	{
		this.module = module;
		this.helper = helper;
		this.context = context;
		this.optimizeCustomAttributeSerializedTypeNames = optimizeCustomAttributeSerializedTypeNames;
	}

	private byte[] Write(IList<SecurityAttribute> secAttrs)
	{
		if (secAttrs == null)
		{
			secAttrs = Array2.Empty<SecurityAttribute>();
		}
		string net1xXmlStringInternal = DeclSecurity.GetNet1xXmlStringInternal(secAttrs);
		if (net1xXmlStringInternal != null)
		{
			return WriteFormat1(net1xXmlStringInternal);
		}
		return WriteFormat2(secAttrs);
	}

	private byte[] WriteFormat1(string xml)
	{
		return Encoding.Unicode.GetBytes(xml);
	}

	private byte[] WriteFormat2(IList<SecurityAttribute> secAttrs)
	{
		StringBuilder stringBuilder = new StringBuilder();
		MemoryStream memoryStream = new MemoryStream();
		DataWriter dataWriter = new DataWriter(memoryStream);
		dataWriter.WriteByte(46);
		WriteCompressedUInt32(dataWriter, (uint)secAttrs.Count);
		int count = secAttrs.Count;
		for (int i = 0; i < count; i++)
		{
			SecurityAttribute securityAttribute = secAttrs[i];
			if (securityAttribute == null)
			{
				helper.Error("SecurityAttribute is null");
				Write(dataWriter, UTF8String.Empty);
				WriteCompressedUInt32(dataWriter, 1u);
				WriteCompressedUInt32(dataWriter, 0u);
				continue;
			}
			ITypeDefOrRef attributeType = securityAttribute.AttributeType;
			string text;
			if (attributeType == null)
			{
				helper.Error("SecurityAttribute attribute type is null");
				text = string.Empty;
			}
			else
			{
				stringBuilder.Length = 0;
				text = FullNameFactory.AssemblyQualifiedName(attributeType, null, stringBuilder);
			}
			Write(dataWriter, text);
			byte[] array = ((context == null) ? CustomAttributeWriter.Write(this, securityAttribute.NamedArguments) : CustomAttributeWriter.Write(this, securityAttribute.NamedArguments, context));
			if (array.Length > 536870911)
			{
				helper.Error("Named arguments blob size doesn't fit in 29 bits");
				array = Array2.Empty<byte>();
			}
			WriteCompressedUInt32(dataWriter, (uint)array.Length);
			dataWriter.WriteBytes(array);
		}
		return memoryStream.ToArray();
	}

	private uint WriteCompressedUInt32(DataWriter writer, uint value)
	{
		return writer.WriteCompressedUInt32(helper, value);
	}

	private void Write(DataWriter writer, UTF8String s)
	{
		writer.Write(helper, s);
	}

	void IWriterError.Error(string message)
	{
		helper.Error(message);
	}

	bool IFullNameFactoryHelper.MustUseAssemblyName(IType type)
	{
		return FullNameFactory.MustUseAssemblyName(module, type, optimizeCustomAttributeSerializedTypeNames);
	}
}
