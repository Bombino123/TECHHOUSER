using System;
using System.Collections.Generic;
using dnlib.DotNet;
using dnlib.DotNet.Emit;

namespace Obfuscator.Obfuscator.Mutation2
{
	// Token: 0x02000019 RID: 25
	public class MulToShift : iMutation
	{
		// Token: 0x0600005F RID: 95 RVA: 0x00005304 File Offset: 0x00003504
		public void Prepare(TypeDef type)
		{
		}

		// Token: 0x06000060 RID: 96 RVA: 0x00005308 File Offset: 0x00003508
		public void Process(MethodDef method, ref int index)
		{
			if (method.Body.Instructions[index - 1].IsLdcI4() && method.Body.Instructions[index - 2].IsLdcI4())
			{
				int ldcI4Value = method.Body.Instructions[index - 2].GetLdcI4Value();
				int ldcI4Value2 = method.Body.Instructions[index - 1].GetLdcI4Value();
				if (ldcI4Value2 >= 3)
				{
					Local local = new Local(method.Module.CorLibTypes.Int32);
					method.Body.Variables.Add(local);
					method.Body.Instructions.Insert(0, new Instruction(OpCodes.Stloc, local));
					method.Body.Instructions.Insert(0, new Instruction(OpCodes.Ldc_I4, ldcI4Value));
					index += 2;
					method.Body.Instructions[index - 2].OpCode = OpCodes.Ldloc;
					method.Body.Instructions[index - 2].Operand = local;
					method.Body.Instructions[index - 1].OpCode = OpCodes.Nop;
					method.Body.Instructions[index].OpCode = OpCodes.Nop;
					int num = 0;
					for (int i = ldcI4Value2; i > 0; i >>= 1)
					{
						if ((i & 1) == 1 && num != 0)
						{
							IList<Instruction> instructions = method.Body.Instructions;
							int num2 = index + 1;
							index = num2;
							instructions.Insert(num2, new Instruction(OpCodes.Ldloc, local));
							IList<Instruction> instructions2 = method.Body.Instructions;
							num2 = index + 1;
							index = num2;
							instructions2.Insert(num2, new Instruction(OpCodes.Ldc_I4, num));
							IList<Instruction> instructions3 = method.Body.Instructions;
							num2 = index + 1;
							index = num2;
							instructions3.Insert(num2, new Instruction(OpCodes.Shl));
							IList<Instruction> instructions4 = method.Body.Instructions;
							num2 = index + 1;
							index = num2;
							instructions4.Insert(num2, new Instruction(OpCodes.Add));
						}
						num++;
					}
					if ((ldcI4Value2 & 1) == 0)
					{
						IList<Instruction> instructions5 = method.Body.Instructions;
						int num2 = index + 1;
						index = num2;
						instructions5.Insert(num2, new Instruction(OpCodes.Ldloc, local));
						IList<Instruction> instructions6 = method.Body.Instructions;
						num2 = index + 1;
						index = num2;
						instructions6.Insert(num2, new Instruction(OpCodes.Sub));
					}
				}
			}
		}

		// Token: 0x06000061 RID: 97 RVA: 0x00005586 File Offset: 0x00003786
		public bool Supported(Instruction instr)
		{
			return instr.OpCode == OpCodes.Mul;
		}
	}
}
