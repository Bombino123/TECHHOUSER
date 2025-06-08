using System;
using System.Collections.Generic;
using System.Linq;
using dnlib.DotNet;
using dnlib.DotNet.Emit;

namespace Obfuscator.Obfuscator.Mutation1
{
	// Token: 0x0200001D RID: 29
	internal class Mutation1
	{
		// Token: 0x0600006C RID: 108 RVA: 0x0000572C File Offset: 0x0000392C
		public static void Execute(ModuleDefMD moduleDefMd)
		{
			Mutation1._moduleDefMd = moduleDefMd;
			MutationHelper.CryptoRandom cryptoRandom = new MutationHelper.CryptoRandom();
			foreach (TypeDef typeDef in moduleDefMd.GetTypes())
			{
				List<MethodDef> list = new List<MethodDef>();
				foreach (MethodDef methodDef in from x in typeDef.Methods
				where x.HasBody
				select x)
				{
					IList<Instruction> instructions = methodDef.Body.Instructions;
					for (int i = 0; i < instructions.Count; i++)
					{
						if (instructions[i].IsLdcI4() && Mutation1.IsSafe(instructions.ToList<Instruction>(), i))
						{
							MethodDef methodDef2 = null;
							int ldcI4Value = instructions[i].GetLdcI4Value();
							instructions[i].OpCode = OpCodes.Ldc_R8;
							switch (cryptoRandom.Next(0, 3))
							{
							case 0:
								methodDef2 = Mutation1.GenerateRefMethod("Floor");
								instructions[i].Operand = Convert.ToDouble((double)ldcI4Value + cryptoRandom.NextDouble());
								break;
							case 1:
								methodDef2 = Mutation1.GenerateRefMethod("Sqrt");
								instructions[i].Operand = Math.Pow(Convert.ToDouble(ldcI4Value), 2.0);
								break;
							case 2:
								methodDef2 = Mutation1.GenerateRefMethod("Round");
								instructions[i].Operand = Convert.ToDouble(ldcI4Value);
								break;
							}
							instructions.Insert(i + 1, OpCodes.Call.ToInstruction(methodDef2));
							instructions.Insert(i + 2, OpCodes.Conv_I4.ToInstruction());
							i += 2;
							list.Add(methodDef2);
						}
					}
					methodDef.Body.SimplifyMacros(methodDef.Parameters);
				}
				foreach (MethodDef item in list)
				{
					typeDef.Methods.Add(item);
				}
			}
		}

		// Token: 0x0600006D RID: 109 RVA: 0x000059CC File Offset: 0x00003BCC
		private static MethodDef GenerateRefMethod(string methodName)
		{
			MethodDefUser methodDefUser = new MethodDefUser("_" + Guid.NewGuid().ToString("D").ToUpper().Substring(2, 5), MethodSig.CreateStatic(Mutation1._moduleDefMd.ImportAsTypeSig(typeof(double))), MethodAttributes.Private | MethodAttributes.Static | MethodAttributes.HideBySig)
			{
				Signature = new MethodSig
				{
					Params = 
					{
						Mutation1._moduleDefMd.ImportAsTypeSig(typeof(double))
					},
					RetType = Mutation1._moduleDefMd.ImportAsTypeSig(typeof(double))
				}
			};
			CilBody body = new CilBody
			{
				Instructions = 
				{
					OpCodes.Ldarg_0.ToInstruction(),
					OpCodes.Call.ToInstruction(Mutation1.GetMethod(typeof(Math), methodName, new Type[]
					{
						typeof(double)
					})),
					OpCodes.Stloc_0.ToInstruction(),
					OpCodes.Ldloc_0.ToInstruction(),
					OpCodes.Ret.ToInstruction()
				}
			};
			methodDefUser.Body = body;
			methodDefUser.Body.Variables.Add(new Local(Mutation1._moduleDefMd.ImportAsTypeSig(typeof(double))));
			return methodDefUser.ResolveMethodDef();
		}

		// Token: 0x0600006E RID: 110 RVA: 0x00005B39 File Offset: 0x00003D39
		private static bool IsSafe(List<Instruction> instructions, int i)
		{
			return !new int[]
			{
				-2,
				-1,
				0,
				1,
				2
			}.Contains(instructions[i].GetLdcI4Value());
		}

		// Token: 0x0600006F RID: 111 RVA: 0x00005B62 File Offset: 0x00003D62
		private static IMethod GetMethod(Type type, string methodName, Type[] types)
		{
			return Mutation1._moduleDefMd.Import(type.GetMethod(methodName, types));
		}

		// Token: 0x04000015 RID: 21
		private static ModuleDefMD _moduleDefMd;
	}
}
