using System;
using System.Collections.Generic;
using System.Linq;
using dnlib.DotNet;
using dnlib.DotNet.Emit;

namespace Obfuscator.Obfuscator.Proxy
{
	// Token: 0x02000009 RID: 9
	public static class ProxyMeth
	{
		// Token: 0x06000022 RID: 34 RVA: 0x00003A1C File Offset: 0x00001C1C
		public static void ScanMemberRef(ModuleDef module)
		{
			foreach (TypeDef typeDef in module.Types)
			{
				foreach (MethodDef methodDef in typeDef.Methods)
				{
					if (methodDef.HasBody && methodDef.Body.HasInstructions)
					{
						for (int i = 0; i < methodDef.Body.Instructions.Count - 1; i++)
						{
							if (methodDef.Body.Instructions[i].OpCode == OpCodes.Call)
							{
								try
								{
									MemberRef memberRef = (MemberRef)methodDef.Body.Instructions[i].Operand;
									if (!memberRef.HasThis)
									{
										ProxyMeth.MemberRefList.Add(memberRef);
									}
								}
								catch
								{
								}
							}
						}
					}
				}
			}
		}

		// Token: 0x06000023 RID: 35 RVA: 0x00003B34 File Offset: 0x00001D34
		public static MethodDef GenerateSwitch(MemberRef original, ModuleDef md)
		{
			MethodDef result;
			try
			{
				List<TypeSig> list = original.MethodSig.Params.ToList<TypeSig>();
				list.Add(md.CorLibTypes.Int32);
				MethodImplAttributes implFlags = MethodImplAttributes.IL;
				MethodAttributes flags = MethodAttributes.FamANDAssem | MethodAttributes.Family | MethodAttributes.Static | MethodAttributes.HideBySig;
				MethodDef methodDef = new MethodDefUser(string.Format("ProxyMeth{0}", ProxyMeth.rand.Next(0, int.MaxValue)), MethodSig.CreateStatic(original.MethodSig.RetType, list.ToArray()), implFlags, flags)
				{
					Body = new CilBody()
				};
				methodDef.Body.Variables.Add(new Local(md.CorLibTypes.Int32));
				methodDef.Body.Variables.Add(new Local(md.CorLibTypes.Int32));
				methodDef.Body.Instructions.Add(Instruction.Create(OpCodes.Ldarg_0));
				List<Instruction> list2 = new List<Instruction>();
				Instruction instruction = new Instruction(OpCodes.Switch);
				methodDef.Body.Instructions.Add(instruction);
				Instruction instruction2 = new Instruction(OpCodes.Br_S);
				methodDef.Body.Instructions.Add(instruction2);
				for (int i = 0; i < 5; i++)
				{
					for (int j = 0; j <= original.MethodSig.Params.Count - 1; j++)
					{
						methodDef.Body.Instructions.Add(Instruction.Create(OpCodes.Ldarg, methodDef.Parameters[j]));
						if (j == 0)
						{
							list2.Add(Instruction.Create(OpCodes.Ldarg, methodDef.Parameters[j]));
						}
					}
					Instruction item = Instruction.Create(OpCodes.Ldc_I4, i);
					methodDef.Body.Instructions.Add(item);
					methodDef.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));
				}
				Instruction instruction3 = Instruction.Create(OpCodes.Ldnull);
				methodDef.Body.Instructions.Add(instruction3);
				methodDef.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));
				instruction2.Operand = instruction3;
				instruction.Operand = list2;
				result = methodDef;
			}
			catch
			{
				result = null;
			}
			return result;
		}

		// Token: 0x06000024 RID: 36 RVA: 0x00003D84 File Offset: 0x00001F84
		public static void Execute(ModuleDef module)
		{
			ProxyMeth.ScanMemberRef(module);
			foreach (TypeDef typeDef in module.GetTypes())
			{
				if (!typeDef.IsGlobalModuleType)
				{
					foreach (MethodDef methodDef in typeDef.Methods.ToArray<MethodDef>())
					{
						if (methodDef.HasBody && !methodDef.Name.Contains("ProxyMeth"))
						{
							IList<Instruction> instructions = methodDef.Body.Instructions;
							for (int j = 0; j < instructions.Count; j++)
							{
								if (methodDef.Body.Instructions[j].OpCode == OpCodes.Call)
								{
									try
									{
										MemberRef original = (MemberRef)methodDef.Body.Instructions[j].Operand;
										if (!original.HasThis)
										{
											MethodDef methodDef2 = ProxyMeth.GenerateSwitch(original, module);
											methodDef.DeclaringType.Methods.Add(methodDef2);
											instructions[j].OpCode = OpCodes.Call;
											instructions[j].Operand = methodDef2;
											int value = ProxyMeth.rand.Next(0, 5);
											for (int k = 0; k < methodDef2.Body.Instructions.Count - 1; k++)
											{
												if (methodDef2.Body.Instructions[k].OpCode == OpCodes.Ldc_I4)
												{
													if (string.Compare(methodDef2.Body.Instructions[k].Operand.ToString(), value.ToString(), StringComparison.Ordinal) != 0)
													{
														methodDef2.Body.Instructions[k].OpCode = OpCodes.Call;
														Instruction instruction = methodDef2.Body.Instructions[k];
														instruction.Operand = ProxyMeth.MemberRefList
															.Where(m => m.MethodSig.Params.Count == original.MethodSig.Params.Count)
															.ToList<MemberRef>()
															.Random<MemberRef>();
													}
													else
													{
														methodDef2.Body.Instructions[k].OpCode = OpCodes.Call;
														methodDef2.Body.Instructions[k].Operand = original;
													}
												}
											}
											methodDef.Body.Instructions.Insert(j, Instruction.CreateLdcI4(value));
										}
									}
									catch
									{
									}
								}
							}
						}
					}
				}
			}
		}

		// Token: 0x0400000E RID: 14
		public static Random rand = new Random();

		// Token: 0x0400000F RID: 15
		public static List<MemberRef> MemberRefList = new List<MemberRef>();
	}
}
