using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace dnlib.DotNet.Emit;

[ComVisible(true)]
public sealed class OpCode
{
	public readonly string Name;

	public readonly Code Code;

	public readonly OperandType OperandType;

	public readonly FlowControl FlowControl;

	public readonly OpCodeType OpCodeType;

	public readonly StackBehaviour StackBehaviourPush;

	public readonly StackBehaviour StackBehaviourPop;

	public short Value => (short)Code;

	public int Size
	{
		get
		{
			if ((int)Code >= 256 && Code != Code.UNKNOWN1)
			{
				return 2;
			}
			return 1;
		}
	}

	public OpCode(string name, byte first, byte second, OperandType operandType, FlowControl flowControl, StackBehaviour push, StackBehaviour pop)
		: this(name, (Code)((first << 8) | second), operandType, flowControl, OpCodeType.Experimental, push, pop, experimental: true)
	{
	}

	internal OpCode(string name, Code code, OperandType operandType, FlowControl flowControl, OpCodeType opCodeType, StackBehaviour push, StackBehaviour pop, bool experimental = false)
	{
		Name = name;
		Code = code;
		OperandType = operandType;
		FlowControl = flowControl;
		OpCodeType = opCodeType;
		StackBehaviourPush = push;
		StackBehaviourPop = pop;
		if (!experimental)
		{
			if ((int)code >> 8 == 0)
			{
				OpCodes.OneByteOpCodes[(byte)code] = this;
			}
			else if ((int)code >> 8 == 254)
			{
				OpCodes.TwoByteOpCodes[(byte)code] = this;
			}
		}
	}

	public Instruction ToInstruction()
	{
		return Instruction.Create(this);
	}

	public Instruction ToInstruction(byte value)
	{
		return Instruction.Create(this, value);
	}

	public Instruction ToInstruction(sbyte value)
	{
		return Instruction.Create(this, value);
	}

	public Instruction ToInstruction(int value)
	{
		return Instruction.Create(this, value);
	}

	public Instruction ToInstruction(long value)
	{
		return Instruction.Create(this, value);
	}

	public Instruction ToInstruction(float value)
	{
		return Instruction.Create(this, value);
	}

	public Instruction ToInstruction(double value)
	{
		return Instruction.Create(this, value);
	}

	public Instruction ToInstruction(string s)
	{
		return Instruction.Create(this, s);
	}

	public Instruction ToInstruction(Instruction target)
	{
		return Instruction.Create(this, target);
	}

	public Instruction ToInstruction(IList<Instruction> targets)
	{
		return Instruction.Create(this, targets);
	}

	public Instruction ToInstruction(ITypeDefOrRef type)
	{
		return Instruction.Create(this, type);
	}

	public Instruction ToInstruction(CorLibTypeSig type)
	{
		return Instruction.Create(this, type.TypeDefOrRef);
	}

	public Instruction ToInstruction(MemberRef mr)
	{
		return Instruction.Create(this, mr);
	}

	public Instruction ToInstruction(IField field)
	{
		return Instruction.Create(this, field);
	}

	public Instruction ToInstruction(IMethod method)
	{
		return Instruction.Create(this, method);
	}

	public Instruction ToInstruction(ITokenOperand token)
	{
		return Instruction.Create(this, token);
	}

	public Instruction ToInstruction(MethodSig methodSig)
	{
		return Instruction.Create(this, methodSig);
	}

	public Instruction ToInstruction(Parameter parameter)
	{
		return Instruction.Create(this, parameter);
	}

	public Instruction ToInstruction(Local local)
	{
		return Instruction.Create(this, local);
	}

	public override string ToString()
	{
		return Name;
	}
}
