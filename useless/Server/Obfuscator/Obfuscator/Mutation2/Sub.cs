using System;
using System.Collections.Generic;
using dnlib.DotNet;
using dnlib.DotNet.Emit;
using Obfuscator.Helper;

namespace Obfuscator.Obfuscator.Mutation2
{
	// Token: 0x02000010 RID: 16
	public class Sub : iMutation
	{
		// Token: 0x06000034 RID: 52 RVA: 0x000045C8 File Offset: 0x000027C8
		public void Prepare(TypeDef type)
		{
		}

		// Token: 0x06000035 RID: 53 RVA: 0x000045CC File Offset: 0x000027CC
		public void Process(MethodDef method, ref int index)
		{
			int ldcI4Value = method.Body.Instructions[index].GetLdcI4Value();
			int num = Methods.Random.Next((int)((double)ldcI4Value / 1.5));
			method.Body.Instructions[index].OpCode = OpCodes.Ldc_I4;
			method.Body.Instructions[index].Operand = ldcI4Value + num;
			IList<Instruction> instructions = method.Body.Instructions;
			int num2 = index + 1;
			index = num2;
			instructions.Insert(num2, new Instruction(OpCodes.Ldc_I4, num));
			IList<Instruction> instructions2 = method.Body.Instructions;
			num2 = index + 1;
			index = num2;
			instructions2.Insert(num2, new Instruction(OpCodes.Sub));
		}

		// Token: 0x06000036 RID: 54 RVA: 0x00004690 File Offset: 0x00002890
		public bool Supported(Instruction instr)
		{
			return Utils.CheckArithmetic(instr);
		}
	}
}
