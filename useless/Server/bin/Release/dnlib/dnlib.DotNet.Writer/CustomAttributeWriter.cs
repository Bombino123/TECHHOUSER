using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace dnlib.DotNet.Writer;

public struct CustomAttributeWriter : IDisposable
{
	private readonly ICustomAttributeWriterHelper helper;

	private RecursionCounter recursionCounter;

	private readonly StringBuilder sb;

	private readonly MemoryStream outStream;

	private readonly DataWriter writer;

	private readonly bool disposeStream;

	private GenericArguments genericArguments;

	public static byte[] Write(ICustomAttributeWriterHelper helper, CustomAttribute ca)
	{
		using CustomAttributeWriter customAttributeWriter = new CustomAttributeWriter(helper);
		customAttributeWriter.Write(ca);
		return customAttributeWriter.GetResult();
	}

	internal static byte[] Write(ICustomAttributeWriterHelper helper, CustomAttribute ca, DataWriterContext context)
	{
		using CustomAttributeWriter customAttributeWriter = new CustomAttributeWriter(helper, context);
		customAttributeWriter.Write(ca);
		return customAttributeWriter.GetResult();
	}

	internal static byte[] Write(ICustomAttributeWriterHelper helper, IList<CANamedArgument> namedArgs)
	{
		using CustomAttributeWriter customAttributeWriter = new CustomAttributeWriter(helper);
		customAttributeWriter.Write(namedArgs);
		return customAttributeWriter.GetResult();
	}

	internal static byte[] Write(ICustomAttributeWriterHelper helper, IList<CANamedArgument> namedArgs, DataWriterContext context)
	{
		using CustomAttributeWriter customAttributeWriter = new CustomAttributeWriter(helper, context);
		customAttributeWriter.Write(namedArgs);
		return customAttributeWriter.GetResult();
	}

	private CustomAttributeWriter(ICustomAttributeWriterHelper helper)
	{
		this.helper = helper;
		recursionCounter = default(RecursionCounter);
		sb = new StringBuilder();
		outStream = new MemoryStream();
		writer = new DataWriter(outStream);
		genericArguments = null;
		disposeStream = true;
	}

	private CustomAttributeWriter(ICustomAttributeWriterHelper helper, DataWriterContext context)
	{
		this.helper = helper;
		recursionCounter = default(RecursionCounter);
		sb = new StringBuilder();
		outStream = context.OutStream;
		writer = context.Writer;
		genericArguments = null;
		disposeStream = false;
		outStream.SetLength(0L);
		outStream.Position = 0L;
	}

	private byte[] GetResult()
	{
		return outStream.ToArray();
	}

	private void Write(CustomAttribute ca)
	{
		if (ca == null)
		{
			helper.Error("The custom attribute is null");
			return;
		}
		if (ca.IsRawBlob)
		{
			if ((ca.ConstructorArguments != null && ca.ConstructorArguments.Count > 0) || (ca.NamedArguments != null && ca.NamedArguments.Count > 0))
			{
				helper.Error("Raw custom attribute contains arguments and/or named arguments");
			}
			writer.WriteBytes(ca.RawData);
			return;
		}
		if (ca.Constructor == null)
		{
			helper.Error("Custom attribute ctor is null");
			return;
		}
		MethodSig methodSig = GetMethodSig(ca.Constructor);
		if (methodSig == null)
		{
			helper.Error("Custom attribute ctor's method signature is invalid");
			return;
		}
		if (ca.ConstructorArguments.Count != methodSig.Params.Count)
		{
			helper.Error("Custom attribute arguments count != method sig arguments count");
		}
		if (methodSig.ParamsAfterSentinel != null && methodSig.ParamsAfterSentinel.Count > 0)
		{
			helper.Error("Custom attribute ctor has parameters after the sentinel");
		}
		if (ca.NamedArguments.Count > 65535)
		{
			helper.Error("Custom attribute has too many named arguments");
		}
		if (ca.Constructor is MemberRef { Class: TypeSpec { TypeSig: GenericInstSig typeSig } })
		{
			genericArguments = new GenericArguments();
			genericArguments.PushTypeArgs(typeSig.GenericArguments);
		}
		writer.WriteUInt16(1);
		int num = Math.Min(methodSig.Params.Count, ca.ConstructorArguments.Count);
		for (int i = 0; i < num; i++)
		{
			WriteValue(FixTypeSig(methodSig.Params[i]), ca.ConstructorArguments[i]);
		}
		int num2 = Math.Min(65535, ca.NamedArguments.Count);
		writer.WriteUInt16((ushort)num2);
		for (int j = 0; j < num2; j++)
		{
			Write(ca.NamedArguments[j]);
		}
	}

	private void Write(IList<CANamedArgument> namedArgs)
	{
		if (namedArgs == null || namedArgs.Count > 536870911)
		{
			helper.Error("Too many custom attribute named arguments");
			namedArgs = Array2.Empty<CANamedArgument>();
		}
		writer.WriteCompressedUInt32((uint)namedArgs.Count);
		for (int i = 0; i < namedArgs.Count; i++)
		{
			Write(namedArgs[i]);
		}
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

	private void WriteValue(TypeSig argType, CAArgument value)
	{
		if (argType == null || value.Type == null)
		{
			helper.Error("Custom attribute argument type is null");
			return;
		}
		if (!recursionCounter.Increment())
		{
			helper.Error("Infinite recursion");
			return;
		}
		if (argType is SZArraySig arrayType)
		{
			IList<CAArgument> list = value.Value as IList<CAArgument>;
			if (list == null && value.Value != null)
			{
				helper.Error("CAArgument.Value is not null or an array");
			}
			WriteArrayValue(arrayType, list);
		}
		else
		{
			WriteElem(argType, value);
		}
		recursionCounter.Decrement();
	}

	private void WriteArrayValue(SZArraySig arrayType, IList<CAArgument> args)
	{
		if (arrayType == null)
		{
			helper.Error("Custom attribute: Array type is null");
			return;
		}
		if (args == null)
		{
			writer.WriteUInt32(uint.MaxValue);
			return;
		}
		writer.WriteUInt32((uint)args.Count);
		TypeSig argType = FixTypeSig(arrayType.Next);
		for (int i = 0; i < args.Count; i++)
		{
			WriteValue(argType, args[i]);
		}
	}

	private bool VerifyTypeAndValue(CAArgument value, ElementType etype)
	{
		if (!VerifyType(value.Type, etype))
		{
			helper.Error("Custom attribute arg type != value.Type");
			return false;
		}
		if (!VerifyValue(value.Value, etype))
		{
			helper.Error("Custom attribute value.Value's type != value.Type");
			return false;
		}
		return true;
	}

	private bool VerifyTypeAndValue(CAArgument value, ElementType etype, Type valueType)
	{
		if (!VerifyType(value.Type, etype))
		{
			helper.Error("Custom attribute arg type != value.Type");
			return false;
		}
		if (value.Value != null)
		{
			return value.Value.GetType() == valueType;
		}
		return true;
	}

	private static bool VerifyType(TypeSig type, ElementType etype)
	{
		type = type.RemoveModifiers();
		if (type != null)
		{
			if (etype != type.ElementType)
			{
				return type.ElementType == ElementType.ValueType;
			}
			return true;
		}
		return false;
	}

	private static bool VerifyValue(object o, ElementType etype)
	{
		if (o == null)
		{
			return false;
		}
		return Type.GetTypeCode(o.GetType()) switch
		{
			TypeCode.Boolean => etype == ElementType.Boolean, 
			TypeCode.Char => etype == ElementType.Char, 
			TypeCode.SByte => etype == ElementType.I1, 
			TypeCode.Byte => etype == ElementType.U1, 
			TypeCode.Int16 => etype == ElementType.I2, 
			TypeCode.UInt16 => etype == ElementType.U2, 
			TypeCode.Int32 => etype == ElementType.I4, 
			TypeCode.UInt32 => etype == ElementType.U4, 
			TypeCode.Int64 => etype == ElementType.I8, 
			TypeCode.UInt64 => etype == ElementType.U8, 
			TypeCode.Single => etype == ElementType.R4, 
			TypeCode.Double => etype == ElementType.R8, 
			_ => false, 
		};
	}

	private static ulong ToUInt64(object o)
	{
		ToUInt64(o, out var result);
		return result;
	}

	private static bool ToUInt64(object o, out ulong result)
	{
		if (o == null)
		{
			result = 0uL;
			return false;
		}
		switch (Type.GetTypeCode(o.GetType()))
		{
		case TypeCode.Boolean:
			result = (ulong)(((bool)o) ? 1 : 0);
			return true;
		case TypeCode.Char:
			result = (char)o;
			return true;
		case TypeCode.SByte:
			result = (ulong)(sbyte)o;
			return true;
		case TypeCode.Byte:
			result = (byte)o;
			return true;
		case TypeCode.Int16:
			result = (ulong)(short)o;
			return true;
		case TypeCode.UInt16:
			result = (ushort)o;
			return true;
		case TypeCode.Int32:
			result = (ulong)(int)o;
			return true;
		case TypeCode.UInt32:
			result = (uint)o;
			return true;
		case TypeCode.Int64:
			result = (ulong)(long)o;
			return true;
		case TypeCode.UInt64:
			result = (ulong)o;
			return true;
		case TypeCode.Single:
			result = (ulong)(float)o;
			return true;
		case TypeCode.Double:
			result = (ulong)(double)o;
			return true;
		default:
			result = 0uL;
			return false;
		}
	}

	private static double ToDouble(object o)
	{
		ToDouble(o, out var result);
		return result;
	}

	private static bool ToDouble(object o, out double result)
	{
		if (o == null)
		{
			result = double.NaN;
			return false;
		}
		switch (Type.GetTypeCode(o.GetType()))
		{
		case TypeCode.Boolean:
			result = (((bool)o) ? 1 : 0);
			return true;
		case TypeCode.Char:
			result = (int)(char)o;
			return true;
		case TypeCode.SByte:
			result = (sbyte)o;
			return true;
		case TypeCode.Byte:
			result = (int)(byte)o;
			return true;
		case TypeCode.Int16:
			result = (short)o;
			return true;
		case TypeCode.UInt16:
			result = (int)(ushort)o;
			return true;
		case TypeCode.Int32:
			result = (int)o;
			return true;
		case TypeCode.UInt32:
			result = (uint)o;
			return true;
		case TypeCode.Int64:
			result = (long)o;
			return true;
		case TypeCode.UInt64:
			result = (ulong)o;
			return true;
		case TypeCode.Single:
			result = (float)o;
			return true;
		case TypeCode.Double:
			result = (double)o;
			return true;
		default:
			result = double.NaN;
			return false;
		}
	}

	private void WriteElem(TypeSig argType, CAArgument value)
	{
		if (argType == null)
		{
			helper.Error("Custom attribute: Arg type is null");
			argType = value.Type;
			if (argType == null)
			{
				return;
			}
		}
		if (!recursionCounter.Increment())
		{
			helper.Error("Infinite recursion");
			return;
		}
		switch (argType.ElementType)
		{
		case ElementType.Boolean:
			if (!VerifyTypeAndValue(value, ElementType.Boolean))
			{
				writer.WriteBoolean(ToUInt64(value.Value) != 0);
			}
			else
			{
				writer.WriteBoolean((bool)value.Value);
			}
			break;
		case ElementType.Char:
			if (!VerifyTypeAndValue(value, ElementType.Char))
			{
				writer.WriteUInt16((ushort)ToUInt64(value.Value));
			}
			else
			{
				writer.WriteUInt16((char)value.Value);
			}
			break;
		case ElementType.I1:
			if (!VerifyTypeAndValue(value, ElementType.I1))
			{
				writer.WriteSByte((sbyte)ToUInt64(value.Value));
			}
			else
			{
				writer.WriteSByte((sbyte)value.Value);
			}
			break;
		case ElementType.U1:
			if (!VerifyTypeAndValue(value, ElementType.U1))
			{
				writer.WriteByte((byte)ToUInt64(value.Value));
			}
			else
			{
				writer.WriteByte((byte)value.Value);
			}
			break;
		case ElementType.I2:
			if (!VerifyTypeAndValue(value, ElementType.I2))
			{
				writer.WriteInt16((short)ToUInt64(value.Value));
			}
			else
			{
				writer.WriteInt16((short)value.Value);
			}
			break;
		case ElementType.U2:
			if (!VerifyTypeAndValue(value, ElementType.U2))
			{
				writer.WriteUInt16((ushort)ToUInt64(value.Value));
			}
			else
			{
				writer.WriteUInt16((ushort)value.Value);
			}
			break;
		case ElementType.I4:
			if (!VerifyTypeAndValue(value, ElementType.I4))
			{
				writer.WriteInt32((int)ToUInt64(value.Value));
			}
			else
			{
				writer.WriteInt32((int)value.Value);
			}
			break;
		case ElementType.U4:
			if (!VerifyTypeAndValue(value, ElementType.U4))
			{
				writer.WriteUInt32((uint)ToUInt64(value.Value));
			}
			else
			{
				writer.WriteUInt32((uint)value.Value);
			}
			break;
		case ElementType.I8:
			if (!VerifyTypeAndValue(value, ElementType.I8))
			{
				writer.WriteInt64((long)ToUInt64(value.Value));
			}
			else
			{
				writer.WriteInt64((long)value.Value);
			}
			break;
		case ElementType.U8:
			if (!VerifyTypeAndValue(value, ElementType.U8))
			{
				writer.WriteUInt64(ToUInt64(value.Value));
			}
			else
			{
				writer.WriteUInt64((ulong)value.Value);
			}
			break;
		case ElementType.R4:
			if (!VerifyTypeAndValue(value, ElementType.R4))
			{
				writer.WriteSingle((float)ToDouble(value.Value));
			}
			else
			{
				writer.WriteSingle((float)value.Value);
			}
			break;
		case ElementType.R8:
			if (!VerifyTypeAndValue(value, ElementType.R8))
			{
				writer.WriteDouble(ToDouble(value.Value));
			}
			else
			{
				writer.WriteDouble((double)value.Value);
			}
			break;
		case ElementType.String:
			if (VerifyTypeAndValue(value, ElementType.String, typeof(UTF8String)))
			{
				WriteUTF8String((UTF8String)value.Value);
			}
			else if (VerifyTypeAndValue(value, ElementType.String, typeof(string)))
			{
				WriteUTF8String((string)value.Value);
			}
			else
			{
				WriteUTF8String(UTF8String.Empty);
			}
			break;
		case ElementType.ValueType:
		{
			ITypeDefOrRef typeDefOrRef = ((TypeDefOrRefSig)argType).TypeDefOrRef;
			TypeSig enumUnderlyingType = GetEnumUnderlyingType(argType);
			if (enumUnderlyingType != null)
			{
				WriteElem(enumUnderlyingType, value);
			}
			else if (!(typeDefOrRef is TypeRef) || !TryWriteEnumUnderlyingTypeValue(value.Value))
			{
				helper.Error("Custom attribute value is not an enum");
			}
			break;
		}
		case ElementType.Class:
		{
			ITypeDefOrRef typeDefOrRef = ((TypeDefOrRefSig)argType).TypeDefOrRef;
			if (CheckCorLibType(argType, "Type"))
			{
				if (CheckCorLibType(value.Type, "Type"))
				{
					if (value.Value is TypeSig type)
					{
						WriteType(type);
						break;
					}
					if (value.Value == null)
					{
						WriteUTF8String(null);
						break;
					}
					helper.Error("Custom attribute value is not a type");
					WriteUTF8String(UTF8String.Empty);
				}
				else
				{
					helper.Error("Custom attribute value type is not System.Type");
					WriteUTF8String(UTF8String.Empty);
				}
				break;
			}
			if (typeDefOrRef is TypeRef && TryWriteEnumUnderlyingTypeValue(value.Value))
			{
				break;
			}
			goto default;
		}
		case ElementType.SZArray:
			WriteValue(argType, value);
			break;
		case ElementType.Object:
			WriteFieldOrPropType(value.Type);
			WriteElem(value.Type, value);
			break;
		default:
			helper.Error("Invalid or unsupported element type in custom attribute");
			break;
		}
		recursionCounter.Decrement();
	}

	private bool TryWriteEnumUnderlyingTypeValue(object o)
	{
		if (o == null)
		{
			return false;
		}
		switch (Type.GetTypeCode(o.GetType()))
		{
		case TypeCode.Boolean:
			writer.WriteBoolean((bool)o);
			break;
		case TypeCode.Char:
			writer.WriteUInt16((char)o);
			break;
		case TypeCode.SByte:
			writer.WriteSByte((sbyte)o);
			break;
		case TypeCode.Byte:
			writer.WriteByte((byte)o);
			break;
		case TypeCode.Int16:
			writer.WriteInt16((short)o);
			break;
		case TypeCode.UInt16:
			writer.WriteUInt16((ushort)o);
			break;
		case TypeCode.Int32:
			writer.WriteInt32((int)o);
			break;
		case TypeCode.UInt32:
			writer.WriteUInt32((uint)o);
			break;
		case TypeCode.Int64:
			writer.WriteInt64((long)o);
			break;
		case TypeCode.UInt64:
			writer.WriteUInt64((ulong)o);
			break;
		default:
			return false;
		}
		return true;
	}

	private static TypeSig GetEnumUnderlyingType(TypeSig type)
	{
		return GetEnumTypeDef(type)?.GetEnumUnderlyingType().RemoveModifiers();
	}

	private static TypeDef GetEnumTypeDef(TypeSig type)
	{
		if (type == null)
		{
			return null;
		}
		TypeDef typeDef = GetTypeDef(type);
		if (typeDef == null)
		{
			return null;
		}
		if (!typeDef.IsEnum)
		{
			return null;
		}
		return typeDef;
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

	private void Write(CANamedArgument namedArg)
	{
		if (namedArg == null)
		{
			helper.Error("Custom attribute named arg is null");
			return;
		}
		if (!recursionCounter.Increment())
		{
			helper.Error("Infinite recursion");
			return;
		}
		if (namedArg.IsProperty)
		{
			writer.WriteByte(84);
		}
		else
		{
			writer.WriteByte(83);
		}
		WriteFieldOrPropType(namedArg.Type);
		WriteUTF8String(namedArg.Name);
		WriteValue(namedArg.Type, namedArg.Argument);
		recursionCounter.Decrement();
	}

	private void WriteFieldOrPropType(TypeSig type)
	{
		type = type.RemoveModifiers();
		if (type == null)
		{
			helper.Error("Custom attribute: Field/property type is null");
			return;
		}
		if (!recursionCounter.Increment())
		{
			helper.Error("Infinite recursion");
			return;
		}
		switch (type.ElementType)
		{
		case ElementType.Boolean:
			writer.WriteByte(2);
			break;
		case ElementType.Char:
			writer.WriteByte(3);
			break;
		case ElementType.I1:
			writer.WriteByte(4);
			break;
		case ElementType.U1:
			writer.WriteByte(5);
			break;
		case ElementType.I2:
			writer.WriteByte(6);
			break;
		case ElementType.U2:
			writer.WriteByte(7);
			break;
		case ElementType.I4:
			writer.WriteByte(8);
			break;
		case ElementType.U4:
			writer.WriteByte(9);
			break;
		case ElementType.I8:
			writer.WriteByte(10);
			break;
		case ElementType.U8:
			writer.WriteByte(11);
			break;
		case ElementType.R4:
			writer.WriteByte(12);
			break;
		case ElementType.R8:
			writer.WriteByte(13);
			break;
		case ElementType.String:
			writer.WriteByte(14);
			break;
		case ElementType.Object:
			writer.WriteByte(81);
			break;
		case ElementType.SZArray:
			writer.WriteByte(29);
			WriteFieldOrPropType(type.Next);
			break;
		case ElementType.Class:
		{
			ITypeDefOrRef typeDefOrRef = ((TypeDefOrRefSig)type).TypeDefOrRef;
			if (CheckCorLibType(type, "Type"))
			{
				writer.WriteByte(80);
				break;
			}
			if (typeDefOrRef is TypeRef)
			{
				writer.WriteByte(85);
				WriteType(typeDefOrRef);
				break;
			}
			goto default;
		}
		case ElementType.ValueType:
		{
			ITypeDefOrRef typeDefOrRef = ((TypeDefOrRefSig)type).TypeDefOrRef;
			if (GetEnumTypeDef(type) != null || typeDefOrRef is TypeRef)
			{
				writer.WriteByte(85);
				WriteType(typeDefOrRef);
			}
			else
			{
				helper.Error("Custom attribute type doesn't seem to be an enum.");
				writer.WriteByte(85);
				WriteType(typeDefOrRef);
			}
			break;
		}
		default:
			helper.Error("Custom attribute: Invalid type");
			writer.WriteByte(byte.MaxValue);
			break;
		}
		recursionCounter.Decrement();
	}

	private void WriteType(IType type)
	{
		if (type == null)
		{
			helper.Error("Custom attribute: Type is null");
			WriteUTF8String(UTF8String.Empty);
		}
		else
		{
			sb.Length = 0;
			WriteUTF8String(FullNameFactory.AssemblyQualifiedName(type, helper, sb));
		}
	}

	private static bool CheckCorLibType(TypeSig ts, string name)
	{
		if (!(ts is TypeDefOrRefSig typeDefOrRefSig))
		{
			return false;
		}
		return CheckCorLibType(typeDefOrRefSig.TypeDefOrRef, name);
	}

	private static bool CheckCorLibType(ITypeDefOrRef tdr, string name)
	{
		if (tdr == null)
		{
			return false;
		}
		if (!tdr.DefinitionAssembly.IsCorLib())
		{
			return false;
		}
		if (tdr is TypeSpec)
		{
			return false;
		}
		if (tdr.TypeName == name)
		{
			return tdr.Namespace == "System";
		}
		return false;
	}

	private static MethodSig GetMethodSig(ICustomAttributeType ctor)
	{
		return ctor?.MethodSig;
	}

	private void WriteUTF8String(UTF8String s)
	{
		if ((object)s == null || s.Data == null)
		{
			writer.WriteByte(byte.MaxValue);
			return;
		}
		writer.WriteCompressedUInt32((uint)s.Data.Length);
		writer.WriteBytes(s.Data);
	}

	public void Dispose()
	{
		if (disposeStream && outStream != null)
		{
			outStream.Dispose();
		}
	}
}
