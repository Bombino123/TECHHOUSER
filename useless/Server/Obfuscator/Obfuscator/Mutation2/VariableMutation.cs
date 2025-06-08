using System;
using dnlib.DotNet;
using dnlib.DotNet.Emit;

namespace Obfuscator.Obfuscator.Mutation2
{
	// Token: 0x02000017 RID: 23
	public class VariableMutation : iMutation
	{
		// Token: 0x06000055 RID: 85 RVA: 0x00004D2A File Offset: 0x00002F2A
		public void Prepare(TypeDef type)
		{
		}

		// Token: 0x06000056 RID: 86 RVA: 0x00004D2C File Offset: 0x00002F2C
		public void Process(MethodDef method, ref int index)
		{
			int ldcI4Value = method.Body.Instructions[index].GetLdcI4Value();
			Local local = new Local(method.Module.CorLibTypes.Int32);
			method.Body.Variables.Add(local);
			method.Body.Instructions.Insert(0, new Instruction(OpCodes.Stloc, local));
			method.Body.Instructions.Insert(0, new Instruction(OpCodes.Ldc_I4, ldcI4Value));
			index += 2;
			method.Body.Instructions[index] = new Instruction(OpCodes.Ldloc, local);
		}

		// Token: 0x06000057 RID: 87 RVA: 0x00004DD9 File Offset: 0x00002FD9
		public bool Supported(Instruction instr)
		{
			return Utils.CheckArithmetic(instr);
		}
	}
}
