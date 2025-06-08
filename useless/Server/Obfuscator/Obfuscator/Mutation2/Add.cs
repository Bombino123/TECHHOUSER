using System;
using System.Collections.Generic;
using dnlib.DotNet;
using dnlib.DotNet.Emit;
using Obfuscator.Helper;

namespace Obfuscator.Obfuscator.Mutation2
{
	// Token: 0x0200000F RID: 15
	public class Add : iMutation
	{
		// Token: 0x06000030 RID: 48 RVA: 0x000044EF File Offset: 0x000026EF
		public void Prepare(TypeDef type)
		{
		}

		// Token: 0x06000031 RID: 49 RVA: 0x000044F4 File Offset: 0x000026F4
		public void Process(MethodDef method, ref int index)
		{
			int ldcI4Value = method.Body.Instructions[index].GetLdcI4Value();
			int num = Methods.Random.Next((int)((double)ldcI4Value / 1.5));
			method.Body.Instructions[index].OpCode = OpCodes.Ldc_I4;
			method.Body.Instructions[index].Operand = ldcI4Value - num;
			IList<Instruction> instructions = method.Body.Instructions;
			int num2 = index + 1;
			index = num2;
			instructions.Insert(num2, new Instruction(OpCodes.Ldc_I4, num));
			IList<Instruction> instructions2 = method.Body.Instructions;
			num2 = index + 1;
			index = num2;
			instructions2.Insert(num2, new Instruction(OpCodes.Add));
		}

		// Token: 0x06000032 RID: 50 RVA: 0x000045B8 File Offset: 0x000027B8
		public bool Supported(Instruction instr)
		{
			return Utils.CheckArithmetic(instr);
		}
	}
}
