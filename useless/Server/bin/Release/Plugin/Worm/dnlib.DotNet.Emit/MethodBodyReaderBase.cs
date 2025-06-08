using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using dnlib.IO;

namespace dnlib.DotNet.Emit;

[ComVisible(true)]
public abstract class MethodBodyReaderBase
{
	protected DataReader reader;

	protected IList<Parameter> parameters;

	protected IList<Local> locals = new List<Local>();

	protected IList<Instruction> instructions;

	protected IList<ExceptionHandler> exceptionHandlers = new List<ExceptionHandler>();

	private uint currentOffset;

	protected uint codeEndOffs;

	protected uint codeStartOffs;

	private readonly ModuleContext context;

	public IList<Parameter> Parameters => parameters;

	public IList<Local> Locals => locals;

	public IList<Instruction> Instructions => instructions;

	public IList<ExceptionHandler> ExceptionHandlers => exceptionHandlers;

	protected MethodBodyReaderBase()
	{
	}

	protected MethodBodyReaderBase(ModuleContext context)
	{
		this.context = context;
	}

	protected MethodBodyReaderBase(DataReader reader)
		: this(reader, null)
	{
	}

	protected MethodBodyReaderBase(DataReader reader, IList<Parameter> parameters)
		: this(reader, parameters, null)
	{
	}

	protected MethodBodyReaderBase(DataReader reader, IList<Parameter> parameters, ModuleContext context)
	{
		this.reader = reader;
		this.parameters = parameters;
		this.context = context;
	}

	protected void SetLocals(IList<TypeSig> newLocals)
	{
		IList<Local> list = locals;
		list.Clear();
		if (newLocals != null)
		{
			int count = newLocals.Count;
			for (int i = 0; i < count; i++)
			{
				list.Add(new Local(newLocals[i]));
			}
		}
	}

	protected void SetLocals(IList<Local> newLocals)
	{
		IList<Local> list = locals;
		list.Clear();
		if (newLocals != null)
		{
			int count = newLocals.Count;
			for (int i = 0; i < count; i++)
			{
				list.Add(new Local(newLocals[i].Type));
			}
		}
	}

	protected void ReadInstructions(int numInstrs)
	{
		codeStartOffs = reader.Position;
		codeEndOffs = reader.Length;
		instructions = new List<Instruction>(numInstrs);
		currentOffset = 0u;
		IList<Instruction> list = instructions;
		for (int i = 0; i < numInstrs; i++)
		{
			if (reader.Position >= codeEndOffs)
			{
				break;
			}
			list.Add(ReadOneInstruction());
		}
		FixBranches();
	}

	protected void ReadInstructionsNumBytes(uint codeSize)
	{
		codeStartOffs = reader.Position;
		codeEndOffs = reader.Position + codeSize;
		if (codeEndOffs < codeStartOffs || codeEndOffs > reader.Length)
		{
			throw new InvalidMethodException("Invalid code size");
		}
		instructions = new List<Instruction>();
		currentOffset = 0u;
		IList<Instruction> list = instructions;
		while (reader.Position < codeEndOffs)
		{
			list.Add(ReadOneInstruction());
		}
		reader.Position = codeEndOffs;
		FixBranches();
	}

	private void FixBranches()
	{
		IList<Instruction> list = instructions;
		int count = list.Count;
		for (int i = 0; i < count; i++)
		{
			Instruction instruction = list[i];
			switch (instruction.OpCode.OperandType)
			{
			case OperandType.InlineBrTarget:
			case OperandType.ShortInlineBrTarget:
				instruction.Operand = GetInstruction((uint)instruction.Operand);
				break;
			case OperandType.InlineSwitch:
			{
				IList<uint> list2 = (IList<uint>)instruction.Operand;
				Instruction[] array = new Instruction[list2.Count];
				for (int j = 0; j < list2.Count; j++)
				{
					array[j] = GetInstruction(list2[j]);
				}
				instruction.Operand = array;
				break;
			}
			}
		}
	}

	protected Instruction GetInstruction(uint offset)
	{
		IList<Instruction> list = instructions;
		int num = 0;
		int num2 = list.Count - 1;
		while (num <= num2 && num2 != -1)
		{
			int num3 = (num + num2) / 2;
			Instruction instruction = list[num3];
			if (instruction.Offset == offset)
			{
				return instruction;
			}
			if (offset < instruction.Offset)
			{
				num2 = num3 - 1;
			}
			else
			{
				num = num3 + 1;
			}
		}
		return null;
	}

	protected Instruction GetInstructionThrow(uint offset)
	{
		Instruction instruction = GetInstruction(offset);
		if (instruction != null)
		{
			return instruction;
		}
		throw new InvalidOperationException($"There's no instruction @ {offset:X4}");
	}

	private Instruction ReadOneInstruction()
	{
		Instruction instruction = new Instruction();
		instruction.Offset = currentOffset;
		instruction.OpCode = ReadOpCode();
		instruction.Operand = ReadOperand(instruction);
		if (instruction.OpCode.Code == Code.Switch)
		{
			IList<uint> list = (IList<uint>)instruction.Operand;
			currentOffset += (uint)(instruction.OpCode.Size + 4 + 4 * list.Count);
		}
		else
		{
			currentOffset += (uint)instruction.GetSize();
		}
		if (currentOffset < instruction.Offset)
		{
			reader.Position = codeEndOffs;
		}
		return instruction;
	}

	private OpCode ReadOpCode()
	{
		byte b = reader.ReadByte();
		switch (b)
		{
		case 254:
			return OpCodes.TwoByteOpCodes[reader.ReadByte()];
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
			if (context != null && reader.BytesLeft >= 1)
			{
				OpCode experimentalOpCode = context.GetExperimentalOpCode(b, reader.ReadByte());
				if (experimentalOpCode != null)
				{
					return experimentalOpCode;
				}
				reader.Position--;
			}
			break;
		}
		return OpCodes.OneByteOpCodes[b];
	}

	private object ReadOperand(Instruction instr)
	{
		return instr.OpCode.OperandType switch
		{
			OperandType.InlineBrTarget => ReadInlineBrTarget(instr), 
			OperandType.InlineField => ReadInlineField(instr), 
			OperandType.InlineI => ReadInlineI(instr), 
			OperandType.InlineI8 => ReadInlineI8(instr), 
			OperandType.InlineMethod => ReadInlineMethod(instr), 
			OperandType.InlineNone => ReadInlineNone(instr), 
			OperandType.InlinePhi => ReadInlinePhi(instr), 
			OperandType.InlineR => ReadInlineR(instr), 
			OperandType.InlineSig => ReadInlineSig(instr), 
			OperandType.InlineString => ReadInlineString(instr), 
			OperandType.InlineSwitch => ReadInlineSwitch(instr), 
			OperandType.InlineTok => ReadInlineTok(instr), 
			OperandType.InlineType => ReadInlineType(instr), 
			OperandType.InlineVar => ReadInlineVar(instr), 
			OperandType.ShortInlineBrTarget => ReadShortInlineBrTarget(instr), 
			OperandType.ShortInlineI => ReadShortInlineI(instr), 
			OperandType.ShortInlineR => ReadShortInlineR(instr), 
			OperandType.ShortInlineVar => ReadShortInlineVar(instr), 
			_ => throw new InvalidOperationException("Invalid OpCode.OperandType"), 
		};
	}

	protected virtual uint ReadInlineBrTarget(Instruction instr)
	{
		return (uint)((int)instr.Offset + instr.GetSize()) + reader.ReadUInt32();
	}

	protected abstract IField ReadInlineField(Instruction instr);

	protected virtual int ReadInlineI(Instruction instr)
	{
		return reader.ReadInt32();
	}

	protected virtual long ReadInlineI8(Instruction instr)
	{
		return reader.ReadInt64();
	}

	protected abstract IMethod ReadInlineMethod(Instruction instr);

	protected virtual object ReadInlineNone(Instruction instr)
	{
		return null;
	}

	protected virtual object ReadInlinePhi(Instruction instr)
	{
		return null;
	}

	protected virtual double ReadInlineR(Instruction instr)
	{
		return reader.ReadDouble();
	}

	protected abstract MethodSig ReadInlineSig(Instruction instr);

	protected abstract string ReadInlineString(Instruction instr);

	protected virtual IList<uint> ReadInlineSwitch(Instruction instr)
	{
		uint num = reader.ReadUInt32();
		long num2 = instr.Offset + instr.OpCode.Size + 4 + (long)num * 4L;
		if (num2 > uint.MaxValue || codeStartOffs + num2 > codeEndOffs)
		{
			reader.Position = codeEndOffs;
			return Array2.Empty<uint>();
		}
		uint[] array = new uint[num];
		uint num3 = (uint)num2;
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = num3 + reader.ReadUInt32();
		}
		return array;
	}

	protected abstract ITokenOperand ReadInlineTok(Instruction instr);

	protected abstract ITypeDefOrRef ReadInlineType(Instruction instr);

	protected virtual IVariable ReadInlineVar(Instruction instr)
	{
		if (IsArgOperandInstruction(instr))
		{
			return ReadInlineVarArg(instr);
		}
		return ReadInlineVarLocal(instr);
	}

	protected virtual Parameter ReadInlineVarArg(Instruction instr)
	{
		return GetParameter(reader.ReadUInt16());
	}

	protected virtual Local ReadInlineVarLocal(Instruction instr)
	{
		return GetLocal(reader.ReadUInt16());
	}

	protected virtual uint ReadShortInlineBrTarget(Instruction instr)
	{
		return (uint)((int)instr.Offset + instr.GetSize() + reader.ReadSByte());
	}

	protected virtual object ReadShortInlineI(Instruction instr)
	{
		if (instr.OpCode.Code == Code.Ldc_I4_S)
		{
			return reader.ReadSByte();
		}
		return reader.ReadByte();
	}

	protected virtual float ReadShortInlineR(Instruction instr)
	{
		return reader.ReadSingle();
	}

	protected virtual IVariable ReadShortInlineVar(Instruction instr)
	{
		if (IsArgOperandInstruction(instr))
		{
			return ReadShortInlineVarArg(instr);
		}
		return ReadShortInlineVarLocal(instr);
	}

	protected virtual Parameter ReadShortInlineVarArg(Instruction instr)
	{
		return GetParameter(reader.ReadByte());
	}

	protected virtual Local ReadShortInlineVarLocal(Instruction instr)
	{
		return GetLocal(reader.ReadByte());
	}

	protected static bool IsArgOperandInstruction(Instruction instr)
	{
		Code code = instr.OpCode.Code;
		if (code - 14 <= Code.Ldarg_0 || code - 65033 <= Code.Ldarg_0)
		{
			return true;
		}
		return false;
	}

	protected Parameter GetParameter(int index)
	{
		IList<Parameter> list = parameters;
		if ((uint)index < (uint)list.Count)
		{
			return list[index];
		}
		return null;
	}

	protected Local GetLocal(int index)
	{
		IList<Local> list = locals;
		if ((uint)index < (uint)list.Count)
		{
			return list[index];
		}
		return null;
	}

	protected bool Add(ExceptionHandler eh)
	{
		uint offset = GetOffset(eh.TryStart);
		uint offset2 = GetOffset(eh.TryEnd);
		if (offset2 <= offset)
		{
			return false;
		}
		uint offset3 = GetOffset(eh.HandlerStart);
		uint offset4 = GetOffset(eh.HandlerEnd);
		if (offset4 <= offset3)
		{
			return false;
		}
		if (eh.IsFilter)
		{
			if (eh.FilterStart == null)
			{
				return false;
			}
			if (eh.FilterStart.Offset >= offset3)
			{
				return false;
			}
		}
		if (offset3 <= offset && offset < offset4)
		{
			return false;
		}
		if (offset3 < offset2 && offset2 <= offset4)
		{
			return false;
		}
		if (offset <= offset3 && offset3 < offset2)
		{
			return false;
		}
		if (offset < offset4 && offset4 <= offset2)
		{
			return false;
		}
		exceptionHandlers.Add(eh);
		return true;
	}

	private uint GetOffset(Instruction instr)
	{
		if (instr != null)
		{
			return instr.Offset;
		}
		IList<Instruction> list = instructions;
		if (list.Count == 0)
		{
			return 0u;
		}
		return list[list.Count - 1].Offset;
	}

	public virtual void RestoreMethod(MethodDef method)
	{
		CilBody body = method.Body;
		body.Variables.Clear();
		IList<Local> list = locals;
		if (list != null)
		{
			int count = list.Count;
			for (int i = 0; i < count; i++)
			{
				body.Variables.Add(list[i]);
			}
		}
		body.Instructions.Clear();
		IList<Instruction> list2 = instructions;
		if (list2 != null)
		{
			int count2 = list2.Count;
			for (int j = 0; j < count2; j++)
			{
				body.Instructions.Add(list2[j]);
			}
		}
		body.ExceptionHandlers.Clear();
		IList<ExceptionHandler> list3 = exceptionHandlers;
		if (list3 != null)
		{
			int count3 = list3.Count;
			for (int k = 0; k < count3; k++)
			{
				body.ExceptionHandlers.Add(list3[k]);
			}
		}
	}
}
