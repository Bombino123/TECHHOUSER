using System;
using System.Collections.Generic;
using dnlib.DotNet.Emit;

namespace dnlib.DotNet.Writer;

public sealed class MethodBodyWriter : MethodBodyWriterBase
{
	private readonly ITokenProvider helper;

	private CilBody cilBody;

	private bool keepMaxStack;

	private uint codeSize;

	private uint maxStack;

	private byte[] code;

	private byte[] extraSections;

	private uint localVarSigTok;

	public byte[] Code => code;

	public byte[] ExtraSections => extraSections;

	public uint LocalVarSigTok => localVarSigTok;

	public MethodBodyWriter(ITokenProvider helper, MethodDef method)
		: this(helper, method, keepMaxStack: false)
	{
	}

	public MethodBodyWriter(ITokenProvider helper, MethodDef method, bool keepMaxStack)
		: base(method.Body.Instructions, method.Body.ExceptionHandlers)
	{
		this.helper = helper;
		cilBody = method.Body;
		this.keepMaxStack = keepMaxStack;
	}

	public MethodBodyWriter(ITokenProvider helper, CilBody cilBody)
		: this(helper, cilBody, keepMaxStack: false)
	{
	}

	public MethodBodyWriter(ITokenProvider helper, CilBody cilBody, bool keepMaxStack)
		: base(cilBody.Instructions, cilBody.ExceptionHandlers)
	{
		this.helper = helper;
		this.cilBody = cilBody;
		this.keepMaxStack = keepMaxStack;
	}

	internal MethodBodyWriter(ITokenProvider helper)
	{
		this.helper = helper;
	}

	internal void Reset(CilBody cilBody, bool keepMaxStack)
	{
		Reset(cilBody.Instructions, cilBody.ExceptionHandlers);
		this.cilBody = cilBody;
		this.keepMaxStack = keepMaxStack;
		codeSize = 0u;
		maxStack = 0u;
		code = null;
		extraSections = null;
		localVarSigTok = 0u;
	}

	public void Write()
	{
		codeSize = InitializeInstructionOffsets();
		maxStack = (keepMaxStack ? cilBody.MaxStack : GetMaxStack());
		if (NeedFatHeader())
		{
			WriteFatHeader();
		}
		else
		{
			WriteTinyHeader();
		}
		if (exceptionHandlers.Count > 0)
		{
			WriteExceptionHandlers();
		}
	}

	public byte[] GetFullMethodBody()
	{
		int num = Utils.AlignUp(code.Length, 4u) - code.Length;
		byte[] array = new byte[code.Length + ((extraSections != null) ? (num + extraSections.Length) : 0)];
		Array.Copy(code, 0, array, 0, code.Length);
		if (extraSections != null)
		{
			Array.Copy(extraSections, 0, array, code.Length + num, extraSections.Length);
		}
		return array;
	}

	private bool NeedFatHeader()
	{
		if (codeSize <= 63 && exceptionHandlers.Count <= 0 && !cilBody.HasVariables)
		{
			return maxStack > 8;
		}
		return true;
	}

	private void WriteFatHeader()
	{
		if (maxStack > 65535)
		{
			Error("MaxStack is too big");
			maxStack = 65535u;
		}
		ushort num = 12291;
		if (exceptionHandlers.Count > 0)
		{
			num = (ushort)(num | 8u);
		}
		if (cilBody.InitLocals)
		{
			num = (ushort)(num | 0x10u);
		}
		code = new byte[12 + codeSize];
		ArrayWriter writer = new ArrayWriter(code);
		writer.WriteUInt16(num);
		writer.WriteUInt16((ushort)maxStack);
		writer.WriteUInt32(codeSize);
		writer.WriteUInt32(localVarSigTok = helper.GetToken(GetLocals(), cilBody.LocalVarSigTok).Raw);
		if (WriteInstructions(ref writer) != codeSize)
		{
			Error("Didn't write all code bytes");
		}
	}

	private IList<TypeSig> GetLocals()
	{
		TypeSig[] array = new TypeSig[cilBody.Variables.Count];
		for (int i = 0; i < cilBody.Variables.Count; i++)
		{
			array[i] = cilBody.Variables[i].Type;
		}
		return array;
	}

	private void WriteTinyHeader()
	{
		localVarSigTok = 0u;
		code = new byte[1 + codeSize];
		ArrayWriter writer = new ArrayWriter(code);
		writer.WriteByte((byte)((codeSize << 2) | 2u));
		if (WriteInstructions(ref writer) != codeSize)
		{
			Error("Didn't write all code bytes");
		}
	}

	private void WriteExceptionHandlers()
	{
		if (NeedFatExceptionClauses())
		{
			extraSections = WriteFatExceptionClauses();
		}
		else
		{
			extraSections = WriteSmallExceptionClauses();
		}
	}

	private bool NeedFatExceptionClauses()
	{
		IList<ExceptionHandler> list = exceptionHandlers;
		if (list.Count > 20)
		{
			return true;
		}
		for (int i = 0; i < list.Count; i++)
		{
			ExceptionHandler exceptionHandler = list[i];
			if (!FitsInSmallExceptionClause(exceptionHandler.TryStart, exceptionHandler.TryEnd))
			{
				return true;
			}
			if (!FitsInSmallExceptionClause(exceptionHandler.HandlerStart, exceptionHandler.HandlerEnd))
			{
				return true;
			}
		}
		return false;
	}

	private bool FitsInSmallExceptionClause(Instruction start, Instruction end)
	{
		uint offset = GetOffset2(start);
		uint offset2 = GetOffset2(end);
		if (offset2 < offset)
		{
			return false;
		}
		if (offset <= 65535)
		{
			return offset2 - offset <= 255;
		}
		return false;
	}

	private uint GetOffset2(Instruction instr)
	{
		if (instr == null)
		{
			return codeSize;
		}
		return GetOffset(instr);
	}

	private byte[] WriteFatExceptionClauses()
	{
		IList<ExceptionHandler> list = exceptionHandlers;
		int num = list.Count;
		if (num > 699050)
		{
			Error("Too many exception handlers");
			num = 699050;
		}
		byte[] array = new byte[num * 24 + 4];
		ArrayWriter arrayWriter = new ArrayWriter(array);
		arrayWriter.WriteUInt32((uint)(num * 24 + 4 << 8) | 0x41u);
		for (int i = 0; i < num; i++)
		{
			ExceptionHandler exceptionHandler = list[i];
			arrayWriter.WriteUInt32((uint)exceptionHandler.HandlerType);
			uint offset = GetOffset2(exceptionHandler.TryStart);
			uint offset2 = GetOffset2(exceptionHandler.TryEnd);
			if (offset2 <= offset)
			{
				Error("Exception handler: TryEnd <= TryStart");
			}
			arrayWriter.WriteUInt32(offset);
			arrayWriter.WriteUInt32(offset2 - offset);
			offset = GetOffset2(exceptionHandler.HandlerStart);
			offset2 = GetOffset2(exceptionHandler.HandlerEnd);
			if (offset2 <= offset)
			{
				Error("Exception handler: HandlerEnd <= HandlerStart");
			}
			arrayWriter.WriteUInt32(offset);
			arrayWriter.WriteUInt32(offset2 - offset);
			if (exceptionHandler.IsCatch)
			{
				arrayWriter.WriteUInt32(helper.GetToken(exceptionHandler.CatchType).Raw);
			}
			else if (exceptionHandler.IsFilter)
			{
				arrayWriter.WriteUInt32(GetOffset2(exceptionHandler.FilterStart));
			}
			else
			{
				arrayWriter.WriteInt32(0);
			}
		}
		if (arrayWriter.Position != array.Length)
		{
			throw new InvalidOperationException();
		}
		return array;
	}

	private byte[] WriteSmallExceptionClauses()
	{
		IList<ExceptionHandler> list = exceptionHandlers;
		int num = list.Count;
		if (num > 20)
		{
			Error("Too many exception handlers");
			num = 20;
		}
		byte[] array = new byte[num * 12 + 4];
		ArrayWriter arrayWriter = new ArrayWriter(array);
		arrayWriter.WriteUInt32((uint)(num * 12 + 4 << 8) | 1u);
		for (int i = 0; i < num; i++)
		{
			ExceptionHandler exceptionHandler = list[i];
			arrayWriter.WriteUInt16((ushort)exceptionHandler.HandlerType);
			uint offset = GetOffset2(exceptionHandler.TryStart);
			uint offset2 = GetOffset2(exceptionHandler.TryEnd);
			if (offset2 <= offset)
			{
				Error("Exception handler: TryEnd <= TryStart");
			}
			arrayWriter.WriteUInt16((ushort)offset);
			arrayWriter.WriteByte((byte)(offset2 - offset));
			offset = GetOffset2(exceptionHandler.HandlerStart);
			offset2 = GetOffset2(exceptionHandler.HandlerEnd);
			if (offset2 <= offset)
			{
				Error("Exception handler: HandlerEnd <= HandlerStart");
			}
			arrayWriter.WriteUInt16((ushort)offset);
			arrayWriter.WriteByte((byte)(offset2 - offset));
			if (exceptionHandler.IsCatch)
			{
				arrayWriter.WriteUInt32(helper.GetToken(exceptionHandler.CatchType).Raw);
			}
			else if (exceptionHandler.IsFilter)
			{
				arrayWriter.WriteUInt32(GetOffset2(exceptionHandler.FilterStart));
			}
			else
			{
				arrayWriter.WriteInt32(0);
			}
		}
		if (arrayWriter.Position != array.Length)
		{
			throw new InvalidOperationException();
		}
		return array;
	}

	protected override void ErrorImpl(string message)
	{
		helper.Error(message);
	}

	protected override void WriteInlineField(ref ArrayWriter writer, Instruction instr)
	{
		writer.WriteUInt32(helper.GetToken(instr.Operand).Raw);
	}

	protected override void WriteInlineMethod(ref ArrayWriter writer, Instruction instr)
	{
		writer.WriteUInt32(helper.GetToken(instr.Operand).Raw);
	}

	protected override void WriteInlineSig(ref ArrayWriter writer, Instruction instr)
	{
		writer.WriteUInt32(helper.GetToken(instr.Operand).Raw);
	}

	protected override void WriteInlineString(ref ArrayWriter writer, Instruction instr)
	{
		writer.WriteUInt32(helper.GetToken(instr.Operand).Raw);
	}

	protected override void WriteInlineTok(ref ArrayWriter writer, Instruction instr)
	{
		writer.WriteUInt32(helper.GetToken(instr.Operand).Raw);
	}

	protected override void WriteInlineType(ref ArrayWriter writer, Instruction instr)
	{
		writer.WriteUInt32(helper.GetToken(instr.Operand).Raw);
	}
}
