using System.Runtime.InteropServices;
using dnlib.IO;

namespace dnlib.DotNet;

[ComVisible(true)]
public struct MarshalBlobReader
{
	private readonly ModuleDef module;

	private DataReader reader;

	private readonly GenericParamContext gpContext;

	public static MarshalType Read(ModuleDefMD module, uint sig)
	{
		return Read(module, module.BlobStream.CreateReader(sig), default(GenericParamContext));
	}

	public static MarshalType Read(ModuleDefMD module, uint sig, GenericParamContext gpContext)
	{
		return Read(module, module.BlobStream.CreateReader(sig), gpContext);
	}

	public static MarshalType Read(ModuleDef module, byte[] data)
	{
		return Read(module, ByteArrayDataReaderFactory.CreateReader(data), default(GenericParamContext));
	}

	public static MarshalType Read(ModuleDef module, byte[] data, GenericParamContext gpContext)
	{
		return Read(module, ByteArrayDataReaderFactory.CreateReader(data), gpContext);
	}

	public static MarshalType Read(ModuleDef module, DataReader reader)
	{
		return Read(module, reader, default(GenericParamContext));
	}

	public static MarshalType Read(ModuleDef module, DataReader reader, GenericParamContext gpContext)
	{
		return new MarshalBlobReader(module, ref reader, gpContext).Read();
	}

	private MarshalBlobReader(ModuleDef module, ref DataReader reader, GenericParamContext gpContext)
	{
		this.module = module;
		this.reader = reader;
		this.gpContext = gpContext;
	}

	private MarshalType Read()
	{
		try
		{
			NativeType nativeType = (NativeType)reader.ReadByte();
			switch (nativeType)
			{
			case NativeType.FixedSysString:
			{
				int numElems = (CanRead() ? ((int)reader.ReadCompressedUInt32()) : (-1));
				return new FixedSysStringMarshalType(numElems);
			}
			case NativeType.SafeArray:
			{
				int vt = (CanRead() ? ((int)reader.ReadCompressedUInt32()) : (-1));
				UTF8String uTF8String = (CanRead() ? ReadUTF8String() : null);
				ITypeDefOrRef userDefinedSubType = (((object)uTF8String == null) ? null : TypeNameParser.ParseReflection(module, UTF8String.ToSystemStringOrEmpty(uTF8String), null, gpContext));
				return new SafeArrayMarshalType((VariantType)vt, userDefinedSubType);
			}
			case NativeType.FixedArray:
			{
				int numElems = (CanRead() ? ((int)reader.ReadCompressedUInt32()) : (-1));
				NativeType elementType = (CanRead() ? ((NativeType)reader.ReadCompressedUInt32()) : NativeType.NotInitialized);
				return new FixedArrayMarshalType(numElems, elementType);
			}
			case NativeType.Array:
			{
				NativeType elementType = (CanRead() ? ((NativeType)reader.ReadCompressedUInt32()) : NativeType.NotInitialized);
				int paramNum = (CanRead() ? ((int)reader.ReadCompressedUInt32()) : (-1));
				int numElems = (CanRead() ? ((int)reader.ReadCompressedUInt32()) : (-1));
				int flags = (CanRead() ? ((int)reader.ReadCompressedUInt32()) : (-1));
				return new ArrayMarshalType(elementType, paramNum, numElems, flags);
			}
			case NativeType.CustomMarshaler:
			{
				UTF8String guid = ReadUTF8String();
				UTF8String nativeTypeName = ReadUTF8String();
				UTF8String uTF8String2 = ReadUTF8String();
				ITypeDefOrRef custMarshaler = ((uTF8String2.DataLength == 0) ? null : TypeNameParser.ParseReflection(module, UTF8String.ToSystemStringOrEmpty(uTF8String2), new CAAssemblyRefFinder(module), gpContext));
				UTF8String cookie = ReadUTF8String();
				return new CustomMarshalType(guid, nativeTypeName, custMarshaler, cookie);
			}
			case NativeType.IUnknown:
			case NativeType.IDispatch:
			case NativeType.IntF:
			{
				int iidParamIndex = (CanRead() ? ((int)reader.ReadCompressedUInt32()) : (-1));
				return new InterfaceMarshalType(nativeType, iidParamIndex);
			}
			default:
				return new MarshalType(nativeType);
			}
		}
		catch
		{
			return new RawMarshalType(reader.ToArray());
		}
	}

	private bool CanRead()
	{
		return reader.Position < reader.Length;
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
