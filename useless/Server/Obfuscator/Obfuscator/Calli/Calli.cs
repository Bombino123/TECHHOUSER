using System;
using System.Linq;
using dnlib.DotNet;
using dnlib.DotNet.Emit;

namespace Obfuscator.Obfuscator.Calli
{
	// Token: 0x0200002E RID: 46
	internal class Calli
	{
		// Token: 0x060000C3 RID: 195 RVA: 0x00009B44 File Offset: 0x00007D44
		public static void Execute(ModuleDef module)
		{
			TypeDef[] array = module.Types.ToArray<TypeDef>();
			for (int i = 0; i < array.Length; i++)
			{
				foreach (MethodDef methodDef in array[i].Methods.ToArray<MethodDef>())
				{
					if (methodDef.HasBody && methodDef.Body.HasInstructions && !methodDef.FullName.Contains("My.") && !methodDef.FullName.Contains(".My") && !methodDef.FullName.Contains("Costura") && !methodDef.IsConstructor && !methodDef.DeclaringType.IsGlobalModuleType)
					{
						for (int k = 0; k < methodDef.Body.Instructions.Count - 1; k++)
						{
							try
							{
								if (!methodDef.Body.Instructions[k].ToString().Contains("ISupportInitialize") && (methodDef.Body.Instructions[k].OpCode == OpCodes.Call || methodDef.Body.Instructions[k].OpCode == OpCodes.Callvirt || methodDef.Body.Instructions[k].OpCode == OpCodes.Ldloc_S))
								{
									if (!methodDef.Body.Instructions[k].ToString().Contains("Object") && (methodDef.Body.Instructions[k].OpCode == OpCodes.Call || methodDef.Body.Instructions[k].OpCode == OpCodes.Callvirt || methodDef.Body.Instructions[k].OpCode == OpCodes.Ldloc_S))
									{
										try
										{
											MemberRef memberRef = (MemberRef)methodDef.Body.Instructions[k].Operand;
											methodDef.Body.Instructions[k].OpCode = OpCodes.Calli;
											methodDef.Body.Instructions[k].Operand = memberRef.MethodSig;
											methodDef.Body.Instructions.Insert(k, Instruction.Create(OpCodes.Ldftn, memberRef));
										}
										catch (Exception)
										{
										}
									}
								}
							}
							catch (Exception)
							{
							}
						}
					}
				}
				foreach (MethodDef methodDef2 in module.GlobalType.Methods)
				{
					if (!(methodDef2.Name != ".ctor"))
					{
						module.GlobalType.Remove(methodDef2);
						break;
					}
				}
			}
		}
	}
}
