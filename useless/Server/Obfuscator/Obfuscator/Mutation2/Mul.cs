using System;
using System.Collections.Generic;
using dnlib.DotNet;
using dnlib.DotNet.Emit;
using Obfuscator.Helper;

namespace Obfuscator.Obfuscator.Mutation2
{
	// Token: 0x02000011 RID: 17
	public class Mul : iMutation
	{
		// Token: 0x06000038 RID: 56 RVA: 0x000046A0 File Offset: 0x000028A0
		public void Prepare(TypeDef type)
		{
		}

		// Token: 0x06000039 RID: 57 RVA: 0x000046A4 File Offset: 0x000028A4
		public void Process(MethodDef method, ref int index)
		{
			int ldcI4Value = method.Body.Instructions[index].GetLdcI4Value();
			int num = Methods.Random.Next(1, (int)((double)ldcI4Value / 1.5));
			int num2 = ldcI4Value / num;
			while (num * num2 != ldcI4Value)
			{
				num = Methods.Random.Next(1, (int)((double)ldcI4Value / 1.5));
				num2 = ldcI4Value / num;
			}
			method.Body.Instructions[index].OpCode = OpCodes.Ldc_I4;
			method.Body.Instructions[index].Operand = num2;
			IList<Instruction> instructions = method.Body.Instructions;
			int num3 = index + 1;
			index = num3;
			instructions.Insert(num3, new Instruction(OpCodes.Ldc_I4, num));
			IList<Instruction> instructions2 = method.Body.Instructions;
			num3 = index + 1;
			index = num3;
			instructions2.Insert(num3, new Instruction(OpCodes.Mul));
			index++;
		}

		// Token: 0x0600003A RID: 58 RVA: 0x00004796 File Offset: 0x00002996
		public bool Supported(Instruction instr)
		{
			return Utils.CheckArithmetic(instr);
		}
	}
}
