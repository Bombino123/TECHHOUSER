using System;
using dnlib.DotNet;
using dnlib.DotNet.Emit;

namespace Obfuscator.Obfuscator.Invalid
{
	// Token: 0x02000024 RID: 36
	internal class InvalidOpcodes
	{
		// Token: 0x06000095 RID: 149 RVA: 0x00007C30 File Offset: 0x00005E30
		public static void Execute(ModuleDef module)
		{
			foreach (TypeDef typeDef in module.GetTypes())
			{
				foreach (MethodDef methodDef in typeDef.Methods)
				{
					if (methodDef.HasBody || methodDef.Body.HasInstructions)
					{
						methodDef.Body.Instructions.Insert(0, Instruction.Create(OpCodes.Box, methodDef.Module.Import(typeof(Math))));
					}
				}
			}
		}
	}
}
