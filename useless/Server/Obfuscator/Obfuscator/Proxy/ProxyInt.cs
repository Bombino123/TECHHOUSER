using System;
using System.Collections.Generic;
using dnlib.DotNet;
using dnlib.DotNet.Emit;
using Obfuscator.Helper;

namespace Obfuscator.Obfuscator.Proxy
{
	// Token: 0x02000008 RID: 8
	public static class ProxyInt
	{
		// Token: 0x06000021 RID: 33 RVA: 0x00003740 File Offset: 0x00001940
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
							IList<Instruction> instructions = methodDef.Body.Instructions;
							for (int i = 0; i < instructions.Count; i++)
							{
								if (methodDef.Body.Instructions[i].IsLdcI4())
								{
									MethodImplAttributes implFlags = MethodImplAttributes.IL;
									MethodAttributes flags = MethodAttributes.FamANDAssem | MethodAttributes.Family | MethodAttributes.Static | MethodAttributes.HideBySig;
									MethodDefUser methodDefUser = new MethodDefUser(Methods.GenerateString(), MethodSig.CreateStatic(module.CorLibTypes.Int32), implFlags, flags);
									module.GlobalType.Methods.Add(methodDefUser);
									methodDefUser.Body = new CilBody();
									methodDefUser.Body.Variables.Add(new Local(module.CorLibTypes.Int32));
									methodDefUser.Body.Instructions.Add(Instruction.Create(OpCodes.Ldc_I4, instructions[i].GetLdcI4Value()));
									methodDefUser.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));
									instructions[i].OpCode = OpCodes.Call;
									instructions[i].Operand = methodDefUser;
								}
								else if (methodDef.Body.Instructions[i].OpCode == OpCodes.Ldc_R4)
								{
									MethodImplAttributes implFlags2 = MethodImplAttributes.IL;
									MethodAttributes flags2 = MethodAttributes.FamANDAssem | MethodAttributes.Family | MethodAttributes.Static | MethodAttributes.HideBySig;
									MethodDefUser methodDefUser2 = new MethodDefUser(Methods.GenerateString(), MethodSig.CreateStatic(module.CorLibTypes.Double), implFlags2, flags2);
									module.GlobalType.Methods.Add(methodDefUser2);
									methodDefUser2.Body = new CilBody();
									methodDefUser2.Body.Variables.Add(new Local(module.CorLibTypes.Double));
									methodDefUser2.Body.Instructions.Add(Instruction.Create(OpCodes.Ldc_R4, (float)methodDef.Body.Instructions[i].Operand));
									methodDefUser2.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));
									instructions[i].OpCode = OpCodes.Call;
									instructions[i].Operand = methodDefUser2;
								}
							}
						}
					}
				}
			}
		}
	}
}
