using System;
using System.IO;

namespace dnlib.DotNet.Writer;

public readonly struct MarshalBlobWriter : IDisposable, IFullNameFactoryHelper
{
	private readonly ModuleDef module;

	private readonly MemoryStream outStream;

	private readonly DataWriter writer;

	private readonly IWriterError helper;

	private readonly bool optimizeCustomAttributeSerializedTypeNames;

	public static byte[] Write(ModuleDef module, MarshalType marshalType, IWriterError helper)
	{
		return Write(module, marshalType, helper, optimizeCustomAttributeSerializedTypeNames: false);
	}

	public static byte[] Write(ModuleDef module, MarshalType marshalType, IWriterError helper, bool optimizeCustomAttributeSerializedTypeNames)
	{
		using MarshalBlobWriter marshalBlobWriter = new MarshalBlobWriter(module, helper, optimizeCustomAttributeSerializedTypeNames);
		return marshalBlobWriter.Write(marshalType);
	}

	private MarshalBlobWriter(ModuleDef module, IWriterError helper, bool optimizeCustomAttributeSerializedTypeNames)
	{
		this.module = module;
		outStream = new MemoryStream();
		writer = new DataWriter(outStream);
		this.helper = helper;
		this.optimizeCustomAttributeSerializedTypeNames = optimizeCustomAttributeSerializedTypeNames;
	}

	private byte[] Write(MarshalType marshalType)
	{
		if (marshalType == null)
		{
			return null;
		}
		NativeType nativeType = marshalType.NativeType;
		if (nativeType != NativeType.RawBlob)
		{
			if (nativeType > (NativeType)255u)
			{
				helper.Error("Invalid MarshalType.NativeType");
			}
			writer.WriteByte((byte)nativeType);
		}
		bool canWriteMore = true;
		switch (nativeType)
		{
		case NativeType.FixedSysString:
		{
			FixedSysStringMarshalType fixedSysStringMarshalType = (FixedSysStringMarshalType)marshalType;
			if (fixedSysStringMarshalType.IsSizeValid)
			{
				WriteCompressedUInt32((uint)fixedSysStringMarshalType.Size);
			}
			break;
		}
		case NativeType.SafeArray:
		{
			SafeArrayMarshalType safeArrayMarshalType = (SafeArrayMarshalType)marshalType;
			if (UpdateCanWrite(safeArrayMarshalType.IsVariantTypeValid, "VariantType", ref canWriteMore))
			{
				WriteCompressedUInt32((uint)safeArrayMarshalType.VariantType);
			}
			if (UpdateCanWrite(safeArrayMarshalType.IsUserDefinedSubTypeValid, "UserDefinedSubType", ref canWriteMore))
			{
				Write(safeArrayMarshalType.UserDefinedSubType.AssemblyQualifiedName);
			}
			break;
		}
		case NativeType.FixedArray:
		{
			FixedArrayMarshalType fixedArrayMarshalType = (FixedArrayMarshalType)marshalType;
			if (UpdateCanWrite(fixedArrayMarshalType.IsSizeValid, "Size", ref canWriteMore))
			{
				WriteCompressedUInt32((uint)fixedArrayMarshalType.Size);
			}
			if (UpdateCanWrite(fixedArrayMarshalType.IsElementTypeValid, "ElementType", ref canWriteMore))
			{
				WriteCompressedUInt32((uint)fixedArrayMarshalType.ElementType);
			}
			break;
		}
		case NativeType.Array:
		{
			ArrayMarshalType arrayMarshalType = (ArrayMarshalType)marshalType;
			if (UpdateCanWrite(arrayMarshalType.IsElementTypeValid, "ElementType", ref canWriteMore))
			{
				WriteCompressedUInt32((uint)arrayMarshalType.ElementType);
			}
			if (UpdateCanWrite(arrayMarshalType.IsParamNumberValid, "ParamNumber", ref canWriteMore))
			{
				WriteCompressedUInt32((uint)arrayMarshalType.ParamNumber);
			}
			if (UpdateCanWrite(arrayMarshalType.IsSizeValid, "Size", ref canWriteMore))
			{
				WriteCompressedUInt32((uint)arrayMarshalType.Size);
			}
			if (UpdateCanWrite(arrayMarshalType.IsFlagsValid, "Flags", ref canWriteMore))
			{
				WriteCompressedUInt32((uint)arrayMarshalType.Flags);
			}
			break;
		}
		case NativeType.CustomMarshaler:
		{
			CustomMarshalType customMarshalType = (CustomMarshalType)marshalType;
			Write(customMarshalType.Guid);
			Write(customMarshalType.NativeTypeName);
			ITypeDefOrRef customMarshaler = customMarshalType.CustomMarshaler;
			string text = ((customMarshaler == null) ? string.Empty : FullNameFactory.AssemblyQualifiedName(customMarshaler, this));
			Write(text);
			Write(customMarshalType.Cookie);
			break;
		}
		case NativeType.IUnknown:
		case NativeType.IDispatch:
		case NativeType.IntF:
		{
			InterfaceMarshalType interfaceMarshalType = (InterfaceMarshalType)marshalType;
			if (interfaceMarshalType.IsIidParamIndexValid)
			{
				WriteCompressedUInt32((uint)interfaceMarshalType.IidParamIndex);
			}
			break;
		}
		case NativeType.RawBlob:
		{
			byte[] data = ((RawMarshalType)marshalType).Data;
			if (data != null)
			{
				writer.WriteBytes(data);
			}
			break;
		}
		}
		return outStream.ToArray();
	}

	private bool UpdateCanWrite(bool isValid, string field, ref bool canWriteMore)
	{
		if (!canWriteMore)
		{
			if (isValid)
			{
				helper.Error2("MarshalType field {0} is valid even though a previous field was invalid.", field);
			}
			return canWriteMore;
		}
		if (!isValid)
		{
			canWriteMore = false;
		}
		return canWriteMore;
	}

	private uint WriteCompressedUInt32(uint value)
	{
		return writer.WriteCompressedUInt32(helper, value);
	}

	private void Write(UTF8String s)
	{
		writer.Write(helper, s);
	}

	public void Dispose()
	{
		outStream?.Dispose();
	}

	bool IFullNameFactoryHelper.MustUseAssemblyName(IType type)
	{
		return FullNameFactory.MustUseAssemblyName(module, type, optimizeCustomAttributeSerializedTypeNames);
	}
}
