using System;
using System.Collections.Generic;
using dnlib.DotNet;
using dnlib.DotNet.Emit;
using Obfuscator.Helper;

namespace Obfuscator.Obfuscator.Mutation2
{
	// Token: 0x02000014 RID: 20
	public class StringLen : iMutation
	{
		// Token: 0x06000044 RID: 68 RVA: 0x000048E6 File Offset: 0x00002AE6
		public void Prepare(TypeDef type)
		{
		}

		// Token: 0x06000045 RID: 69 RVA: 0x000048E8 File Offset: 0x00002AE8
		public void Process(MethodDef method, ref int index)
		{
			if (method.DeclaringType == method.Module.GlobalType)
			{
				index--;
				return;
			}
			int ldcI4Value = method.Body.Instructions[index].GetLdcI4Value();
			int num = Methods.Random.Next(4, 15);
			string operand = Methods.GenerateString((byte)num);
			method.Body.Instructions[index].OpCode = OpCodes.Ldc_I4;
			method.Body.Instructions[index].Operand = ldcI4Value - num;
			IList<Instruction> instructions = method.Body.Instructions;
			int num2 = index + 1;
			index = num2;
			instructions.Insert(num2, new Instruction(OpCodes.Ldstr, operand));
			IList<Instruction> instructions2 = method.Body.Instructions;
			num2 = index + 1;
			index = num2;
			instructions2.Insert(num2, new Instruction(OpCodes.Call, method.Module.Import(typeof(string).GetMethod("get_Length"))));
			IList<Instruction> instructions3 = method.Body.Instructions;
			num2 = index + 1;
			index = num2;
			instructions3.Insert(num2, new Instruction(OpCodes.Add));
		}

		// Token: 0x06000046 RID: 70 RVA: 0x00004A01 File Offset: 0x00002C01
		public bool Supported(Instruction instr)
		{
			return Utils.CheckArithmetic(instr);
		}
	}
}
