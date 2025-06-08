using System;
using System.Text;
using dnlib.DotNet.MD;
using dnlib.DotNet.Writer;

namespace dnlib.DotNet.Pdb.Portable;

internal readonly struct LocalConstantSigBlobWriter
{
	private readonly IWriterError helper;

	private readonly dnlib.DotNet.Writer.Metadata systemMetadata;

	private static readonly UTF8String stringSystem = new UTF8String("System");

	private static readonly UTF8String stringDecimal = new UTF8String("Decimal");

	private static readonly UTF8String stringDateTime = new UTF8String("DateTime");

	private LocalConstantSigBlobWriter(IWriterError helper, dnlib.DotNet.Writer.Metadata systemMetadata)
	{
		this.helper = helper;
		this.systemMetadata = systemMetadata;
	}

	public static void Write(IWriterError helper, dnlib.DotNet.Writer.Metadata systemMetadata, DataWriter writer, TypeSig type, object value)
	{
		new LocalConstantSigBlobWriter(helper, systemMetadata).Write(writer, type, value);
	}

	private void Write(DataWriter writer, TypeSig type, object value)
	{
		while (type != null)
		{
			ElementType elementType = type.ElementType;
			writer.WriteByte((byte)elementType);
			switch (elementType)
			{
			case ElementType.Boolean:
			case ElementType.Char:
			case ElementType.I1:
			case ElementType.U1:
			case ElementType.I2:
			case ElementType.U2:
			case ElementType.I4:
			case ElementType.U4:
			case ElementType.I8:
			case ElementType.U8:
				WritePrimitiveValue(writer, elementType, value);
				return;
			case ElementType.R4:
				if (value is float)
				{
					writer.WriteSingle((float)value);
					return;
				}
				helper.Error("Expected a Single constant");
				writer.WriteSingle(0f);
				return;
			case ElementType.R8:
				if (value is double)
				{
					writer.WriteDouble((double)value);
					return;
				}
				helper.Error("Expected a Double constant");
				writer.WriteDouble(0.0);
				return;
			case ElementType.String:
				if (value == null)
				{
					writer.WriteByte(byte.MaxValue);
				}
				else if (value is string)
				{
					writer.WriteBytes(Encoding.Unicode.GetBytes((string)value));
				}
				else
				{
					helper.Error("Expected a String constant");
				}
				return;
			case ElementType.Ptr:
			case ElementType.ByRef:
				WriteTypeDefOrRef(writer, new TypeSpecUser(type));
				return;
			case ElementType.Object:
				return;
			case ElementType.ValueType:
			{
				ITypeDefOrRef typeDefOrRef = ((ValueTypeSig)type).TypeDefOrRef;
				TypeDef typeDef = typeDefOrRef.ResolveTypeDef();
				if (typeDef == null)
				{
					helper.Error2("Couldn't resolve type 0x{0:X8}.", typeDefOrRef?.MDToken.Raw ?? 0);
					return;
				}
				if (typeDef.IsEnum)
				{
					TypeSig a = typeDef.GetEnumUnderlyingType().RemovePinnedAndModifiers();
					ElementType elementType2 = a.GetElementType();
					if (elementType2 - 2 <= ElementType.U4)
					{
						writer.Position--;
						writer.WriteByte((byte)a.GetElementType());
						WritePrimitiveValue(writer, a.GetElementType(), value);
						WriteTypeDefOrRef(writer, typeDefOrRef);
					}
					else
					{
						helper.Error("Invalid enum underlying type");
					}
					return;
				}
				WriteTypeDefOrRef(writer, typeDefOrRef);
				bool flag = false;
				if (GetName(typeDefOrRef, out var @namespace, out var name) && @namespace == stringSystem && typeDefOrRef.DefinitionAssembly.IsCorLib())
				{
					if (name == stringDecimal)
					{
						if (value is decimal)
						{
							int[] bits = decimal.GetBits((decimal)value);
							writer.WriteByte((byte)((uint)(bits[3] >>> 31 << 7) | (((uint)bits[3] >> 16) & 0x7Fu)));
							writer.WriteInt32(bits[0]);
							writer.WriteInt32(bits[1]);
							writer.WriteInt32(bits[2]);
						}
						else
						{
							helper.Error("Expected a Decimal constant");
							writer.WriteBytes(new byte[13]);
						}
						flag = true;
					}
					else if (name == stringDateTime)
					{
						if (value is DateTime)
						{
							writer.WriteInt64(((DateTime)value).Ticks);
						}
						else
						{
							helper.Error("Expected a DateTime constant");
							writer.WriteInt64(0L);
						}
						flag = true;
					}
				}
				if (!flag)
				{
					if (value is byte[])
					{
						writer.WriteBytes((byte[])value);
					}
					else if (value != null)
					{
						helper.Error2("Unsupported constant: {0}.", value.GetType().FullName);
					}
				}
				return;
			}
			case ElementType.Class:
				WriteTypeDefOrRef(writer, ((ClassSig)type).TypeDefOrRef);
				if (value is byte[])
				{
					writer.WriteBytes((byte[])value);
				}
				else if (value != null)
				{
					helper.Error("Expected a null constant");
				}
				return;
			case ElementType.CModReqd:
			case ElementType.CModOpt:
				break;
			case ElementType.Var:
			case ElementType.Array:
			case ElementType.GenericInst:
			case ElementType.TypedByRef:
			case ElementType.I:
			case ElementType.U:
			case ElementType.FnPtr:
			case ElementType.SZArray:
			case ElementType.MVar:
				WriteTypeDefOrRef(writer, new TypeSpecUser(type));
				return;
			default:
				helper.Error2("Unsupported element type in LocalConstant sig blob: {0}.", elementType);
				return;
			}
			WriteTypeDefOrRef(writer, ((ModifierSig)type).Modifier);
			type = type.Next;
		}
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

	private void WritePrimitiveValue(DataWriter writer, ElementType et, object value)
	{
		switch (et)
		{
		case ElementType.Boolean:
			if (value is bool)
			{
				writer.WriteBoolean((bool)value);
				break;
			}
			helper.Error("Expected a Boolean constant");
			writer.WriteBoolean(value: false);
			break;
		case ElementType.Char:
			if (value is char)
			{
				writer.WriteUInt16((char)value);
				break;
			}
			helper.Error("Expected a Char constant");
			writer.WriteUInt16(0);
			break;
		case ElementType.I1:
			if (value is sbyte)
			{
				writer.WriteSByte((sbyte)value);
				break;
			}
			helper.Error("Expected a SByte constant");
			writer.WriteSByte(0);
			break;
		case ElementType.U1:
			if (value is byte)
			{
				writer.WriteByte((byte)value);
				break;
			}
			helper.Error("Expected a Byte constant");
			writer.WriteByte(0);
			break;
		case ElementType.I2:
			if (value is short)
			{
				writer.WriteInt16((short)value);
				break;
			}
			helper.Error("Expected an Int16 constant");
			writer.WriteInt16(0);
			break;
		case ElementType.U2:
			if (value is ushort)
			{
				writer.WriteUInt16((ushort)value);
				break;
			}
			helper.Error("Expected a UInt16 constant");
			writer.WriteUInt16(0);
			break;
		case ElementType.I4:
			if (value is int)
			{
				writer.WriteInt32((int)value);
				break;
			}
			helper.Error("Expected an Int32 constant");
			writer.WriteInt32(0);
			break;
		case ElementType.U4:
			if (value is uint)
			{
				writer.WriteUInt32((uint)value);
				break;
			}
			helper.Error("Expected a UInt32 constant");
			writer.WriteUInt32(0u);
			break;
		case ElementType.I8:
			if (value is long)
			{
				writer.WriteInt64((long)value);
				break;
			}
			helper.Error("Expected an Int64 constant");
			writer.WriteInt64(0L);
			break;
		case ElementType.U8:
			if (value is ulong)
			{
				writer.WriteUInt64((ulong)value);
				break;
			}
			helper.Error("Expected a UInt64 constant");
			writer.WriteUInt64(0uL);
			break;
		default:
			throw new InvalidOperationException();
		}
	}

	private void WriteTypeDefOrRef(DataWriter writer, ITypeDefOrRef tdr)
	{
		if (!CodedToken.TypeDefOrRef.Encode(systemMetadata.GetToken(tdr), out var codedToken))
		{
			helper.Error("Couldn't encode a TypeDefOrRef");
		}
		else
		{
			writer.WriteCompressedUInt32(codedToken);
		}
	}
}
