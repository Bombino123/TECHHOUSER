using System;
using dnlib.IO;

namespace dnlib.DotNet.Pdb.Portable;

internal struct LocalConstantSigBlobReader
{
	private readonly ModuleDef module;

	private DataReader reader;

	private readonly GenericParamContext gpContext;

	private RecursionCounter recursionCounter;

	private static readonly UTF8String stringSystem = new UTF8String("System");

	private static readonly UTF8String stringDecimal = new UTF8String("Decimal");

	private static readonly UTF8String stringDateTime = new UTF8String("DateTime");

	public LocalConstantSigBlobReader(ModuleDef module, ref DataReader reader, GenericParamContext gpContext)
	{
		this.module = module;
		this.reader = reader;
		this.gpContext = gpContext;
		recursionCounter = default(RecursionCounter);
	}

	public bool Read(out TypeSig type, out object value)
	{
		return ReadCatch(out type, out value);
	}

	private bool ReadCatch(out TypeSig type, out object value)
	{
		try
		{
			return ReadCore(out type, out value);
		}
		catch
		{
		}
		type = null;
		value = null;
		return false;
	}

	private bool ReadCore(out TypeSig type, out object value)
	{
		if (!recursionCounter.Increment())
		{
			type = null;
			value = null;
			return false;
		}
		bool flag;
		switch ((ElementType)reader.ReadByte())
		{
		case ElementType.Boolean:
			type = module.CorLibTypes.Boolean;
			value = reader.ReadBoolean();
			if (reader.Position < reader.Length)
			{
				type = ReadTypeDefOrRefSig();
			}
			flag = true;
			break;
		case ElementType.Char:
			type = module.CorLibTypes.Char;
			value = reader.ReadChar();
			if (reader.Position < reader.Length)
			{
				type = ReadTypeDefOrRefSig();
			}
			flag = true;
			break;
		case ElementType.I1:
			type = module.CorLibTypes.SByte;
			value = reader.ReadSByte();
			if (reader.Position < reader.Length)
			{
				type = ReadTypeDefOrRefSig();
			}
			flag = true;
			break;
		case ElementType.U1:
			type = module.CorLibTypes.Byte;
			value = reader.ReadByte();
			if (reader.Position < reader.Length)
			{
				type = ReadTypeDefOrRefSig();
			}
			flag = true;
			break;
		case ElementType.I2:
			type = module.CorLibTypes.Int16;
			value = reader.ReadInt16();
			if (reader.Position < reader.Length)
			{
				type = ReadTypeDefOrRefSig();
			}
			flag = true;
			break;
		case ElementType.U2:
			type = module.CorLibTypes.UInt16;
			value = reader.ReadUInt16();
			if (reader.Position < reader.Length)
			{
				type = ReadTypeDefOrRefSig();
			}
			flag = true;
			break;
		case ElementType.I4:
			type = module.CorLibTypes.Int32;
			value = reader.ReadInt32();
			if (reader.Position < reader.Length)
			{
				type = ReadTypeDefOrRefSig();
			}
			flag = true;
			break;
		case ElementType.U4:
			type = module.CorLibTypes.UInt32;
			value = reader.ReadUInt32();
			if (reader.Position < reader.Length)
			{
				type = ReadTypeDefOrRefSig();
			}
			flag = true;
			break;
		case ElementType.I8:
			type = module.CorLibTypes.Int64;
			value = reader.ReadInt64();
			if (reader.Position < reader.Length)
			{
				type = ReadTypeDefOrRefSig();
			}
			flag = true;
			break;
		case ElementType.U8:
			type = module.CorLibTypes.UInt64;
			value = reader.ReadUInt64();
			if (reader.Position < reader.Length)
			{
				type = ReadTypeDefOrRefSig();
			}
			flag = true;
			break;
		case ElementType.R4:
			type = module.CorLibTypes.Single;
			value = reader.ReadSingle();
			flag = true;
			break;
		case ElementType.R8:
			type = module.CorLibTypes.Double;
			value = reader.ReadDouble();
			flag = true;
			break;
		case ElementType.String:
			type = module.CorLibTypes.String;
			value = ReadString();
			flag = true;
			break;
		case ElementType.Ptr:
			flag = ReadCatch(out type, out value);
			if (flag)
			{
				type = new PtrSig(type);
			}
			break;
		case ElementType.ByRef:
			flag = ReadCatch(out type, out value);
			if (flag)
			{
				type = new ByRefSig(type);
			}
			break;
		case ElementType.Object:
			type = module.CorLibTypes.Object;
			value = null;
			flag = true;
			break;
		case ElementType.ValueType:
		{
			ITypeDefOrRef modifier = ReadTypeDefOrRef();
			type = modifier.ToTypeSig();
			value = null;
			if (GetName(modifier, out var @namespace, out var name) && @namespace == stringSystem && modifier.DefinitionAssembly.IsCorLib())
			{
				if (name == stringDecimal)
				{
					if (reader.Length - reader.Position != 13)
					{
						goto default;
					}
					try
					{
						byte b = reader.ReadByte();
						value = new decimal(reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32(), (b & 0x80) != 0, (byte)(b & 0x7Fu));
					}
					catch
					{
						goto default;
					}
				}
				else if (name == stringDateTime)
				{
					if (reader.Length - reader.Position != 8)
					{
						goto default;
					}
					try
					{
						value = new DateTime(reader.ReadInt64());
					}
					catch
					{
						goto default;
					}
				}
			}
			if (value == null && reader.Position != reader.Length)
			{
				value = reader.ReadRemainingBytes();
			}
			flag = true;
			break;
		}
		case ElementType.Class:
			type = new ClassSig(ReadTypeDefOrRef());
			value = ((reader.Position == reader.Length) ? null : reader.ReadRemainingBytes());
			flag = true;
			break;
		case ElementType.CModReqd:
		{
			ITypeDefOrRef modifier = ReadTypeDefOrRef();
			flag = ReadCatch(out type, out value);
			if (flag)
			{
				type = new CModReqdSig(modifier, type);
			}
			break;
		}
		case ElementType.CModOpt:
		{
			ITypeDefOrRef modifier = ReadTypeDefOrRef();
			flag = ReadCatch(out type, out value);
			if (flag)
			{
				type = new CModOptSig(modifier, type);
			}
			break;
		}
		default:
			flag = false;
			type = null;
			value = null;
			break;
		}
		recursionCounter.Decrement();
		return flag;
	}

	private static bool GetName(ITypeDefOrRef tdr, out UTF8String @namespace, out UTF8String name)
	{
		if (tdr is TypeRef typeRef)
		{
			@namespace = typeRef.Namespace;
			name = typeRef.Name;
			return true;
		}
		if (tdr is TypeDef typeDef)
		{
			@namespace = typeDef.Namespace;
			name = typeDef.Name;
			return true;
		}
		@namespace = null;
		name = null;
		return false;
	}

	private TypeSig ReadTypeDefOrRefSig()
	{
		if (!reader.TryReadCompressedUInt32(out var value))
		{
			return null;
		}
		return ((ISignatureReaderHelper)module).ResolveTypeDefOrRef(value, gpContext).ToTypeSig();
	}

	private ITypeDefOrRef ReadTypeDefOrRef()
	{
		if (!reader.TryReadCompressedUInt32(out var value))
		{
			return null;
		}
		ITypeDefOrRef typeDefOrRef = ((ISignatureReaderHelper)module).ResolveTypeDefOrRef(value, gpContext);
		CorLibTypeSig corLibTypeSig = module.CorLibTypes.GetCorLibTypeSig(typeDefOrRef);
		if (corLibTypeSig != null)
		{
			return corLibTypeSig.TypeDefOrRef;
		}
		return typeDefOrRef;
	}

	private string ReadString()
	{
		if (reader.Position == reader.Length)
		{
			return string.Empty;
		}
		if (reader.ReadByte() == byte.MaxValue && reader.Position == reader.Length)
		{
			return null;
		}
		reader.Position--;
		return reader.ReadUtf16String((int)(reader.BytesLeft / 2));
	}
}
