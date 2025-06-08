using System;
using dnlib.DotNet;
using dnlib.DotNet.Emit;

namespace Obfuscator.Obfuscator.Mutation2
{
	// Token: 0x0200001A RID: 26
	public interface iMutation
	{
		// Token: 0x06000063 RID: 99
		void Process(MethodDef method, ref int index);

		// Token: 0x06000064 RID: 100
		void Prepare(TypeDef type);

		// Token: 0x06000065 RID: 101
		bool Supported(Instruction instr);
	}
}
