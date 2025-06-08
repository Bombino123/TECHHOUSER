using System;
using System.IO;

namespace dnlib.DotNet.Writer;

public struct SignatureWriter : IDisposable
{
	private readonly ISignatureWriterHelper helper;

	private RecursionCounter recursionCounter;

	private readonly MemoryStream outStream;

	private readonly DataWriter writer;

	private readonly bool disposeStream;

	public static byte[] Write(ISignatureWriterHelper helper, TypeSig typeSig)
	{
		using SignatureWriter signatureWriter = new SignatureWriter(helper);
		signatureWriter.Write(typeSig);
		return signatureWriter.GetResult();
	}

	internal static byte[] Write(ISignatureWriterHelper helper, TypeSig typeSig, DataWriterContext context)
	{
		using SignatureWriter signatureWriter = new SignatureWriter(helper, context);
		signatureWriter.Write(typeSig);
		return signatureWriter.GetResult();
	}

	public static byte[] Write(ISignatureWriterHelper helper, CallingConventionSig sig)
	{
		using SignatureWriter signatureWriter = new SignatureWriter(helper);
		signatureWriter.Write(sig);
		return signatureWriter.GetResult();
	}

	internal static byte[] Write(ISignatureWriterHelper helper, CallingConventionSig sig, DataWriterContext context)
	{
		using SignatureWriter signatureWriter = new SignatureWriter(helper, context);
		signatureWriter.Write(sig);
		return signatureWriter.GetResult();
	}

	private SignatureWriter(ISignatureWriterHelper helper)
	{
		this.helper = helper;
		recursionCounter = default(RecursionCounter);
		outStream = new MemoryStream();
		writer = new DataWriter(outStream);
		disposeStream = true;
	}

	private SignatureWriter(ISignatureWriterHelper helper, DataWriterContext context)
	{
		this.helper = helper;
		recursionCounter = default(RecursionCounter);
		outStream = context.OutStream;
		writer = context.Writer;
		disposeStream = false;
		outStream.SetLength(0L);
		outStream.Position = 0L;
	}

	private byte[] GetResult()
	{
		return outStream.ToArray();
	}

	private uint WriteCompressedUInt32(uint value)
	{
		return writer.WriteCompressedUInt32(helper, value);
	}

	private int WriteCompressedInt32(int value)
	{
		return writer.WriteCompressedInt32(helper, value);
	}

	private void Write(TypeSig typeSig)
	{
		if (typeSig == null)
		{
			helper.Error("TypeSig is null");
			writer.WriteByte(2);
			return;
		}
		if (!recursionCounter.Increment())
		{
			helper.Error("Infinite recursion");
			writer.WriteByte(2);
			return;
		}
		switch (typeSig.ElementType)
		{
		case ElementType.Void:
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
		case ElementType.R4:
		case ElementType.R8:
		case ElementType.String:
		case ElementType.TypedByRef:
		case ElementType.I:
		case ElementType.U:
		case ElementType.Object:
		case ElementType.Sentinel:
			writer.WriteByte((byte)typeSig.ElementType);
			break;
		case ElementType.Ptr:
		case ElementType.ByRef:
		case ElementType.SZArray:
		case ElementType.Pinned:
			writer.WriteByte((byte)typeSig.ElementType);
			Write(typeSig.Next);
			break;
		case ElementType.ValueType:
		case ElementType.Class:
			writer.WriteByte((byte)typeSig.ElementType);
			Write(((TypeDefOrRefSig)typeSig).TypeDefOrRef);
			break;
		case ElementType.Var:
		case ElementType.MVar:
			writer.WriteByte((byte)typeSig.ElementType);
			WriteCompressedUInt32(((GenericSig)typeSig).Number);
			break;
		case ElementType.Array:
		{
			writer.WriteByte((byte)typeSig.ElementType);
			ArraySig arraySig = (ArraySig)typeSig;
			Write(arraySig.Next);
			WriteCompressedUInt32(arraySig.Rank);
			if (arraySig.Rank != 0)
			{
				uint num = WriteCompressedUInt32((uint)arraySig.Sizes.Count);
				for (uint num3 = 0u; num3 < num; num3++)
				{
					WriteCompressedUInt32(arraySig.Sizes[(int)num3]);
				}
				num = WriteCompressedUInt32((uint)arraySig.LowerBounds.Count);
				for (uint num4 = 0u; num4 < num; num4++)
				{
					WriteCompressedInt32(arraySig.LowerBounds[(int)num4]);
				}
			}
			break;
		}
		case ElementType.GenericInst:
		{
			writer.WriteByte((byte)typeSig.ElementType);
			GenericInstSig genericInstSig = (GenericInstSig)typeSig;
			Write(genericInstSig.GenericType);
			uint num = WriteCompressedUInt32((uint)genericInstSig.GenericArguments.Count);
			for (uint num2 = 0u; num2 < num; num2++)
			{
				Write(genericInstSig.GenericArguments[(int)num2]);
			}
			break;
		}
		case ElementType.ValueArray:
			writer.WriteByte((byte)typeSig.ElementType);
			Write(typeSig.Next);
			WriteCompressedUInt32((typeSig as ValueArraySig).Size);
			break;
		case ElementType.FnPtr:
			writer.WriteByte((byte)typeSig.ElementType);
			Write((typeSig as FnPtrSig).Signature);
			break;
		case ElementType.CModReqd:
		case ElementType.CModOpt:
			writer.WriteByte((byte)typeSig.ElementType);
			Write((typeSig as ModifierSig).Modifier);
			Write(typeSig.Next);
			break;
		case ElementType.Module:
			writer.WriteByte((byte)typeSig.ElementType);
			WriteCompressedUInt32((typeSig as ModuleSig).Index);
			Write(typeSig.Next);
			break;
		default:
			helper.Error("Unknown or unsupported element type");
			writer.WriteByte(2);
			break;
		}
		recursionCounter.Decrement();
	}

	private void Write(ITypeDefOrRef tdr)
	{
		if (tdr == null)
		{
			helper.Error("TypeDefOrRef is null");
			WriteCompressedUInt32(0u);
			return;
		}
		uint num = helper.ToEncodedToken(tdr);
		if (num > 536870911)
		{
			helper.Error("Encoded token doesn't fit in 29 bits");
			num = 0u;
		}
		WriteCompressedUInt32(num);
	}

	private void Write(CallingConventionSig sig)
	{
		if (sig == null)
		{
			helper.Error("sig is null");
			return;
		}
		if (!recursionCounter.Increment())
		{
			helper.Error("Infinite recursion");
			return;
		}
		if (sig is MethodBaseSig sig2)
		{
			Write(sig2);
		}
		else if (sig is FieldSig sig3)
		{
			Write(sig3);
		}
		else if (sig is LocalSig sig4)
		{
			Write(sig4);
		}
		else if (sig is GenericInstMethodSig sig5)
		{
			Write(sig5);
		}
		else
		{
			helper.Error("Unknown calling convention sig");
			writer.WriteByte((byte)sig.GetCallingConvention());
		}
		recursionCounter.Decrement();
	}

	private void Write(MethodBaseSig sig)
	{
		if (sig == null)
		{
			helper.Error("sig is null");
			return;
		}
		if (!recursionCounter.Increment())
		{
			helper.Error("Infinite recursion");
			return;
		}
		writer.WriteByte((byte)sig.GetCallingConvention());
		if (sig.Generic)
		{
			WriteCompressedUInt32(sig.GenParamCount);
		}
		uint num = (uint)sig.Params.Count;
		if (sig.ParamsAfterSentinel != null)
		{
			num += (uint)sig.ParamsAfterSentinel.Count;
		}
		uint num2 = WriteCompressedUInt32(num);
		Write(sig.RetType);
		for (uint num3 = 0u; num3 < num2 && num3 < (uint)sig.Params.Count; num3++)
		{
			Write(sig.Params[(int)num3]);
		}
		if (sig.ParamsAfterSentinel != null && sig.ParamsAfterSentinel.Count > 0)
		{
			writer.WriteByte(65);
			uint num4 = 0u;
			uint num5 = (uint)sig.Params.Count;
			while (num4 < (uint)sig.ParamsAfterSentinel.Count && num5 < num2)
			{
				Write(sig.ParamsAfterSentinel[(int)num4]);
				num4++;
				num5++;
			}
		}
		recursionCounter.Decrement();
	}

	private void Write(FieldSig sig)
	{
		if (sig == null)
		{
			helper.Error("sig is null");
			return;
		}
		if (!recursionCounter.Increment())
		{
			helper.Error("Infinite recursion");
			return;
		}
		writer.WriteByte((byte)sig.GetCallingConvention());
		Write(sig.Type);
		recursionCounter.Decrement();
	}

	private void Write(LocalSig sig)
	{
		if (sig == null)
		{
			helper.Error("sig is null");
			return;
		}
		if (!recursionCounter.Increment())
		{
			helper.Error("Infinite recursion");
			return;
		}
		writer.WriteByte((byte)sig.GetCallingConvention());
		uint num = WriteCompressedUInt32((uint)sig.Locals.Count);
		if (num >= 65536)
		{
			helper.Error("Too many locals, max number of locals is 65535 (0xFFFF)");
		}
		for (uint num2 = 0u; num2 < num; num2++)
		{
			Write(sig.Locals[(int)num2]);
		}
		recursionCounter.Decrement();
	}

	private void Write(GenericInstMethodSig sig)
	{
		if (sig == null)
		{
			helper.Error("sig is null");
			return;
		}
		if (!recursionCounter.Increment())
		{
			helper.Error("Infinite recursion");
			return;
		}
		writer.WriteByte((byte)sig.GetCallingConvention());
		uint num = WriteCompressedUInt32((uint)sig.GenericArguments.Count);
		for (uint num2 = 0u; num2 < num; num2++)
		{
			Write(sig.GenericArguments[(int)num2]);
		}
		recursionCounter.Decrement();
	}

	public void Dispose()
	{
		if (disposeStream && outStream != null)
		{
			outStream.Dispose();
		}
	}
}
