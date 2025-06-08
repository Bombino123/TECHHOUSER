using System;
using System.Collections.Generic;
using dnlib.DotNet;
using dnlib.DotNet.Emit;
using Obfuscator.Helper;

namespace Obfuscator.Obfuscator.Mutation2
{
	// Token: 0x02000012 RID: 18
	public class Div : iMutation
	{
		// Token: 0x0600003C RID: 60 RVA: 0x000047A6 File Offset: 0x000029A6
		public void Prepare(TypeDef type)
		{
		}

		// Token: 0x0600003D RID: 61 RVA: 0x000047A8 File Offset: 0x000029A8
		public void Process(MethodDef method, ref int index)
		{
			int ldcI4Value = method.Body.Instructions[index].GetLdcI4Value();
			int num = Methods.Random.Next(1, 5);
			method.Body.Instructions[index].OpCode = OpCodes.Ldc_I4;
			method.Body.Instructions[index].Operand = ldcI4Value * num;
			IList<Instruction> instructions = method.Body.Instructions;
			int num2 = index + 1;
			index = num2;
			instructions.Insert(num2, new Instruction(OpCodes.Ldc_I4, num));
			IList<Instruction> instructions2 = method.Body.Instructions;
			num2 = index + 1;
			index = num2;
			instructions2.Insert(num2, new Instruction(OpCodes.Div));
		}

		// Token: 0x0600003E RID: 62 RVA: 0x00004861 File Offset: 0x00002A61
		public bool Supported(Instruction instr)
		{
			return Utils.CheckArithmetic(instr);
		}
	}
}
