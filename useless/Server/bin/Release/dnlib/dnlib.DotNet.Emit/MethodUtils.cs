using System.Collections.Generic;

namespace dnlib.DotNet.Emit;

public static class MethodUtils
{
	public static void SimplifyMacros(this IList<Instruction> instructions, IList<Local> locals, IList<Parameter> parameters)
	{
		int count = instructions.Count;
		for (int i = 0; i < count; i++)
		{
			Instruction instruction = instructions[i];
			switch (instruction.OpCode.Code)
			{
			case Code.Beq_S:
				instruction.OpCode = OpCodes.Beq;
				break;
			case Code.Bge_S:
				instruction.OpCode = OpCodes.Bge;
				break;
			case Code.Bge_Un_S:
				instruction.OpCode = OpCodes.Bge_Un;
				break;
			case Code.Bgt_S:
				instruction.OpCode = OpCodes.Bgt;
				break;
			case Code.Bgt_Un_S:
				instruction.OpCode = OpCodes.Bgt_Un;
				break;
			case Code.Ble_S:
				instruction.OpCode = OpCodes.Ble;
				break;
			case Code.Ble_Un_S:
				instruction.OpCode = OpCodes.Ble_Un;
				break;
			case Code.Blt_S:
				instruction.OpCode = OpCodes.Blt;
				break;
			case Code.Blt_Un_S:
				instruction.OpCode = OpCodes.Blt_Un;
				break;
			case Code.Bne_Un_S:
				instruction.OpCode = OpCodes.Bne_Un;
				break;
			case Code.Br_S:
				instruction.OpCode = OpCodes.Br;
				break;
			case Code.Brfalse_S:
				instruction.OpCode = OpCodes.Brfalse;
				break;
			case Code.Brtrue_S:
				instruction.OpCode = OpCodes.Brtrue;
				break;
			case Code.Ldarg_0:
				instruction.OpCode = OpCodes.Ldarg;
				instruction.Operand = ReadList(parameters, 0);
				break;
			case Code.Ldarg_1:
				instruction.OpCode = OpCodes.Ldarg;
				instruction.Operand = ReadList(parameters, 1);
				break;
			case Code.Ldarg_2:
				instruction.OpCode = OpCodes.Ldarg;
				instruction.Operand = ReadList(parameters, 2);
				break;
			case Code.Ldarg_3:
				instruction.OpCode = OpCodes.Ldarg;
				instruction.Operand = ReadList(parameters, 3);
				break;
			case Code.Ldarg_S:
				instruction.OpCode = OpCodes.Ldarg;
				break;
			case Code.Ldarga_S:
				instruction.OpCode = OpCodes.Ldarga;
				break;
			case Code.Ldc_I4_0:
				instruction.OpCode = OpCodes.Ldc_I4;
				instruction.Operand = 0;
				break;
			case Code.Ldc_I4_1:
				instruction.OpCode = OpCodes.Ldc_I4;
				instruction.Operand = 1;
				break;
			case Code.Ldc_I4_2:
				instruction.OpCode = OpCodes.Ldc_I4;
				instruction.Operand = 2;
				break;
			case Code.Ldc_I4_3:
				instruction.OpCode = OpCodes.Ldc_I4;
				instruction.Operand = 3;
				break;
			case Code.Ldc_I4_4:
				instruction.OpCode = OpCodes.Ldc_I4;
				instruction.Operand = 4;
				break;
			case Code.Ldc_I4_5:
				instruction.OpCode = OpCodes.Ldc_I4;
				instruction.Operand = 5;
				break;
			case Code.Ldc_I4_6:
				instruction.OpCode = OpCodes.Ldc_I4;
				instruction.Operand = 6;
				break;
			case Code.Ldc_I4_7:
				instruction.OpCode = OpCodes.Ldc_I4;
				instruction.Operand = 7;
				break;
			case Code.Ldc_I4_8:
				instruction.OpCode = OpCodes.Ldc_I4;
				instruction.Operand = 8;
				break;
			case Code.Ldc_I4_M1:
				instruction.OpCode = OpCodes.Ldc_I4;
				instruction.Operand = -1;
				break;
			case Code.Ldc_I4_S:
				instruction.OpCode = OpCodes.Ldc_I4;
				instruction.Operand = (int)(sbyte)instruction.Operand;
				break;
			case Code.Ldloc_0:
				instruction.OpCode = OpCodes.Ldloc;
				instruction.Operand = ReadList(locals, 0);
				break;
			case Code.Ldloc_1:
				instruction.OpCode = OpCodes.Ldloc;
				instruction.Operand = ReadList(locals, 1);
				break;
			case Code.Ldloc_2:
				instruction.OpCode = OpCodes.Ldloc;
				instruction.Operand = ReadList(locals, 2);
				break;
			case Code.Ldloc_3:
				instruction.OpCode = OpCodes.Ldloc;
				instruction.Operand = ReadList(locals, 3);
				break;
			case Code.Ldloc_S:
				instruction.OpCode = OpCodes.Ldloc;
				break;
			case Code.Ldloca_S:
				instruction.OpCode = OpCodes.Ldloca;
				break;
			case Code.Leave_S:
				instruction.OpCode = OpCodes.Leave;
				break;
			case Code.Starg_S:
				instruction.OpCode = OpCodes.Starg;
				break;
			case Code.Stloc_0:
				instruction.OpCode = OpCodes.Stloc;
				instruction.Operand = ReadList(locals, 0);
				break;
			case Code.Stloc_1:
				instruction.OpCode = OpCodes.Stloc;
				instruction.Operand = ReadList(locals, 1);
				break;
			case Code.Stloc_2:
				instruction.OpCode = OpCodes.Stloc;
				instruction.Operand = ReadList(locals, 2);
				break;
			case Code.Stloc_3:
				instruction.OpCode = OpCodes.Stloc;
				instruction.Operand = ReadList(locals, 3);
				break;
			case Code.Stloc_S:
				instruction.OpCode = OpCodes.Stloc;
				break;
			}
		}
	}

	private static T ReadList<T>(IList<T> list, int index)
	{
		if (list == null)
		{
			return default(T);
		}
		if ((uint)index < (uint)list.Count)
		{
			return list[index];
		}
		return default(T);
	}

	public static void OptimizeMacros(this IList<Instruction> instructions)
	{
		int count = instructions.Count;
		for (int i = 0; i < count; i++)
		{
			Instruction instruction = instructions[i];
			switch (instruction.OpCode.Code)
			{
			case Code.Ldarg_S:
			case Code.Ldarg:
				if (instruction.Operand is Parameter parameter2)
				{
					if (parameter2.Index == 0)
					{
						instruction.OpCode = OpCodes.Ldarg_0;
						instruction.Operand = null;
					}
					else if (parameter2.Index == 1)
					{
						instruction.OpCode = OpCodes.Ldarg_1;
						instruction.Operand = null;
					}
					else if (parameter2.Index == 2)
					{
						instruction.OpCode = OpCodes.Ldarg_2;
						instruction.Operand = null;
					}
					else if (parameter2.Index == 3)
					{
						instruction.OpCode = OpCodes.Ldarg_3;
						instruction.Operand = null;
					}
					else if (0 <= parameter2.Index && parameter2.Index <= 255)
					{
						instruction.OpCode = OpCodes.Ldarg_S;
					}
				}
				break;
			case Code.Ldarga:
				if (instruction.Operand is Parameter parameter && 0 <= parameter.Index && parameter.Index <= 255)
				{
					instruction.OpCode = OpCodes.Ldarga_S;
				}
				break;
			case Code.Ldc_I4_S:
			case Code.Ldc_I4:
			{
				int num;
				if (instruction.Operand is int)
				{
					num = (int)instruction.Operand;
				}
				else
				{
					if (!(instruction.Operand is sbyte))
					{
						break;
					}
					num = (sbyte)instruction.Operand;
				}
				switch (num)
				{
				case 0:
					instruction.OpCode = OpCodes.Ldc_I4_0;
					instruction.Operand = null;
					break;
				case 1:
					instruction.OpCode = OpCodes.Ldc_I4_1;
					instruction.Operand = null;
					break;
				case 2:
					instruction.OpCode = OpCodes.Ldc_I4_2;
					instruction.Operand = null;
					break;
				case 3:
					instruction.OpCode = OpCodes.Ldc_I4_3;
					instruction.Operand = null;
					break;
				case 4:
					instruction.OpCode = OpCodes.Ldc_I4_4;
					instruction.Operand = null;
					break;
				case 5:
					instruction.OpCode = OpCodes.Ldc_I4_5;
					instruction.Operand = null;
					break;
				case 6:
					instruction.OpCode = OpCodes.Ldc_I4_6;
					instruction.Operand = null;
					break;
				case 7:
					instruction.OpCode = OpCodes.Ldc_I4_7;
					instruction.Operand = null;
					break;
				case 8:
					instruction.OpCode = OpCodes.Ldc_I4_8;
					instruction.Operand = null;
					break;
				case -1:
					instruction.OpCode = OpCodes.Ldc_I4_M1;
					instruction.Operand = null;
					break;
				default:
					if (-128 <= num && num <= 127)
					{
						instruction.OpCode = OpCodes.Ldc_I4_S;
						instruction.Operand = (sbyte)num;
					}
					break;
				}
				break;
			}
			case Code.Ldloc_S:
			case Code.Ldloc:
				if (instruction.Operand is Local local2)
				{
					if (local2.Index == 0)
					{
						instruction.OpCode = OpCodes.Ldloc_0;
						instruction.Operand = null;
					}
					else if (local2.Index == 1)
					{
						instruction.OpCode = OpCodes.Ldloc_1;
						instruction.Operand = null;
					}
					else if (local2.Index == 2)
					{
						instruction.OpCode = OpCodes.Ldloc_2;
						instruction.Operand = null;
					}
					else if (local2.Index == 3)
					{
						instruction.OpCode = OpCodes.Ldloc_3;
						instruction.Operand = null;
					}
					else if (0 <= local2.Index && local2.Index <= 255)
					{
						instruction.OpCode = OpCodes.Ldloc_S;
					}
				}
				break;
			case Code.Ldloca:
				if (instruction.Operand is Local local3 && 0 <= local3.Index && local3.Index <= 255)
				{
					instruction.OpCode = OpCodes.Ldloca_S;
				}
				break;
			case Code.Starg:
				if (instruction.Operand is Parameter parameter3 && 0 <= parameter3.Index && parameter3.Index <= 255)
				{
					instruction.OpCode = OpCodes.Starg_S;
				}
				break;
			case Code.Stloc_S:
			case Code.Stloc:
				if (instruction.Operand is Local local)
				{
					if (local.Index == 0)
					{
						instruction.OpCode = OpCodes.Stloc_0;
						instruction.Operand = null;
					}
					else if (local.Index == 1)
					{
						instruction.OpCode = OpCodes.Stloc_1;
						instruction.Operand = null;
					}
					else if (local.Index == 2)
					{
						instruction.OpCode = OpCodes.Stloc_2;
						instruction.Operand = null;
					}
					else if (local.Index == 3)
					{
						instruction.OpCode = OpCodes.Stloc_3;
						instruction.Operand = null;
					}
					else if (0 <= local.Index && local.Index <= 255)
					{
						instruction.OpCode = OpCodes.Stloc_S;
					}
				}
				break;
			}
		}
		instructions.OptimizeBranches();
	}

	public static void SimplifyBranches(this IList<Instruction> instructions)
	{
		int count = instructions.Count;
		for (int i = 0; i < count; i++)
		{
			Instruction instruction = instructions[i];
			switch (instruction.OpCode.Code)
			{
			case Code.Beq_S:
				instruction.OpCode = OpCodes.Beq;
				break;
			case Code.Bge_S:
				instruction.OpCode = OpCodes.Bge;
				break;
			case Code.Bgt_S:
				instruction.OpCode = OpCodes.Bgt;
				break;
			case Code.Ble_S:
				instruction.OpCode = OpCodes.Ble;
				break;
			case Code.Blt_S:
				instruction.OpCode = OpCodes.Blt;
				break;
			case Code.Bne_Un_S:
				instruction.OpCode = OpCodes.Bne_Un;
				break;
			case Code.Bge_Un_S:
				instruction.OpCode = OpCodes.Bge_Un;
				break;
			case Code.Bgt_Un_S:
				instruction.OpCode = OpCodes.Bgt_Un;
				break;
			case Code.Ble_Un_S:
				instruction.OpCode = OpCodes.Ble_Un;
				break;
			case Code.Blt_Un_S:
				instruction.OpCode = OpCodes.Blt_Un;
				break;
			case Code.Br_S:
				instruction.OpCode = OpCodes.Br;
				break;
			case Code.Brfalse_S:
				instruction.OpCode = OpCodes.Brfalse;
				break;
			case Code.Brtrue_S:
				instruction.OpCode = OpCodes.Brtrue;
				break;
			case Code.Leave_S:
				instruction.OpCode = OpCodes.Leave;
				break;
			}
		}
	}

	public static void OptimizeBranches(this IList<Instruction> instructions)
	{
		bool flag;
		do
		{
			instructions.UpdateInstructionOffsets();
			flag = false;
			int count = instructions.Count;
			for (int i = 0; i < count; i++)
			{
				Instruction instruction = instructions[i];
				OpCode opCode;
				switch (instruction.OpCode.Code)
				{
				case Code.Beq:
					opCode = OpCodes.Beq_S;
					break;
				case Code.Bge:
					opCode = OpCodes.Bge_S;
					break;
				case Code.Bge_Un:
					opCode = OpCodes.Bge_Un_S;
					break;
				case Code.Bgt:
					opCode = OpCodes.Bgt_S;
					break;
				case Code.Bgt_Un:
					opCode = OpCodes.Bgt_Un_S;
					break;
				case Code.Ble:
					opCode = OpCodes.Ble_S;
					break;
				case Code.Ble_Un:
					opCode = OpCodes.Ble_Un_S;
					break;
				case Code.Blt:
					opCode = OpCodes.Blt_S;
					break;
				case Code.Blt_Un:
					opCode = OpCodes.Blt_Un_S;
					break;
				case Code.Bne_Un:
					opCode = OpCodes.Bne_Un_S;
					break;
				case Code.Br:
					opCode = OpCodes.Br_S;
					break;
				case Code.Brfalse:
					opCode = OpCodes.Brfalse_S;
					break;
				case Code.Brtrue:
					opCode = OpCodes.Brtrue_S;
					break;
				case Code.Leave:
					opCode = OpCodes.Leave_S;
					break;
				default:
					continue;
				}
				if (instruction.Operand is Instruction instruction2)
				{
					int num = ((instruction2.Offset < instruction.Offset) ? ((int)instruction.Offset + opCode.Size + 1) : ((int)instruction.Offset + instruction.GetSize()));
					int num2 = (int)instruction2.Offset - num;
					if (-128 <= num2 && num2 <= 127)
					{
						instruction.OpCode = opCode;
						flag = true;
					}
				}
			}
		}
		while (flag);
	}

	public static uint UpdateInstructionOffsets(this IList<Instruction> instructions)
	{
		uint num = 0u;
		int count = instructions.Count;
		for (int i = 0; i < count; i++)
		{
			Instruction instruction = instructions[i];
			instruction.Offset = num;
			num += (uint)instruction.GetSize();
		}
		return num;
	}
}
