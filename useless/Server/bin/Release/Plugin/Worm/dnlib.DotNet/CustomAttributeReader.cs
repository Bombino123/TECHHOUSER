using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using dnlib.IO;

namespace dnlib.DotNet;

[ComVisible(true)]
public struct CustomAttributeReader
{
	private readonly ModuleDef module;

	private DataReader reader;

	private readonly uint caBlobOffset;

	private readonly GenericParamContext gpContext;

	private GenericArguments genericArguments;

	private RecursionCounter recursionCounter;

	private bool verifyReadAllBytes;

	public static CustomAttribute Read(ModuleDefMD readerModule, ICustomAttributeType ctor, uint offset)
	{
		return Read(readerModule, ctor, offset, default(GenericParamContext));
	}

	public static CustomAttribute Read(ModuleDefMD readerModule, ICustomAttributeType ctor, uint offset, GenericParamContext gpContext)
	{
		CustomAttributeReader customAttributeReader = new CustomAttributeReader(readerModule, offset, gpContext);
		try
		{
			if (ctor == null)
			{
				return customAttributeReader.CreateRaw(ctor);
			}
			return customAttributeReader.Read(ctor);
		}
		catch (CABlobParserException)
		{
			return customAttributeReader.CreateRaw(ctor);
		}
		catch (IOException)
		{
			return customAttributeReader.CreateRaw(ctor);
		}
	}

	private CustomAttribute CreateRaw(ICustomAttributeType ctor)
	{
		return new CustomAttribute(ctor, GetRawBlob());
	}

	public static CustomAttribute Read(ModuleDef module, byte[] caBlob, ICustomAttributeType ctor)
	{
		return Read(module, ByteArrayDataReaderFactory.CreateReader(caBlob), ctor, default(GenericParamContext));
	}

	public static CustomAttribute Read(ModuleDef module, DataReader reader, ICustomAttributeType ctor)
	{
		return Read(module, ref reader, ctor, default(GenericParamContext));
	}

	public static CustomAttribute Read(ModuleDef module, byte[] caBlob, ICustomAttributeType ctor, GenericParamContext gpContext)
	{
		return Read(module, ByteArrayDataReaderFactory.CreateReader(caBlob), ctor, gpContext);
	}

	public static CustomAttribute Read(ModuleDef module, DataReader reader, ICustomAttributeType ctor, GenericParamContext gpContext)
	{
		return Read(module, ref reader, ctor, gpContext);
	}

	private static CustomAttribute Read(ModuleDef module, ref DataReader reader, ICustomAttributeType ctor, GenericParamContext gpContext)
	{
		CustomAttributeReader customAttributeReader = new CustomAttributeReader(module, ref reader, gpContext);
		try
		{
			if (ctor == null)
			{
				return customAttributeReader.CreateRaw(ctor);
			}
			return customAttributeReader.Read(ctor);
		}
		catch (CABlobParserException)
		{
			return customAttributeReader.CreateRaw(ctor);
		}
		catch (IOException)
		{
			return customAttributeReader.CreateRaw(ctor);
		}
	}

	internal static List<CANamedArgument> ReadNamedArguments(ModuleDef module, ref DataReader reader, int numNamedArgs, GenericParamContext gpContext)
	{
		try
		{
			CustomAttributeReader customAttributeReader = new CustomAttributeReader(module, ref reader, gpContext);
			List<CANamedArgument> result = customAttributeReader.ReadNamedArguments(numNamedArgs);
			reader.CurrentOffset = customAttributeReader.reader.CurrentOffset;
			return result;
		}
		catch (CABlobParserException)
		{
			return null;
		}
		catch (IOException)
		{
			return null;
		}
	}

	private CustomAttributeReader(ModuleDefMD readerModule, uint offset, GenericParamContext gpContext)
	{
		module = readerModule;
		caBlobOffset = offset;
		reader = readerModule.BlobStream.CreateReader(offset);
		genericArguments = null;
		recursionCounter = default(RecursionCounter);
		verifyReadAllBytes = false;
		this.gpContext = gpContext;
	}

	private CustomAttributeReader(ModuleDef module, ref DataReader reader, GenericParamContext gpContext)
	{
		this.module = module;
		caBlobOffset = 0u;
		this.reader = reader;
		genericArguments = null;
		recursionCounter = default(RecursionCounter);
		verifyReadAllBytes = false;
		this.gpContext = gpContext;
	}

	private byte[] GetRawBlob()
	{
		return reader.ToArray();
	}

	private CustomAttribute Read(ICustomAttributeType ctor)
	{
		MethodSig obj = ctor?.MethodSig ?? throw new CABlobParserException("ctor is null or not a method");
		if (ctor is MemberRef { Class: TypeSpec { TypeSig: GenericInstSig typeSig } })
		{
			genericArguments = new GenericArguments();
			genericArguments.PushTypeArgs(typeSig.GenericArguments);
		}
		IList<TypeSig> @params = obj.Params;
		if ((@params.Count != 0 || reader.Position != reader.Length) && reader.ReadUInt16() != 1)
		{
			throw new CABlobParserException("Invalid CA blob prolog");
		}
		List<CAArgument> list = new List<CAArgument>(@params.Count);
		int count = @params.Count;
		for (int i = 0; i < count; i++)
		{
			list.Add(ReadFixedArg(FixTypeSig(@params[i])));
		}
		int numNamedArgs = ((reader.Position != reader.Length) ? reader.ReadUInt16() : 0);
		List<CANamedArgument> namedArguments = ReadNamedArguments(numNamedArgs);
		if (verifyReadAllBytes && reader.Position != reader.Length)
		{
			throw new CABlobParserException("Not all CA blob bytes were read");
		}
		return new CustomAttribute(ctor, list, namedArguments, caBlobOffset);
	}

	private List<CANamedArgument> ReadNamedArguments(int numNamedArgs)
	{
		if ((uint)numNamedArgs >= 1073741824u || numNamedArgs * 4 > reader.BytesLeft)
		{
			return null;
		}
		List<CANamedArgument> list = new List<CANamedArgument>(numNamedArgs);
		for (int i = 0; i < numNamedArgs; i++)
		{
			if (reader.Position == reader.Length)
			{
				break;
			}
			list.Add(ReadNamedArgument());
		}
		return list;
	}

	private TypeSig FixTypeSig(TypeSig type)
	{
		return SubstituteGenericParameter(type.RemoveModifiers()).RemoveModifiers();
	}

	private TypeSig SubstituteGenericParameter(TypeSig type)
	{
		if (genericArguments == null)
		{
			return type;
		}
		return genericArguments.Resolve(type);
	}

	private CAArgument ReadFixedArg(TypeSig argType)
	{
		if (!recursionCounter.Increment())
		{
			throw new CABlobParserException("Too much recursion");
		}
		if (argType == null)
		{
			throw new CABlobParserException("null argType");
		}
		CAArgument result = ((!(argType is SZArraySig arrayType)) ? ReadElem(argType) : ReadArrayArgument(arrayType));
		recursionCounter.Decrement();
		return result;
	}

	private CAArgument ReadElem(TypeSig argType)
	{
		if (argType == null)
		{
			throw new CABlobParserException("null argType");
		}
		TypeSig realArgType;
		object obj = ReadValue((SerializationType)argType.ElementType, argType, out realArgType);
		if (realArgType == null)
		{
			throw new CABlobParserException("Invalid arg type");
		}
		if (obj is CAArgument)
		{
			return (CAArgument)obj;
		}
		return new CAArgument(realArgType, obj);
	}

	private object ReadValue(SerializationType etype, TypeSig argType, out TypeSig realArgType)
	{
		if (!recursionCounter.Increment())
		{
			throw new CABlobParserException("Too much recursion");
		}
		object result;
		switch (etype)
		{
		case SerializationType.Boolean:
			realArgType = module.CorLibTypes.Boolean;
			result = reader.ReadByte() != 0;
			break;
		case SerializationType.Char:
			realArgType = module.CorLibTypes.Char;
			result = reader.ReadChar();
			break;
		case SerializationType.I1:
			realArgType = module.CorLibTypes.SByte;
			result = reader.ReadSByte();
			break;
		case SerializationType.U1:
			realArgType = module.CorLibTypes.Byte;
			result = reader.ReadByte();
			break;
		case SerializationType.I2:
			realArgType = module.CorLibTypes.Int16;
			result = reader.ReadInt16();
			break;
		case SerializationType.U2:
			realArgType = module.CorLibTypes.UInt16;
			result = reader.ReadUInt16();
			break;
		case SerializationType.I4:
			realArgType = module.CorLibTypes.Int32;
			result = reader.ReadInt32();
			break;
		case SerializationType.U4:
			realArgType = module.CorLibTypes.UInt32;
			result = reader.ReadUInt32();
			break;
		case SerializationType.I8:
			realArgType = module.CorLibTypes.Int64;
			result = reader.ReadInt64();
			break;
		case SerializationType.U8:
			realArgType = module.CorLibTypes.UInt64;
			result = reader.ReadUInt64();
			break;
		case SerializationType.R4:
			realArgType = module.CorLibTypes.Single;
			result = reader.ReadSingle();
			break;
		case SerializationType.R8:
			realArgType = module.CorLibTypes.Double;
			result = reader.ReadDouble();
			break;
		case SerializationType.String:
			realArgType = module.CorLibTypes.String;
			result = ReadUTF8String();
			break;
		case (SerializationType)17:
			if (argType == null)
			{
				throw new CABlobParserException("Invalid element type");
			}
			realArgType = argType;
			result = ReadEnumValue(GetEnumUnderlyingType(argType));
			break;
		case (SerializationType)28:
		case SerializationType.TaggedObject:
		{
			realArgType = ReadFieldOrPropType();
			result = ((!(realArgType is SZArraySig arrayType)) ? ReadValue((SerializationType)realArgType.ElementType, realArgType, out var _) : ((object)ReadArrayArgument(arrayType)));
			break;
		}
		case (SerializationType)18:
			if (argType is TypeDefOrRefSig typeDefOrRefSig && typeDefOrRefSig.DefinitionAssembly.IsCorLib() && typeDefOrRefSig.Namespace == "System")
			{
				if (typeDefOrRefSig.TypeName == "Type")
				{
					result = ReadValue(SerializationType.Type, typeDefOrRefSig, out realArgType);
					break;
				}
				if (typeDefOrRefSig.TypeName == "String")
				{
					result = ReadValue(SerializationType.String, typeDefOrRefSig, out realArgType);
					break;
				}
				if (typeDefOrRefSig.TypeName == "Object")
				{
					result = ReadValue(SerializationType.TaggedObject, typeDefOrRefSig, out realArgType);
					break;
				}
			}
			realArgType = argType;
			result = ReadEnumValue(null);
			break;
		case SerializationType.Type:
			realArgType = argType;
			result = ReadType(canReturnNull: true);
			break;
		case SerializationType.Enum:
			realArgType = ReadType(canReturnNull: false);
			result = ReadEnumValue(GetEnumUnderlyingType(realArgType));
			break;
		default:
			throw new CABlobParserException("Invalid element type");
		}
		recursionCounter.Decrement();
		return result;
	}

	private object ReadEnumValue(TypeSig underlyingType)
	{
		if (underlyingType != null)
		{
			if ((int)underlyingType.ElementType < 2 || (int)underlyingType.ElementType > 11)
			{
				throw new CABlobParserException("Invalid enum underlying type");
			}
			TypeSig realArgType;
			return ReadValue((SerializationType)underlyingType.ElementType, underlyingType, out realArgType);
		}
		verifyReadAllBytes = true;
		return reader.ReadInt32();
	}

	private TypeSig ReadType(bool canReturnNull)
	{
		UTF8String uTF8String = ReadUTF8String();
		if (canReturnNull && (object)uTF8String == null)
		{
			return null;
		}
		CAAssemblyRefFinder typeNameParserHelper = new CAAssemblyRefFinder(module);
		return TypeNameParser.ParseAsTypeSigReflection(module, UTF8String.ToSystemStringOrEmpty(uTF8String), typeNameParserHelper, gpContext) ?? throw new CABlobParserException("Could not parse type");
	}

	private static TypeSig GetEnumUnderlyingType(TypeSig type)
	{
		if (type == null)
		{
			throw new CABlobParserException("null enum type");
		}
		TypeDef typeDef = GetTypeDef(type);
		if (typeDef == null)
		{
			return null;
		}
		if (!typeDef.IsEnum)
		{
			throw new CABlobParserException("Not an enum");
		}
		return typeDef.GetEnumUnderlyingType().RemoveModifiers();
	}

	private static TypeDef GetTypeDef(TypeSig type)
	{
		if (type is TypeDefOrRefSig { TypeDef: var typeDef } typeDefOrRefSig)
		{
			if (typeDef != null)
			{
				return typeDef;
			}
			TypeRef typeRef = typeDefOrRefSig.TypeRef;
			if (typeRef != null)
			{
				return typeRef.Resolve();
			}
		}
		return null;
	}

	private CAArgument ReadArrayArgument(SZArraySig arrayType)
	{
		if (!recursionCounter.Increment())
		{
			throw new CABlobParserException("Too much recursion");
		}
		CAArgument result = new CAArgument(arrayType);
		int num = reader.ReadInt32();
		if (num != -1)
		{
			if (num < 0 || num > reader.BytesLeft)
			{
				throw new CABlobParserException("Array is too big");
			}
			List<CAArgument> list2 = (List<CAArgument>)(result.Value = new List<CAArgument>(num));
			for (int i = 0; i < num; i++)
			{
				list2.Add(ReadFixedArg(FixTypeSig(arrayType.Next)));
			}
		}
		recursionCounter.Decrement();
		return result;
	}

	private CANamedArgument ReadNamedArgument()
	{
		int isField = (SerializationType)reader.ReadByte() switch
		{
			SerializationType.Property => 0, 
			SerializationType.Field => 1, 
			_ => throw new CABlobParserException("Named argument is not a field/property"), 
		};
		TypeSig typeSig = ReadFieldOrPropType();
		UTF8String name = ReadUTF8String();
		CAArgument argument = ReadFixedArg(typeSig);
		return new CANamedArgument((byte)isField != 0, typeSig, name, argument);
	}

	private TypeSig ReadFieldOrPropType()
	{
		if (!recursionCounter.Increment())
		{
			throw new CABlobParserException("Too much recursion");
		}
		object result = (SerializationType)reader.ReadByte() switch
		{
			SerializationType.Boolean => module.CorLibTypes.Boolean, 
			SerializationType.Char => module.CorLibTypes.Char, 
			SerializationType.I1 => module.CorLibTypes.SByte, 
			SerializationType.U1 => module.CorLibTypes.Byte, 
			SerializationType.I2 => module.CorLibTypes.Int16, 
			SerializationType.U2 => module.CorLibTypes.UInt16, 
			SerializationType.I4 => module.CorLibTypes.Int32, 
			SerializationType.U4 => module.CorLibTypes.UInt32, 
			SerializationType.I8 => module.CorLibTypes.Int64, 
			SerializationType.U8 => module.CorLibTypes.UInt64, 
			SerializationType.R4 => module.CorLibTypes.Single, 
			SerializationType.R8 => module.CorLibTypes.Double, 
			SerializationType.String => module.CorLibTypes.String, 
			SerializationType.SZArray => new SZArraySig(ReadFieldOrPropType()), 
			SerializationType.Type => new ClassSig(module.CorLibTypes.GetTypeRef("System", "Type")), 
			SerializationType.TaggedObject => module.CorLibTypes.Object, 
			SerializationType.Enum => ReadType(canReturnNull: false), 
			_ => throw new CABlobParserException("Invalid type"), 
		};
		recursionCounter.Decrement();
		return (TypeSig)result;
	}

	private UTF8String ReadUTF8String()
	{
		if (reader.ReadByte() == byte.MaxValue)
		{
			return null;
		}
		reader.Position--;
		if (!reader.TryReadCompressedUInt32(out var value))
		{
			throw new CABlobParserException("Could not read compressed UInt32");
		}
		if (value == 0)
		{
			return UTF8String.Empty;
		}
		return new UTF8String(reader.ReadBytes((int)value));
	}
}
