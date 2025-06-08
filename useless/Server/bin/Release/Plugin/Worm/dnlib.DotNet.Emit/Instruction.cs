using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using dnlib.DotNet.Pdb;

namespace dnlib.DotNet.Emit;

[ComVisible(true)]
public sealed class Instruction
{
	public OpCode OpCode;

	public object Operand;

	public uint Offset;

	public SequencePoint SequencePoint;

	public Instruction()
	{
	}

	public Instruction(OpCode opCode)
	{
		OpCode = opCode;
	}

	public Instruction(OpCode opCode, object operand)
	{
		OpCode = opCode;
		Operand = operand;
	}

	public static Instruction Create(OpCode opCode)
	{
		if (opCode.OperandType != OperandType.InlineNone)
		{
			throw new ArgumentException("Must be a no-operand opcode", "opCode");
		}
		return new Instruction(opCode);
	}

	public static Instruction Create(OpCode opCode, byte value)
	{
		if (opCode.Code != Code.Unaligned)
		{
			throw new ArgumentException("Opcode does not have a byte operand", "opCode");
		}
		return new Instruction(opCode, value);
	}

	public static Instruction Create(OpCode opCode, sbyte value)
	{
		if (opCode.Code != Code.Ldc_I4_S)
		{
			throw new ArgumentException("Opcode does not have a sbyte operand", "opCode");
		}
		return new Instruction(opCode, value);
	}

	public static Instruction Create(OpCode opCode, int value)
	{
		if (opCode.OperandType != OperandType.InlineI)
		{
			throw new ArgumentException("Opcode does not have an int32 operand", "opCode");
		}
		return new Instruction(opCode, value);
	}

	public static Instruction Create(OpCode opCode, long value)
	{
		if (opCode.OperandType != OperandType.InlineI8)
		{
			throw new ArgumentException("Opcode does not have an int64 operand", "opCode");
		}
		return new Instruction(opCode, value);
	}

	public static Instruction Create(OpCode opCode, float value)
	{
		if (opCode.OperandType != OperandType.ShortInlineR)
		{
			throw new ArgumentException("Opcode does not have a real4 operand", "opCode");
		}
		return new Instruction(opCode, value);
	}

	public static Instruction Create(OpCode opCode, double value)
	{
		if (opCode.OperandType != OperandType.InlineR)
		{
			throw new ArgumentException("Opcode does not have a real8 operand", "opCode");
		}
		return new Instruction(opCode, value);
	}

	public static Instruction Create(OpCode opCode, string s)
	{
		if (opCode.OperandType != OperandType.InlineString)
		{
			throw new ArgumentException("Opcode does not have a string operand", "opCode");
		}
		return new Instruction(opCode, s);
	}

	public static Instruction Create(OpCode opCode, Instruction target)
	{
		if (opCode.OperandType != OperandType.ShortInlineBrTarget && opCode.OperandType != 0)
		{
			throw new ArgumentException("Opcode does not have an instruction operand", "opCode");
		}
		return new Instruction(opCode, target);
	}

	public static Instruction Create(OpCode opCode, IList<Instruction> targets)
	{
		if (opCode.OperandType != OperandType.InlineSwitch)
		{
			throw new ArgumentException("Opcode does not have a targets array operand", "opCode");
		}
		return new Instruction(opCode, targets);
	}

	public static Instruction Create(OpCode opCode, ITypeDefOrRef type)
	{
		if (opCode.OperandType != OperandType.InlineType && opCode.OperandType != OperandType.InlineTok)
		{
			throw new ArgumentException("Opcode does not have a type operand", "opCode");
		}
		return new Instruction(opCode, type);
	}

	public static Instruction Create(OpCode opCode, CorLibTypeSig type)
	{
		return Create(opCode, type.TypeDefOrRef);
	}

	public static Instruction Create(OpCode opCode, MemberRef mr)
	{
		if (opCode.OperandType != OperandType.InlineField && opCode.OperandType != OperandType.InlineMethod && opCode.OperandType != OperandType.InlineTok)
		{
			throw new ArgumentException("Opcode does not have a field operand", "opCode");
		}
		return new Instruction(opCode, mr);
	}

	public static Instruction Create(OpCode opCode, IField field)
	{
		if (opCode.OperandType != OperandType.InlineField && opCode.OperandType != OperandType.InlineTok)
		{
			throw new ArgumentException("Opcode does not have a field operand", "opCode");
		}
		return new Instruction(opCode, field);
	}

	public static Instruction Create(OpCode opCode, IMethod method)
	{
		if (opCode.OperandType != OperandType.InlineMethod && opCode.OperandType != OperandType.InlineTok)
		{
			throw new ArgumentException("Opcode does not have a method operand", "opCode");
		}
		return new Instruction(opCode, method);
	}

	public static Instruction Create(OpCode opCode, ITokenOperand token)
	{
		if (opCode.OperandType != OperandType.InlineTok)
		{
			throw new ArgumentException("Opcode does not have a token operand", "opCode");
		}
		return new Instruction(opCode, token);
	}

	public static Instruction Create(OpCode opCode, MethodSig methodSig)
	{
		if (opCode.OperandType != OperandType.InlineSig)
		{
			throw new ArgumentException("Opcode does not have a method sig operand", "opCode");
		}
		return new Instruction(opCode, methodSig);
	}

	public static Instruction Create(OpCode opCode, Parameter parameter)
	{
		if (opCode.OperandType != OperandType.ShortInlineVar && opCode.OperandType != OperandType.InlineVar)
		{
			throw new ArgumentException("Opcode does not have a method parameter operand", "opCode");
		}
		return new Instruction(opCode, parameter);
	}

	public static Instruction Create(OpCode opCode, Local local)
	{
		if (opCode.OperandType != OperandType.ShortInlineVar && opCode.OperandType != OperandType.InlineVar)
		{
			throw new ArgumentException("Opcode does not have a method local operand", "opCode");
		}
		return new Instruction(opCode, local);
	}

	public static Instruction CreateLdcI4(int value)
	{
		switch (value)
		{
		case -1:
			return OpCodes.Ldc_I4_M1.ToInstruction();
		case 0:
			return OpCodes.Ldc_I4_0.ToInstruction();
		case 1:
			return OpCodes.Ldc_I4_1.ToInstruction();
		case 2:
			return OpCodes.Ldc_I4_2.ToInstruction();
		case 3:
			return OpCodes.Ldc_I4_3.ToInstruction();
		case 4:
			return OpCodes.Ldc_I4_4.ToInstruction();
		case 5:
			return OpCodes.Ldc_I4_5.ToInstruction();
		case 6:
			return OpCodes.Ldc_I4_6.ToInstruction();
		case 7:
			return OpCodes.Ldc_I4_7.ToInstruction();
		case 8:
			return OpCodes.Ldc_I4_8.ToInstruction();
		default:
			if (-128 <= value && value <= 127)
			{
				return new Instruction(OpCodes.Ldc_I4_S, (sbyte)value);
			}
			return new Instruction(OpCodes.Ldc_I4, value);
		}
	}

	public int GetSize()
	{
		OpCode opCode = OpCode;
		switch (opCode.OperandType)
		{
		case OperandType.InlineBrTarget:
		case OperandType.InlineField:
		case OperandType.InlineI:
		case OperandType.InlineMethod:
		case OperandType.InlineSig:
		case OperandType.InlineString:
		case OperandType.InlineTok:
		case OperandType.InlineType:
		case OperandType.ShortInlineR:
			return opCode.Size + 4;
		case OperandType.InlineI8:
		case OperandType.InlineR:
			return opCode.Size + 8;
		default:
			return opCode.Size;
		case OperandType.InlineSwitch:
		{
			IList<Instruction> list = Operand as IList<Instruction>;
			return opCode.Size + 4 + ((list != null) ? (list.Count * 4) : 0);
		}
		case OperandType.InlineVar:
			return opCode.Size + 2;
		case OperandType.ShortInlineBrTarget:
		case OperandType.ShortInlineI:
		case OperandType.ShortInlineVar:
			return opCode.Size + 1;
		}
	}

	private static bool IsSystemVoid(TypeSig type)
	{
		return type.RemovePinnedAndModifiers().GetElementType() == ElementType.Void;
	}

	public void UpdateStack(ref int stack)
	{
		UpdateStack(ref stack, methodHasReturnValue: false);
	}

	public void UpdateStack(ref int stack, bool methodHasReturnValue)
	{
		CalculateStackUsage(methodHasReturnValue, out var pushes, out var pops);
		if (pops == -1)
		{
			stack = 0;
		}
		else
		{
			stack += pushes - pops;
		}
	}

	public void CalculateStackUsage(out int pushes, out int pops)
	{
		CalculateStackUsage(methodHasReturnValue: false, out pushes, out pops);
	}

	public void CalculateStackUsage(bool methodHasReturnValue, out int pushes, out int pops)
	{
		OpCode opCode = OpCode;
		if (opCode.FlowControl == FlowControl.Call)
		{
			CalculateStackUsageCall(opCode.Code, out pushes, out pops);
		}
		else
		{
			CalculateStackUsageNonCall(opCode, methodHasReturnValue, out pushes, out pops);
		}
	}

	private void CalculateStackUsageCall(Code code, out int pushes, out int pops)
	{
		pushes = 0;
		pops = 0;
		if (code == Code.Jmp)
		{
			return;
		}
		object operand = Operand;
		MethodSig methodSig = ((!(operand is IMethod method)) ? (operand as MethodSig) : method.MethodSig);
		if (methodSig != null)
		{
			bool implicitThis = methodSig.ImplicitThis;
			if (!IsSystemVoid(methodSig.RetType) || (code == Code.Newobj && methodSig.HasThis))
			{
				pushes++;
			}
			pops += methodSig.Params.Count;
			IList<TypeSig> paramsAfterSentinel = methodSig.ParamsAfterSentinel;
			if (paramsAfterSentinel != null)
			{
				pops += paramsAfterSentinel.Count;
			}
			if (implicitThis && code != Code.Newobj)
			{
				pops++;
			}
			if (code == Code.Calli)
			{
				pops++;
			}
		}
	}

	private void CalculateStackUsageNonCall(OpCode opCode, bool hasReturnValue, out int pushes, out int pops)
	{
		switch (opCode.StackBehaviourPush)
		{
		case StackBehaviour.Push0:
			pushes = 0;
			break;
		case StackBehaviour.Push1:
		case StackBehaviour.Pushi:
		case StackBehaviour.Pushi8:
		case StackBehaviour.Pushr4:
		case StackBehaviour.Pushr8:
		case StackBehaviour.Pushref:
			pushes = 1;
			break;
		case StackBehaviour.Push1_push1:
			pushes = 2;
			break;
		default:
			pushes = 0;
			break;
		}
		switch (opCode.StackBehaviourPop)
		{
		case StackBehaviour.Pop0:
			pops = 0;
			break;
		case StackBehaviour.Pop1:
		case StackBehaviour.Popi:
		case StackBehaviour.Popref:
			pops = 1;
			break;
		case StackBehaviour.Pop1_pop1:
		case StackBehaviour.Popi_pop1:
		case StackBehaviour.Popi_popi:
		case StackBehaviour.Popi_popi8:
		case StackBehaviour.Popi_popr4:
		case StackBehaviour.Popi_popr8:
		case StackBehaviour.Popref_pop1:
		case StackBehaviour.Popref_popi:
			pops = 2;
			break;
		case StackBehaviour.Popi_popi_popi:
		case StackBehaviour.Popref_popi_popi:
		case StackBehaviour.Popref_popi_popi8:
		case StackBehaviour.Popref_popi_popr4:
		case StackBehaviour.Popref_popi_popr8:
		case StackBehaviour.Popref_popi_popref:
		case StackBehaviour.Popref_popi_pop1:
			pops = 3;
			break;
		case StackBehaviour.PopAll:
			pops = -1;
			break;
		case StackBehaviour.Varpop:
			if (hasReturnValue)
			{
				pops = 1;
			}
			else
			{
				pops = 0;
			}
			break;
		default:
			pops = 0;
			break;
		}
	}

	public bool IsLeave()
	{
		if (OpCode != OpCodes.Leave)
		{
			return OpCode == OpCodes.Leave_S;
		}
		return true;
	}

	public bool IsBr()
	{
		if (OpCode != OpCodes.Br)
		{
			return OpCode == OpCodes.Br_S;
		}
		return true;
	}

	public bool IsBrfalse()
	{
		if (OpCode != OpCodes.Brfalse)
		{
			return OpCode == OpCodes.Brfalse_S;
		}
		return true;
	}

	public bool IsBrtrue()
	{
		if (OpCode != OpCodes.Brtrue)
		{
			return OpCode == OpCodes.Brtrue_S;
		}
		return true;
	}

	public bool IsConditionalBranch()
	{
		Code code = OpCode.Code;
		if (code - 44 <= Code.Stloc_1 || code - 57 <= Code.Stloc_1)
		{
			return true;
		}
		return false;
	}

	public bool IsLdcI4()
	{
		Code code = OpCode.Code;
		if (code - 21 <= Code.Stloc_1)
		{
			return true;
		}
		return false;
	}

	public int GetLdcI4Value()
	{
		return OpCode.Code switch
		{
			Code.Ldc_I4_M1 => -1, 
			Code.Ldc_I4_0 => 0, 
			Code.Ldc_I4_1 => 1, 
			Code.Ldc_I4_2 => 2, 
			Code.Ldc_I4_3 => 3, 
			Code.Ldc_I4_4 => 4, 
			Code.Ldc_I4_5 => 5, 
			Code.Ldc_I4_6 => 6, 
			Code.Ldc_I4_7 => 7, 
			Code.Ldc_I4_8 => 8, 
			Code.Ldc_I4_S => (sbyte)Operand, 
			Code.Ldc_I4 => (int)Operand, 
			_ => throw new InvalidOperationException($"Not a ldc.i4 instruction: {this}"), 
		};
	}

	public bool IsLdarg()
	{
		Code code = OpCode.Code;
		if (code - 2 <= Code.Ldarg_1 || code == Code.Ldarg_S || code == Code.Ldarg)
		{
			return true;
		}
		return false;
	}

	public bool IsLdloc()
	{
		Code code = OpCode.Code;
		if (code - 6 <= Code.Ldarg_1 || code == Code.Ldloc_S || code == Code.Ldloc)
		{
			return true;
		}
		return false;
	}

	public bool IsStarg()
	{
		Code code = OpCode.Code;
		if (code == Code.Starg_S || code == Code.Starg)
		{
			return true;
		}
		return false;
	}

	public bool IsStloc()
	{
		Code code = OpCode.Code;
		if (code - 10 <= Code.Ldarg_1 || code == Code.Stloc_S || code == Code.Stloc)
		{
			return true;
		}
		return false;
	}

	public Local GetLocal(IList<Local> locals)
	{
		Code code = OpCode.Code;
		int num;
		switch (code)
		{
		case Code.Ldloc_S:
		case Code.Ldloca_S:
		case Code.Stloc_S:
		case Code.Ldloc:
		case Code.Ldloca:
		case Code.Stloc:
			return Operand as Local;
		case Code.Ldloc_0:
		case Code.Ldloc_1:
		case Code.Ldloc_2:
		case Code.Ldloc_3:
			num = (int)(code - 6);
			break;
		case Code.Stloc_0:
		case Code.Stloc_1:
		case Code.Stloc_2:
		case Code.Stloc_3:
			num = (int)(code - 10);
			break;
		default:
			return null;
		}
		if ((uint)num < (uint)locals.Count)
		{
			return locals[num];
		}
		return null;
	}

	public int GetParameterIndex()
	{
		switch (OpCode.Code)
		{
		case Code.Ldarg_0:
			return 0;
		case Code.Ldarg_1:
			return 1;
		case Code.Ldarg_2:
			return 2;
		case Code.Ldarg_3:
			return 3;
		case Code.Ldarg_S:
		case Code.Ldarga_S:
		case Code.Starg_S:
		case Code.Ldarg:
		case Code.Ldarga:
		case Code.Starg:
			if (Operand is Parameter parameter)
			{
				return parameter.Index;
			}
			break;
		}
		return -1;
	}

	public Parameter GetParameter(IList<Parameter> parameters)
	{
		int parameterIndex = GetParameterIndex();
		if ((uint)parameterIndex < (uint)parameters.Count)
		{
			return parameters[parameterIndex];
		}
		return null;
	}

	public TypeSig GetArgumentType(MethodSig methodSig, ITypeDefOrRef declaringType)
	{
		if (methodSig == null)
		{
			return null;
		}
		int num = GetParameterIndex();
		if (num == 0 && methodSig.ImplicitThis)
		{
			if (declaringType == null)
			{
				return null;
			}
			TypeSig typeSig;
			bool isValueType;
			if (declaringType is TypeSpec typeSpec)
			{
				typeSig = typeSpec.TypeSig;
				isValueType = typeSig.IsValueType;
			}
			else
			{
				TypeDef typeDef = declaringType.ResolveTypeDef();
				if (typeDef == null)
				{
					return declaringType.ToTypeSig();
				}
				isValueType = typeDef.IsValueType;
				ClassOrValueTypeSig classOrValueTypeSig = (isValueType ? ((ClassOrValueTypeSig)new ValueTypeSig(typeDef)) : ((ClassOrValueTypeSig)new ClassSig(typeDef)));
				if (typeDef.HasGenericParameters)
				{
					int count = typeDef.GenericParameters.Count;
					List<TypeSig> list = new List<TypeSig>(count);
					for (int i = 0; i < count; i++)
					{
						list.Add(new GenericVar(i, typeDef));
					}
					typeSig = new GenericInstSig(classOrValueTypeSig, list);
				}
				else
				{
					typeSig = classOrValueTypeSig;
				}
			}
			if (!isValueType)
			{
				return typeSig;
			}
			return new ByRefSig(typeSig);
		}
		if (methodSig.ImplicitThis)
		{
			num--;
		}
		if ((uint)num < (uint)methodSig.Params.Count)
		{
			return methodSig.Params[num];
		}
		return null;
	}

	public Instruction Clone()
	{
		return new Instruction
		{
			Offset = Offset,
			OpCode = OpCode,
			Operand = Operand,
			SequencePoint = SequencePoint
		};
	}

	public override string ToString()
	{
		return InstructionPrinter.ToString(this);
	}
}
