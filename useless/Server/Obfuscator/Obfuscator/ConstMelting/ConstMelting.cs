using System;
using System.Linq;
using dnlib.DotNet;
using dnlib.DotNet.Emit;
using Obfuscator.Helper;

namespace Obfuscator.Obfuscator.ConstMelting
{
	// Token: 0x0200002D RID: 45
	internal class ConstMelting
	{
		// Token: 0x060000BE RID: 190 RVA: 0x000098A4 File Offset: 0x00007AA4
		public static void Execute(ModuleDef moduleDef)
		{
			TypeDef[] array = moduleDef.Types.ToArray<TypeDef>();
			for (int i = 0; i < array.Length; i++)
			{
				foreach (MethodDef methodDef in array[i].Methods.ToArray<MethodDef>())
				{
					ConstMelting.ReplaceStringLiterals(methodDef);
					ConstMelting.ReplaceIntLiterals(methodDef);
				}
			}
		}

		// Token: 0x060000BF RID: 191 RVA: 0x000098F8 File Offset: 0x00007AF8
		private static void ReplaceStringLiterals(MethodDef methodDef)
		{
			if (ConstMelting.CanObfuscate(methodDef))
			{
				foreach (Instruction instruction in methodDef.Body.Instructions)
				{
					if (instruction.OpCode == OpCodes.Ldstr)
					{
						MethodDef methodDef2 = new MethodDefUser(Methods.GenerateString(), MethodSig.CreateStatic(methodDef.DeclaringType.Module.CorLibTypes.String), MethodImplAttributes.IL, MethodAttributes.FamANDAssem | MethodAttributes.Family | MethodAttributes.Static | MethodAttributes.HideBySig)
						{
							Body = new CilBody()
						};
						methodDef2.Body.Instructions.Add(new Instruction(OpCodes.Ldstr, instruction.Operand.ToString()));
						methodDef2.Body.Instructions.Add(new Instruction(OpCodes.Ret));
						methodDef.DeclaringType.Methods.Add(methodDef2);
						instruction.OpCode = OpCodes.Call;
						instruction.Operand = methodDef2;
					}
				}
			}
		}

		// Token: 0x060000C0 RID: 192 RVA: 0x00009A04 File Offset: 0x00007C04
		private static void ReplaceIntLiterals(MethodDef methodDef)
		{
			if (ConstMelting.CanObfuscate(methodDef))
			{
				foreach (Instruction instruction in methodDef.Body.Instructions)
				{
					if (instruction.OpCode == OpCodes.Ldc_I4)
					{
						MethodDef methodDef2 = new MethodDefUser(Methods.GenerateString(), MethodSig.CreateStatic(methodDef.DeclaringType.Module.CorLibTypes.Int32), MethodImplAttributes.IL, MethodAttributes.FamANDAssem | MethodAttributes.Family | MethodAttributes.Static | MethodAttributes.HideBySig)
						{
							Body = new CilBody()
						};
						methodDef2.Body.Instructions.Add(new Instruction(OpCodes.Ldc_I4, instruction.GetLdcI4Value()));
						methodDef2.Body.Instructions.Add(new Instruction(OpCodes.Ret));
						methodDef.DeclaringType.Methods.Add(methodDef2);
						instruction.OpCode = OpCodes.Call;
						instruction.Operand = methodDef2;
					}
				}
			}
		}

		// Token: 0x060000C1 RID: 193 RVA: 0x00009B10 File Offset: 0x00007D10
		public static bool CanObfuscate(MethodDef methodDef)
		{
			return methodDef.HasBody && methodDef.Body.HasInstructions && !methodDef.DeclaringType.IsGlobalModuleType;
		}
	}
}
