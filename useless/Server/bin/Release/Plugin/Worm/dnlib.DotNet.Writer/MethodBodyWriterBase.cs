using System.Collections.Generic;
using System.Runtime.InteropServices;
using dnlib.DotNet.Emit;

namespace dnlib.DotNet.Writer;

[ComVisible(true)]
public abstract class MethodBodyWriterBase
{
	protected IList<Instruction> instructions;

	protected IList<ExceptionHandler> exceptionHandlers;

	private readonly Dictionary<Instruction, uint> offsets = new Dictionary<Instruction, uint>();

	private uint firstInstructionOffset;

	private int errors;

	private MaxStackCalculator maxStackCalculator = MaxStackCalculator.Create();

	public bool ErrorDetected => errors > 0;

	internal MethodBodyWriterBase()
	{
	}

	protected MethodBodyWriterBase(IList<Instruction> instructions, IList<ExceptionHandler> exceptionHandlers)
	{
		this.instructions = instructions;
		this.exceptionHandlers = exceptionHandlers;
	}

	internal void Reset(IList<Instruction> instructions, IList<ExceptionHandler> exceptionHandlers)
	{
		this.instructions = instructions;
		this.exceptionHandlers = exceptionHandlers;
		offsets.Clear();
		firstInstructionOffset = 0u;
		errors = 0;
	}

	protected void Error(string message)
	{
		errors++;
		ErrorImpl(message);
	}

	protected virtual void ErrorImpl(string message)
	{
	}

	protected uint GetMaxStack()
	{
		if (instructions.Count == 0)
		{
			return 0u;
		}
		maxStackCalculator.Reset(instructions, exceptionHandlers);
		if (!maxStackCalculator.Calculate(out var maxStack))
		{
			Error("Error calculating max stack value. If the method's obfuscated, set CilBody.KeepOldMaxStack or MetadataOptions.Flags (KeepOldMaxStack, global option) to ignore this error. Otherwise fix your generated CIL code so it conforms to the ECMA standard.");
			return maxStack + 8;
		}
		return maxStack;
	}

	protected uint GetOffset(Instruction instr)
	{
		if (instr == null)
		{
			Error("Instruction is null");
			return 0u;
		}
		if (offsets.TryGetValue(instr, out var value))
		{
			return value;
		}
		Error("Found some other method's instruction or a removed instruction. You probably removed an instruction that is the target of a branch instruction or an instruction that's the first/last instruction in an exception handler.");
		return 0u;
	}

	protected uint InitializeInstructionOffsets()
	{
		uint num = 0u;
		IList<Instruction> list = instructions;
		for (int i = 0; i < list.Count; i++)
		{
			Instruction instruction = list[i];
			if (instruction != null)
			{
				offsets[instruction] = num;
				num += GetSizeOfInstruction(instruction);
			}
		}
		return num;
	}

	protected virtual uint GetSizeOfInstruction(Instruction instr)
	{
		return (uint)instr.GetSize();
	}

	protected uint WriteInstructions(ref ArrayWriter writer)
	{
		firstInstructionOffset = (uint)writer.Position;
		IList<Instruction> list = instructions;
		for (int i = 0; i < list.Count; i++)
		{
			Instruction instruction = list[i];
			if (instruction != null)
			{
				WriteInstruction(ref writer, instruction);
			}
		}
		return ToInstructionOffset(ref writer);
	}

	protected uint ToInstructionOffset(ref ArrayWriter writer)
	{
		return (uint)writer.Position - firstInstructionOffset;
	}

	protected virtual void WriteInstruction(ref ArrayWriter writer, Instruction instr)
	{
		WriteOpCode(ref writer, instr);
		WriteOperand(ref writer, instr);
	}

	protected void WriteOpCode(ref ArrayWriter writer, Instruction instr)
	{
		Code code = instr.OpCode.Code;
		int num = (int)code >> 8;
		if ((int)code <= 255)
		{
			writer.WriteByte((byte)code);
			return;
		}
		switch (num)
		{
		case 240:
		case 241:
		case 242:
		case 243:
		case 244:
		case 245:
		case 246:
		case 247:
		case 248:
		case 249:
		case 250:
		case 251:
		case 254:
			writer.WriteByte((byte)((int)code >> 8));
			writer.WriteByte((byte)code);
			return;
		}
		switch (code)
		{
		case Code.UNKNOWN1:
			writer.WriteByte(0);
			break;
		case Code.UNKNOWN2:
			writer.WriteUInt16(0);
			break;
		default:
			Error("Unknown instruction");
			writer.WriteByte(0);
			break;
		}
	}

	protected void WriteOperand(ref ArrayWriter writer, Instruction instr)
	{
		switch (instr.OpCode.OperandType)
		{
		case OperandType.InlineBrTarget:
			WriteInlineBrTarget(ref writer, instr);
			break;
		case OperandType.InlineField:
			WriteInlineField(ref writer, instr);
			break;
		case OperandType.InlineI:
			WriteInlineI(ref writer, instr);
			break;
		case OperandType.InlineI8:
			WriteInlineI8(ref writer, instr);
			break;
		case OperandType.InlineMethod:
			WriteInlineMethod(ref writer, instr);
			break;
		case OperandType.InlineNone:
			WriteInlineNone(ref writer, instr);
			break;
		case OperandType.InlinePhi:
			WriteInlinePhi(ref writer, instr);
			break;
		case OperandType.InlineR:
			WriteInlineR(ref writer, instr);
			break;
		case OperandType.InlineSig:
			WriteInlineSig(ref writer, instr);
			break;
		case OperandType.InlineString:
			WriteInlineString(ref writer, instr);
			break;
		case OperandType.InlineSwitch:
			WriteInlineSwitch(ref writer, instr);
			break;
		case OperandType.InlineTok:
			WriteInlineTok(ref writer, instr);
			break;
		case OperandType.InlineType:
			WriteInlineType(ref writer, instr);
			break;
		case OperandType.InlineVar:
			WriteInlineVar(ref writer, instr);
			break;
		case OperandType.ShortInlineBrTarget:
			WriteShortInlineBrTarget(ref writer, instr);
			break;
		case OperandType.ShortInlineI:
			WriteShortInlineI(ref writer, instr);
			break;
		case OperandType.ShortInlineR:
			WriteShortInlineR(ref writer, instr);
			break;
		case OperandType.ShortInlineVar:
			WriteShortInlineVar(ref writer, instr);
			break;
		default:
			Error("Unknown operand type");
			break;
		}
	}

	protected virtual void WriteInlineBrTarget(ref ArrayWriter writer, Instruction instr)
	{
		uint value = GetOffset(instr.Operand as Instruction) - (ToInstructionOffset(ref writer) + 4);
		writer.WriteUInt32(value);
	}

	protected abstract void WriteInlineField(ref ArrayWriter writer, Instruction instr);

	protected virtual void WriteInlineI(ref ArrayWriter writer, Instruction instr)
	{
		if (instr.Operand is int)
		{
			writer.WriteInt32((int)instr.Operand);
			return;
		}
		Error("Operand is not an Int32");
		writer.WriteInt32(0);
	}

	protected virtual void WriteInlineI8(ref ArrayWriter writer, Instruction instr)
	{
		if (instr.Operand is long)
		{
			writer.WriteInt64((long)instr.Operand);
			return;
		}
		Error("Operand is not an Int64");
		writer.WriteInt64(0L);
	}

	protected abstract void WriteInlineMethod(ref ArrayWriter writer, Instruction instr);

	protected virtual void WriteInlineNone(ref ArrayWriter writer, Instruction instr)
	{
	}

	protected virtual void WriteInlinePhi(ref ArrayWriter writer, Instruction instr)
	{
	}

	protected virtual void WriteInlineR(ref ArrayWriter writer, Instruction instr)
	{
		if (instr.Operand is double)
		{
			writer.WriteDouble((double)instr.Operand);
			return;
		}
		Error("Operand is not a Double");
		writer.WriteDouble(0.0);
	}

	protected abstract void WriteInlineSig(ref ArrayWriter writer, Instruction instr);

	protected abstract void WriteInlineString(ref ArrayWriter writer, Instruction instr);

	protected virtual void WriteInlineSwitch(ref ArrayWriter writer, Instruction instr)
	{
		if (!(instr.Operand is IList<Instruction> list))
		{
			Error("switch operand is not a list of instructions");
			writer.WriteInt32(0);
			return;
		}
		uint num = (uint)(ToInstructionOffset(ref writer) + 4 + list.Count * 4);
		writer.WriteInt32(list.Count);
		for (int i = 0; i < list.Count; i++)
		{
			Instruction instr2 = list[i];
			writer.WriteUInt32(GetOffset(instr2) - num);
		}
	}

	protected abstract void WriteInlineTok(ref ArrayWriter writer, Instruction instr);

	protected abstract void WriteInlineType(ref ArrayWriter writer, Instruction instr);

	protected virtual void WriteInlineVar(ref ArrayWriter writer, Instruction instr)
	{
		if (!(instr.Operand is IVariable { Index: var index }))
		{
			Error("Operand is not a local/arg");
			writer.WriteUInt16(0);
		}
		else if (0 <= index && index <= 65535)
		{
			writer.WriteUInt16((ushort)index);
		}
		else
		{
			Error("Local/arg index doesn't fit in a UInt16");
			writer.WriteUInt16(0);
		}
	}

	protected virtual void WriteShortInlineBrTarget(ref ArrayWriter writer, Instruction instr)
	{
		int num = (int)(GetOffset(instr.Operand as Instruction) - (ToInstructionOffset(ref writer) + 1));
		if (-128 <= num && num <= 127)
		{
			writer.WriteSByte((sbyte)num);
			return;
		}
		Error("Target instruction is too far away for a short branch. Use the long branch or call CilBody.SimplifyBranches() and CilBody.OptimizeBranches()");
		writer.WriteByte(0);
	}

	protected virtual void WriteShortInlineI(ref ArrayWriter writer, Instruction instr)
	{
		if (instr.Operand is sbyte)
		{
			writer.WriteSByte((sbyte)instr.Operand);
			return;
		}
		if (instr.Operand is byte)
		{
			writer.WriteByte((byte)instr.Operand);
			return;
		}
		Error("Operand is not a Byte or a SByte");
		writer.WriteByte(0);
	}

	protected virtual void WriteShortInlineR(ref ArrayWriter writer, Instruction instr)
	{
		if (instr.Operand is float)
		{
			writer.WriteSingle((float)instr.Operand);
			return;
		}
		Error("Operand is not a Single");
		writer.WriteSingle(0f);
	}

	protected virtual void WriteShortInlineVar(ref ArrayWriter writer, Instruction instr)
	{
		if (!(instr.Operand is IVariable { Index: var index }))
		{
			Error("Operand is not a local/arg");
			writer.WriteByte(0);
		}
		else if (0 <= index && index <= 255)
		{
			writer.WriteByte((byte)index);
		}
		else
		{
			Error("Local/arg index doesn't fit in a Byte. Use the longer ldloc/ldarg/stloc/starg instruction.");
			writer.WriteByte(0);
		}
	}
}
