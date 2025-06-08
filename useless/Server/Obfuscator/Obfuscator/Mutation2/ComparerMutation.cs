using System;
using System.Collections.Generic;
using dnlib.DotNet;
using dnlib.DotNet.Emit;
using Obfuscator.Helper;

namespace Obfuscator.Obfuscator.Mutation2
{
	// Token: 0x02000018 RID: 24
	public class ComparerMutation : iMutation
	{
		// Token: 0x06000059 RID: 89 RVA: 0x00004DEC File Offset: 0x00002FEC
		public void Prepare(TypeDef type)
		{
			if (type != type.Module.GlobalType)
			{
				for (int i = 0; i < type.Methods.Count; i++)
				{
					MethodDef methodDef = type.Methods[i];
					if (methodDef.HasBody && !methodDef.IsConstructor)
					{
						methodDef.Body.SimplifyBranches();
						for (int j = 0; j < methodDef.Body.Instructions.Count; j++)
						{
							if (Utils.CheckArithmetic(methodDef.Body.Instructions[j]))
							{
								this.Execute(methodDef, ref j);
							}
						}
					}
				}
			}
		}

		// Token: 0x0600005A RID: 90 RVA: 0x00004E84 File Offset: 0x00003084
		public void Execute(MethodDef method, ref int index)
		{
			if (method.Body.Instructions[index].OpCode != OpCodes.Call)
			{
				int ldcI4Value = method.Body.Instructions[index].GetLdcI4Value();
				Local local = new Local(method.Module.CorLibTypes.Int32);
				Local local2 = new Local(method.Module.CorLibTypes.Int32);
				method.Body.Variables.Add(local);
				method.Body.Variables.Add(local2);
				int num = Methods.Random.Next();
				int num2 = Methods.Random.Next();
				bool flag = Convert.ToBoolean(Methods.Random.Next(2));
				int num3;
				if (flag)
				{
					num3 = num2 - num;
				}
				else
				{
					num3 = Methods.Random.Next();
					while (num3 + num == num2)
					{
						num3 = Methods.Random.Next();
					}
				}
				method.Body.Instructions[index] = Instruction.CreateLdcI4(num);
				IList<Instruction> instructions = method.Body.Instructions;
				int num4 = index + 1;
				index = num4;
				instructions.Insert(num4, new Instruction(OpCodes.Stloc, local));
				IList<Instruction> instructions2 = method.Body.Instructions;
				num4 = index + 1;
				index = num4;
				instructions2.Insert(num4, Instruction.CreateLdcI4(num3));
				IList<Instruction> instructions3 = method.Body.Instructions;
				num4 = index + 1;
				index = num4;
				instructions3.Insert(num4, new Instruction(OpCodes.Stloc, local2));
				IList<Instruction> instructions4 = method.Body.Instructions;
				num4 = index + 1;
				index = num4;
				instructions4.Insert(num4, new Instruction(OpCodes.Ldloc, local));
				IList<Instruction> instructions5 = method.Body.Instructions;
				num4 = index + 1;
				index = num4;
				instructions5.Insert(num4, new Instruction(OpCodes.Ldloc, local2));
				IList<Instruction> instructions6 = method.Body.Instructions;
				num4 = index + 1;
				index = num4;
				instructions6.Insert(num4, new Instruction(OpCodes.Add));
				IList<Instruction> instructions7 = method.Body.Instructions;
				num4 = index + 1;
				index = num4;
				instructions7.Insert(num4, new Instruction(OpCodes.Ldc_I4, num2));
				IList<Instruction> instructions8 = method.Body.Instructions;
				num4 = index + 1;
				index = num4;
				instructions8.Insert(num4, new Instruction(OpCodes.Ceq));
				Instruction instruction = OpCodes.Nop.ToInstruction();
				IList<Instruction> instructions9 = method.Body.Instructions;
				num4 = index + 1;
				index = num4;
				instructions9.Insert(num4, new Instruction(flag ? OpCodes.Brfalse : OpCodes.Brtrue, instruction));
				IList<Instruction> instructions10 = method.Body.Instructions;
				num4 = index + 1;
				index = num4;
				instructions10.Insert(num4, new Instruction(OpCodes.Nop));
				IList<Instruction> instructions11 = method.Body.Instructions;
				num4 = index + 1;
				index = num4;
				instructions11.Insert(num4, new Instruction(OpCodes.Ldc_I4, ldcI4Value));
				IList<Instruction> instructions12 = method.Body.Instructions;
				num4 = index + 1;
				index = num4;
				instructions12.Insert(num4, new Instruction(OpCodes.Stloc, local));
				IList<Instruction> instructions13 = method.Body.Instructions;
				num4 = index + 1;
				index = num4;
				instructions13.Insert(num4, new Instruction(OpCodes.Nop));
				Instruction instruction2 = OpCodes.Ldloc_S.ToInstruction(local);
				IList<Instruction> instructions14 = method.Body.Instructions;
				num4 = index + 1;
				index = num4;
				instructions14.Insert(num4, new Instruction(OpCodes.Br, instruction2));
				IList<Instruction> instructions15 = method.Body.Instructions;
				num4 = index + 1;
				index = num4;
				instructions15.Insert(num4, instruction);
				IList<Instruction> instructions16 = method.Body.Instructions;
				num4 = index + 1;
				index = num4;
				instructions16.Insert(num4, new Instruction(OpCodes.Ldc_I4, Methods.Random.Next()));
				IList<Instruction> instructions17 = method.Body.Instructions;
				num4 = index + 1;
				index = num4;
				instructions17.Insert(num4, new Instruction(OpCodes.Stloc, local));
				IList<Instruction> instructions18 = method.Body.Instructions;
				num4 = index + 1;
				index = num4;
				instructions18.Insert(num4, new Instruction(OpCodes.Nop));
				IList<Instruction> instructions19 = method.Body.Instructions;
				num4 = index + 1;
				index = num4;
				instructions19.Insert(num4, instruction2);
			}
		}

		// Token: 0x0600005B RID: 91 RVA: 0x00005292 File Offset: 0x00003492
		public void Process(MethodDef method, ref int index)
		{
		}

		// Token: 0x0600005C RID: 92 RVA: 0x00005294 File Offset: 0x00003494
		public static void InsertInstructions(IList<Instruction> instructions, Dictionary<Instruction, int> keyValuePairs)
		{
			foreach (KeyValuePair<Instruction, int> keyValuePair in keyValuePairs)
			{
				Instruction key = keyValuePair.Key;
				int value = keyValuePair.Value;
				instructions.Insert(value, key);
			}
		}

		// Token: 0x0600005D RID: 93 RVA: 0x000052F4 File Offset: 0x000034F4
		public bool Supported(Instruction instr)
		{
			return Utils.CheckArithmetic(instr);
		}
	}
}
