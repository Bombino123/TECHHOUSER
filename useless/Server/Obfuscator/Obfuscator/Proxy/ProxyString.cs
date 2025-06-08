using System;
using dnlib.DotNet;
using dnlib.DotNet.Emit;
using Obfuscator.Helper;

namespace Obfuscator.Obfuscator.Proxy
{
	// Token: 0x0200000C RID: 12
	internal class ProxyString
	{
		// Token: 0x06000029 RID: 41 RVA: 0x000040C8 File Offset: 0x000022C8
		public static void Execute(ModuleDef module)
		{
			foreach (TypeDef typeDef in module.GetTypes())
			{
				if (!typeDef.IsGlobalModuleType)
				{
					foreach (MethodDef methodDef in typeDef.Methods)
					{
						if (methodDef.HasBody)
						{
							foreach (Instruction instruction in methodDef.Body.Instructions)
							{
								if (instruction.OpCode == OpCodes.Ldstr)
								{
									MethodImplAttributes implFlags = MethodImplAttributes.IL;
									MethodAttributes flags = MethodAttributes.FamANDAssem | MethodAttributes.Family | MethodAttributes.Static | MethodAttributes.HideBySig;
									MethodDefUser methodDefUser = new MethodDefUser(Methods.GenerateString(), MethodSig.CreateStatic(module.CorLibTypes.String), implFlags, flags);
									module.GlobalType.Methods.Add(methodDefUser);
									methodDefUser.Body = new CilBody();
									methodDefUser.Body.Variables.Add(new Local(module.CorLibTypes.String));
									methodDefUser.Body.Instructions.Add(Instruction.Create(OpCodes.Ldstr, instruction.Operand.ToString()));
									methodDefUser.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));
									instruction.OpCode = OpCodes.Call;
									instruction.Operand = methodDefUser;
								}
							}
						}
					}
				}
			}
		}
	}
}
