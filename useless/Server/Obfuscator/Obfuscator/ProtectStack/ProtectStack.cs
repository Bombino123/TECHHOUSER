using System;
using dnlib.DotNet;
using dnlib.DotNet.Emit;

namespace Obfuscator.Obfuscator.ProtectStack
{
	// Token: 0x0200000D RID: 13
	internal class ProtectStack
	{
		// Token: 0x0600002B RID: 43 RVA: 0x000042B0 File Offset: 0x000024B0
		public static void Execute(ModuleDef mod)
		{
			foreach (TypeDef typeDef in mod.Types)
			{
				foreach (MethodDef methodDef in typeDef.Methods)
				{
					if (!methodDef.HasBody)
					{
						break;
					}
					CilBody body = methodDef.Body;
					Instruction instruction = body.Instructions[0];
					Instruction instruction2 = Instruction.Create(OpCodes.Br_S, instruction);
					Instruction item = Instruction.Create(OpCodes.Pop);
					Instruction randomInstruction = ProtectStack.GetRandomInstruction(new Random());
					body.Instructions.Insert(0, randomInstruction);
					body.Instructions.Insert(1, item);
					body.Instructions.Insert(2, instruction2);
					if (body.ExceptionHandlers != null)
					{
						foreach (ExceptionHandler exceptionHandler in body.ExceptionHandlers)
						{
							if (exceptionHandler.TryStart == instruction)
							{
								exceptionHandler.TryStart = instruction2;
							}
							else if (exceptionHandler.HandlerStart == instruction)
							{
								exceptionHandler.HandlerStart = instruction2;
							}
							else if (exceptionHandler.FilterStart == instruction)
							{
								exceptionHandler.FilterStart = instruction2;
							}
						}
					}
				}
			}
		}

		// Token: 0x0600002C RID: 44 RVA: 0x00004450 File Offset: 0x00002650
		private static Instruction GetRandomInstruction(Random random)
		{
			switch (random.Next(0, 5))
			{
			case 0:
				return Instruction.Create(OpCodes.Ldnull);
			case 1:
				return Instruction.Create(OpCodes.Ldc_I4_0);
			case 2:
				return Instruction.Create(OpCodes.Ldstr, "Isolator");
			case 3:
				return Instruction.Create(OpCodes.Ldc_I8, (long)((ulong)random.Next()));
			default:
				return Instruction.Create(OpCodes.Ldc_I8, (long)random.Next());
			}
		}
	}
}
