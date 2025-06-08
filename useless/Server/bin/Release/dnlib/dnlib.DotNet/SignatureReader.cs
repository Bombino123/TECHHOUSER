using System;
using System.Collections.Generic;
using System.IO;
using dnlib.DotNet.MD;
using dnlib.IO;

namespace dnlib.DotNet;

public struct SignatureReader
{
	private const uint MaxArrayRank = 64u;

	private readonly ISignatureReaderHelper helper;

	private readonly ICorLibTypes corLibTypes;

	private DataReader reader;

	private readonly GenericParamContext gpContext;

	private RecursionCounter recursionCounter;

	public static CallingConventionSig ReadSig(ModuleDefMD readerModule, uint sig)
	{
		return ReadSig(readerModule, sig, default(GenericParamContext));
	}

	public static CallingConventionSig ReadSig(ModuleDefMD readerModule, uint sig, GenericParamContext gpContext)
	{
		try
		{
			SignatureReader signatureReader = new SignatureReader(readerModule, sig, gpContext);
			if (signatureReader.reader.Length == 0)
			{
				return null;
			}
			CallingConventionSig callingConventionSig = signatureReader.ReadSig();
			if (callingConventionSig != null)
			{
				callingConventionSig.ExtraData = signatureReader.GetExtraData();
			}
			return callingConventionSig;
		}
		catch
		{
			return null;
		}
	}

	public static CallingConventionSig ReadSig(ModuleDefMD module, byte[] signature)
	{
		return ReadSig(module, module.CorLibTypes, ByteArrayDataReaderFactory.CreateReader(signature), default(GenericParamContext));
	}

	public static CallingConventionSig ReadSig(ModuleDefMD module, byte[] signature, GenericParamContext gpContext)
	{
		return ReadSig(module, module.CorLibTypes, ByteArrayDataReaderFactory.CreateReader(signature), gpContext);
	}

	public static CallingConventionSig ReadSig(ModuleDefMD module, DataReader signature)
	{
		return ReadSig(module, module.CorLibTypes, signature, default(GenericParamContext));
	}

	public static CallingConventionSig ReadSig(ModuleDefMD module, DataReader signature, GenericParamContext gpContext)
	{
		return ReadSig(module, module.CorLibTypes, signature, gpContext);
	}

	public static CallingConventionSig ReadSig(ISignatureReaderHelper helper, ICorLibTypes corLibTypes, byte[] signature)
	{
		return ReadSig(helper, corLibTypes, ByteArrayDataReaderFactory.CreateReader(signature), default(GenericParamContext));
	}

	public static CallingConventionSig ReadSig(ISignatureReaderHelper helper, ICorLibTypes corLibTypes, byte[] signature, GenericParamContext gpContext)
	{
		return ReadSig(helper, corLibTypes, ByteArrayDataReaderFactory.CreateReader(signature), gpContext);
	}

	public static CallingConventionSig ReadSig(ISignatureReaderHelper helper, ICorLibTypes corLibTypes, DataReader signature)
	{
		return ReadSig(helper, corLibTypes, signature, default(GenericParamContext));
	}

	public static CallingConventionSig ReadSig(ISignatureReaderHelper helper, ICorLibTypes corLibTypes, DataReader signature, GenericParamContext gpContext)
	{
		try
		{
			SignatureReader signatureReader = new SignatureReader(helper, corLibTypes, ref signature, gpContext);
			if (signatureReader.reader.Length == 0)
			{
				return null;
			}
			return signatureReader.ReadSig();
		}
		catch
		{
			return null;
		}
	}

	public static TypeSig ReadTypeSig(ModuleDefMD readerModule, uint sig)
	{
		return ReadTypeSig(readerModule, sig, default(GenericParamContext));
	}

	public static TypeSig ReadTypeSig(ModuleDefMD readerModule, uint sig, GenericParamContext gpContext)
	{
		try
		{
			return new SignatureReader(readerModule, sig, gpContext).ReadType();
		}
		catch
		{
			return null;
		}
	}

	public static TypeSig ReadTypeSig(ModuleDefMD readerModule, uint sig, out byte[] extraData)
	{
		return ReadTypeSig(readerModule, sig, default(GenericParamContext), out extraData);
	}

	public static TypeSig ReadTypeSig(ModuleDefMD readerModule, uint sig, GenericParamContext gpContext, out byte[] extraData)
	{
		try
		{
			SignatureReader signatureReader = new SignatureReader(readerModule, sig, gpContext);
			TypeSig result;
			try
			{
				result = signatureReader.ReadType();
			}
			catch (IOException)
			{
				signatureReader.reader.Position = 0u;
				result = null;
			}
			extraData = signatureReader.GetExtraData();
			return result;
		}
		catch
		{
			extraData = null;
			return null;
		}
	}

	public static TypeSig ReadTypeSig(ModuleDefMD module, byte[] signature)
	{
		return ReadTypeSig(module, module.CorLibTypes, ByteArrayDataReaderFactory.CreateReader(signature), default(GenericParamContext));
	}

	public static TypeSig ReadTypeSig(ModuleDefMD module, byte[] signature, GenericParamContext gpContext)
	{
		return ReadTypeSig(module, module.CorLibTypes, ByteArrayDataReaderFactory.CreateReader(signature), gpContext);
	}

	public static TypeSig ReadTypeSig(ModuleDefMD module, DataReader signature)
	{
		return ReadTypeSig(module, module.CorLibTypes, signature, default(GenericParamContext));
	}

	public static TypeSig ReadTypeSig(ModuleDefMD module, DataReader signature, GenericParamContext gpContext)
	{
		return ReadTypeSig(module, module.CorLibTypes, signature, gpContext);
	}

	public static TypeSig ReadTypeSig(ISignatureReaderHelper helper, ICorLibTypes corLibTypes, byte[] signature)
	{
		return ReadTypeSig(helper, corLibTypes, ByteArrayDataReaderFactory.CreateReader(signature), default(GenericParamContext));
	}

	public static TypeSig ReadTypeSig(ISignatureReaderHelper helper, ICorLibTypes corLibTypes, byte[] signature, GenericParamContext gpContext)
	{
		return ReadTypeSig(helper, corLibTypes, ByteArrayDataReaderFactory.CreateReader(signature), gpContext);
	}

	public static TypeSig ReadTypeSig(ISignatureReaderHelper helper, ICorLibTypes corLibTypes, DataReader signature)
	{
		return ReadTypeSig(helper, corLibTypes, signature, default(GenericParamContext));
	}

	public static TypeSig ReadTypeSig(ISignatureReaderHelper helper, ICorLibTypes corLibTypes, DataReader signature, GenericParamContext gpContext)
	{
		byte[] extraData;
		return ReadTypeSig(helper, corLibTypes, signature, gpContext, out extraData);
	}

	public static TypeSig ReadTypeSig(ISignatureReaderHelper helper, ICorLibTypes corLibTypes, byte[] signature, GenericParamContext gpContext, out byte[] extraData)
	{
		return ReadTypeSig(helper, corLibTypes, ByteArrayDataReaderFactory.CreateReader(signature), gpContext, out extraData);
	}

	public static TypeSig ReadTypeSig(ISignatureReaderHelper helper, ICorLibTypes corLibTypes, DataReader signature, GenericParamContext gpContext, out byte[] extraData)
	{
		try
		{
			SignatureReader signatureReader = new SignatureReader(helper, corLibTypes, ref signature, gpContext);
			TypeSig result;
			try
			{
				result = signatureReader.ReadType();
			}
			catch (IOException)
			{
				signatureReader.reader.Position = 0u;
				result = null;
			}
			extraData = signatureReader.GetExtraData();
			return result;
		}
		catch
		{
			extraData = null;
			return null;
		}
	}

	private SignatureReader(ModuleDefMD readerModule, uint sig, GenericParamContext gpContext)
	{
		helper = readerModule;
		corLibTypes = readerModule.CorLibTypes;
		reader = readerModule.BlobStream.CreateReader(sig);
		this.gpContext = gpContext;
		recursionCounter = default(RecursionCounter);
	}

	private SignatureReader(ISignatureReaderHelper helper, ICorLibTypes corLibTypes, ref DataReader reader, GenericParamContext gpContext)
	{
		this.helper = helper;
		this.corLibTypes = corLibTypes;
		this.reader = reader;
		this.gpContext = gpContext;
		recursionCounter = default(RecursionCounter);
	}

	private byte[] GetExtraData()
	{
		if (reader.Position == reader.Length)
		{
			return null;
		}
		return reader.ReadRemainingBytes();
	}

	private CallingConventionSig ReadSig()
	{
		if (!recursionCounter.Increment())
		{
			return null;
		}
		CallingConvention callingConvention = (CallingConvention)reader.ReadByte();
		CallingConventionSig result;
		switch (callingConvention & CallingConvention.Mask)
		{
		case CallingConvention.Default:
		case CallingConvention.C:
		case CallingConvention.StdCall:
		case CallingConvention.ThisCall:
		case CallingConvention.FastCall:
		case CallingConvention.VarArg:
		case CallingConvention.Unmanaged:
		case CallingConvention.NativeVarArg:
			result = ReadMethod(callingConvention);
			break;
		case CallingConvention.Field:
			result = ReadField(callingConvention);
			break;
		case CallingConvention.LocalSig:
			result = ReadLocalSig(callingConvention);
			break;
		case CallingConvention.Property:
			result = ReadProperty(callingConvention);
			break;
		case CallingConvention.GenericInst:
			result = ReadGenericInstMethod(callingConvention);
			break;
		default:
			result = null;
			break;
		}
		recursionCounter.Decrement();
		return result;
	}

	private FieldSig ReadField(CallingConvention callingConvention)
	{
		return new FieldSig(callingConvention, ReadType());
	}

	private MethodSig ReadMethod(CallingConvention callingConvention)
	{
		return ReadSig(new MethodSig(callingConvention));
	}

	private PropertySig ReadProperty(CallingConvention callingConvention)
	{
		return ReadSig(new PropertySig(callingConvention));
	}

	private T ReadSig<T>(T methodSig) where T : MethodBaseSig
	{
		if (methodSig.Generic)
		{
			if (!reader.TryReadCompressedUInt32(out var value) || value > 65536)
			{
				return null;
			}
			methodSig.GenParamCount = value;
		}
		if (!reader.TryReadCompressedUInt32(out var value2) || value2 > 65536 || value2 > reader.BytesLeft)
		{
			return null;
		}
		methodSig.RetType = ReadType();
		IList<TypeSig> list = methodSig.Params;
		for (uint num = 0u; num < value2; num++)
		{
			TypeSig typeSig = ReadType();
			if (typeSig is SentinelSig)
			{
				if (methodSig.ParamsAfterSentinel == null)
				{
					list = (methodSig.ParamsAfterSentinel = new List<TypeSig>((int)(value2 - num)));
				}
				num--;
			}
			else
			{
				list.Add(typeSig);
			}
		}
		return methodSig;
	}

	private LocalSig ReadLocalSig(CallingConvention callingConvention)
	{
		if (!reader.TryReadCompressedUInt32(out var value) || value > 65536 || value > reader.BytesLeft)
		{
			return null;
		}
		LocalSig localSig = new LocalSig(callingConvention, value);
		IList<TypeSig> locals = localSig.Locals;
		for (uint num = 0u; num < value; num++)
		{
			locals.Add(ReadType());
		}
		return localSig;
	}

	private GenericInstMethodSig ReadGenericInstMethod(CallingConvention callingConvention)
	{
		if (!reader.TryReadCompressedUInt32(out var value) || value > 65536 || value > reader.BytesLeft)
		{
			return null;
		}
		GenericInstMethodSig genericInstMethodSig = new GenericInstMethodSig(callingConvention, value);
		IList<TypeSig> genericArguments = genericInstMethodSig.GenericArguments;
		for (uint num = 0u; num < value; num++)
		{
			genericArguments.Add(ReadType());
		}
		return genericInstMethodSig;
	}

	private TypeSig ReadType(bool allowTypeSpec = false)
	{
		if (!recursionCounter.Increment())
		{
			return null;
		}
		TypeSig result = null;
		uint value2;
		switch ((ElementType)reader.ReadByte())
		{
		case ElementType.Void:
			result = corLibTypes.Void;
			break;
		case ElementType.Boolean:
			result = corLibTypes.Boolean;
			break;
		case ElementType.Char:
			result = corLibTypes.Char;
			break;
		case ElementType.I1:
			result = corLibTypes.SByte;
			break;
		case ElementType.U1:
			result = corLibTypes.Byte;
			break;
		case ElementType.I2:
			result = corLibTypes.Int16;
			break;
		case ElementType.U2:
			result = corLibTypes.UInt16;
			break;
		case ElementType.I4:
			result = corLibTypes.Int32;
			break;
		case ElementType.U4:
			result = corLibTypes.UInt32;
			break;
		case ElementType.I8:
			result = corLibTypes.Int64;
			break;
		case ElementType.U8:
			result = corLibTypes.UInt64;
			break;
		case ElementType.R4:
			result = corLibTypes.Single;
			break;
		case ElementType.R8:
			result = corLibTypes.Double;
			break;
		case ElementType.String:
			result = corLibTypes.String;
			break;
		case ElementType.TypedByRef:
			result = corLibTypes.TypedReference;
			break;
		case ElementType.I:
			result = corLibTypes.IntPtr;
			break;
		case ElementType.U:
			result = corLibTypes.UIntPtr;
			break;
		case ElementType.Object:
			result = corLibTypes.Object;
			break;
		case ElementType.Ptr:
			result = new PtrSig(ReadType());
			break;
		case ElementType.ByRef:
			result = new ByRefSig(ReadType());
			break;
		case ElementType.ValueType:
			result = new ValueTypeSig(ReadTypeDefOrRef(allowTypeSpec));
			break;
		case ElementType.Class:
			result = new ClassSig(ReadTypeDefOrRef(allowTypeSpec));
			break;
		case ElementType.FnPtr:
			result = new FnPtrSig(ReadSig());
			break;
		case ElementType.SZArray:
			result = new SZArraySig(ReadType());
			break;
		case ElementType.CModReqd:
			result = new CModReqdSig(ReadTypeDefOrRef(allowTypeSpec: true), ReadType());
			break;
		case ElementType.CModOpt:
			result = new CModOptSig(ReadTypeDefOrRef(allowTypeSpec: true), ReadType());
			break;
		case ElementType.Sentinel:
			result = new SentinelSig();
			break;
		case ElementType.Pinned:
			result = new PinnedSig(ReadType());
			break;
		case ElementType.Var:
			if (reader.TryReadCompressedUInt32(out value2))
			{
				result = new GenericVar(value2, gpContext.Type);
			}
			break;
		case ElementType.MVar:
			if (reader.TryReadCompressedUInt32(out value2))
			{
				result = new GenericMVar(value2, gpContext.Method);
			}
			break;
		case ElementType.ValueArray:
		{
			TypeSig arrayType = ReadType();
			if (reader.TryReadCompressedUInt32(out value2))
			{
				result = new ValueArraySig(arrayType, value2);
			}
			break;
		}
		case ElementType.Module:
			if (reader.TryReadCompressedUInt32(out value2))
			{
				result = new ModuleSig(value2, ReadType());
			}
			break;
		case ElementType.GenericInst:
		{
			TypeSig arrayType = ReadType();
			if (reader.TryReadCompressedUInt32(out value2) && value2 <= 65536 && value2 <= reader.BytesLeft)
			{
				GenericInstSig genericInstSig = new GenericInstSig(arrayType as ClassOrValueTypeSig, value2);
				IList<TypeSig> genericArguments = genericInstSig.GenericArguments;
				for (uint num = 0u; num < value2; num++)
				{
					genericArguments.Add(ReadType());
				}
				result = genericInstSig;
			}
			break;
		}
		case ElementType.Array:
		{
			TypeSig arrayType = ReadType();
			if (!reader.TryReadCompressedUInt32(out var value))
			{
				break;
			}
			switch (value)
			{
			case 0u:
				result = new ArraySig(arrayType, value);
				break;
			case 1u:
			case 2u:
			case 3u:
			case 4u:
			case 5u:
			case 6u:
			case 7u:
			case 8u:
			case 9u:
			case 10u:
			case 11u:
			case 12u:
			case 13u:
			case 14u:
			case 15u:
			case 16u:
			case 17u:
			case 18u:
			case 19u:
			case 20u:
			case 21u:
			case 22u:
			case 23u:
			case 24u:
			case 25u:
			case 26u:
			case 27u:
			case 28u:
			case 29u:
			case 30u:
			case 31u:
			case 32u:
			case 33u:
			case 34u:
			case 35u:
			case 36u:
			case 37u:
			case 38u:
			case 39u:
			case 40u:
			case 41u:
			case 42u:
			case 43u:
			case 44u:
			case 45u:
			case 46u:
			case 47u:
			case 48u:
			case 49u:
			case 50u:
			case 51u:
			case 52u:
			case 53u:
			case 54u:
			case 55u:
			case 56u:
			case 57u:
			case 58u:
			case 59u:
			case 60u:
			case 61u:
			case 62u:
			case 63u:
			case 64u:
			{
				if (!reader.TryReadCompressedUInt32(out value2) || value2 > value)
				{
					break;
				}
				List<uint> list = new List<uint>((int)value2);
				uint num = 0u;
				while (true)
				{
					if (num < value2)
					{
						if (!reader.TryReadCompressedUInt32(out var value3))
						{
							break;
						}
						list.Add(value3);
						num++;
						continue;
					}
					if (!reader.TryReadCompressedUInt32(out value2) || value2 > value)
					{
						break;
					}
					List<int> list2 = new List<int>((int)value2);
					num = 0u;
					while (true)
					{
						if (num < value2)
						{
							if (!reader.TryReadCompressedInt32(out var value4))
							{
								break;
							}
							list2.Add(value4);
							num++;
							continue;
						}
						result = new ArraySig(arrayType, value, list, list2);
						break;
					}
					break;
				}
				break;
			}
			}
			break;
		}
		case ElementType.Internal:
		{
			IntPtr address = ((IntPtr.Size != 4) ? new IntPtr(reader.ReadInt64()) : new IntPtr(reader.ReadInt32()));
			result = helper.ConvertRTInternalAddress(address);
			break;
		}
		default:
			result = null;
			break;
		}
		recursionCounter.Decrement();
		return result;
	}

	private ITypeDefOrRef ReadTypeDefOrRef(bool allowTypeSpec)
	{
		if (!reader.TryReadCompressedUInt32(out var value))
		{
			return null;
		}
		if (!allowTypeSpec && CodedToken.TypeDefOrRef.Decode2(value).Table == Table.TypeSpec)
		{
			return null;
		}
		return helper.ResolveTypeDefOrRef(value, default(GenericParamContext));
	}
}
