using System;
using System.Collections.Generic;
using dnlib.DotNet;
using dnlib.DotNet.Emit;

namespace Obfuscator.Obfuscator.Mutation2
{
	// Token: 0x02000013 RID: 19
	public class Abs : iMutation
	{
		// Token: 0x06000040 RID: 64 RVA: 0x00004871 File Offset: 0x00002A71
		public void Prepare(TypeDef type)
		{
		}

		// Token: 0x06000041 RID: 65 RVA: 0x00004874 File Offset: 0x00002A74
		public void Process(MethodDef method, ref int index)
		{
			IList<Instruction> instructions = method.Body.Instructions;
			int num = index + 1;
			index = num;
			instructions.Insert(num, new Instruction(OpCodes.Call, method.Module.Import(typeof(Math).GetMethod("Abs", new Type[]
			{
				typeof(int)
			}))));
		}

		// Token: 0x06000042 RID: 66 RVA: 0x000048D6 File Offset: 0x00002AD6
		public bool Supported(Instruction instr)
		{
			return Utils.CheckArithmetic(instr);
		}
	}
}
