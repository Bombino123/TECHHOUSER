using System;
using System.Collections.Generic;
using dnlib.DotNet;
using dnlib.DotNet.Emit;
using Obfuscator.Helper;

namespace Obfuscator.Obfuscator.Mutation2
{
	// Token: 0x02000016 RID: 22
	public class CharMutations : iMutation
	{
		// Token: 0x17000002 RID: 2
		// (get) Token: 0x0600004F RID: 79 RVA: 0x00004C5C File Offset: 0x00002E5C
		// (set) Token: 0x06000050 RID: 80 RVA: 0x00004C64 File Offset: 0x00002E64
		public MethodDef Converter { get; set; }

		// Token: 0x06000051 RID: 81 RVA: 0x00004C70 File Offset: 0x00002E70
		public void Prepare(TypeDef type)
		{
			MethodDef methodDef = ModuleDefMD.Load(typeof(FuncMutation).Module).ResolveTypeDef(MDToken.ToRID(typeof(FuncMutation).MetadataToken)).FindMethod("CharToInt");
			methodDef.Name = Methods.GenerateString();
			methodDef.DeclaringType = null;
			type.Methods.Add(methodDef);
			this.Converter = methodDef;
		}

		// Token: 0x06000052 RID: 82 RVA: 0x00004CE4 File Offset: 0x00002EE4
		public void Process(MethodDef method, ref int index)
		{
			IList<Instruction> instructions = method.Body.Instructions;
			int num = index + 1;
			index = num;
			instructions.Insert(num, new Instruction(OpCodes.Call, this.Converter));
		}

		// Token: 0x06000053 RID: 83 RVA: 0x00004D1A File Offset: 0x00002F1A
		public bool Supported(Instruction instr)
		{
			return Utils.CheckArithmetic(instr);
		}
	}
}
