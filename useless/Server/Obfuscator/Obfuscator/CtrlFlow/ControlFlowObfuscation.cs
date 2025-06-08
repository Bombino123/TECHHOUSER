using System;
using System.Collections.Generic;
using System.Linq;
using dnlib.DotNet;
using dnlib.DotNet.Emit;
using Obfuscator.Helper;

namespace Obfuscator.Obfuscator.CtrlFlow
{
	// Token: 0x0200002B RID: 43
	internal class ControlFlowObfuscation
	{
		// Token: 0x060000B3 RID: 179 RVA: 0x00008DC0 File Offset: 0x00006FC0
		public static void Execute(ModuleDefMD md)
		{
			foreach (TypeDef typeDef in md.Types)
			{
				if (typeDef != md.GlobalType)
				{
					foreach (MethodDef methodDef in typeDef.Methods)
					{
						if (!methodDef.Name.StartsWith("get_") && !methodDef.Name.StartsWith("set_") && methodDef.HasBody && !methodDef.IsConstructor)
						{
							methodDef.Body.SimplifyBranches();
							ControlFlowObfuscation.ExecuteMethod(methodDef);
						}
					}
				}
			}
		}

		// Token: 0x060000B4 RID: 180 RVA: 0x00008E8C File Offset: 0x0000708C
		public static TypeSig RandomSig(MethodDef method)
		{
			switch (ControlFlowObfuscation.rnd.Next(0, 3))
			{
			case 0:
				return method.Module.CorLibTypes.Int32;
			case 1:
				return method.Module.CorLibTypes.Int64;
			case 2:
				return method.Module.CorLibTypes.Double;
			default:
				return method.Module.CorLibTypes.Int32;
			}
		}

		// Token: 0x060000B5 RID: 181 RVA: 0x00008F00 File Offset: 0x00007100
		public static void ExecuteMethod(MethodDef method)
		{
			foreach (ParamDef paramDef in method.ParamDefs)
			{
				paramDef.Name = Methods.GenerateString(16);
			}
			method.Body.SimplifyMacros(method.Parameters);
			List<Block> blocks = BlockParser.ParseMethod(method);
			int num = ControlFlowObfuscation.rnd.Next(1, 10);
			int num2 = 0;
			foreach (Block block3 in blocks)
			{
				block3.SubRand = num;
				num = ControlFlowObfuscation.rnd.Next(num + 1, num + 10);
				block3.PlusRand = num;
				if (blocks.Count - 2 == block3.Number)
				{
					num2 = num;
				}
			}
			blocks = ControlFlowObfuscation.Randomize(blocks);
			method.Body.Instructions.Clear();
			Local local = new Local(ControlFlowObfuscation.RandomSig(method));
			method.Body.Variables.Add(local);
			local.Name = Methods.GenerateString();
			Instruction instruction = Instruction.Create(OpCodes.Nop);
			Instruction instruction2 = Instruction.Create(OpCodes.Br, instruction);
			int num3 = ControlFlowObfuscation.rnd.Next(0, 10000);
			method.Body.Instructions.Add(ControlFlowObfuscation.Calc(num3, local.Type));
			method.Body.Instructions.Add(OpCodes.Stloc.ToInstruction(local));
			method.Body.Instructions.Add(Instruction.Create(OpCodes.Br, instruction2));
			method.Body.Instructions.Add(instruction);
			var lastBlock = blocks.Single(x => x.Number == blocks.Count - 1);
			var blocksToProcess = blocks.Where(block => block != lastBlock);
			
			foreach (Block block2 in blocksToProcess)
			{
				method.Body.Instructions.Add(OpCodes.Ldloc.ToInstruction(local));
				if (block2.Number == 0)
				{
					method.Body.Instructions.Add(ControlFlowObfuscation.Calc(num3, local.Type));
				}
				else
				{
					method.Body.Instructions.Add(ControlFlowObfuscation.Calc(block2.SubRand + num3, local.Type));
				}
				method.Body.Instructions.Add(Instruction.Create(OpCodes.Ceq));
				Instruction instruction3 = Instruction.Create(OpCodes.Nop);
				method.Body.Instructions.Add(Instruction.Create(OpCodes.Brfalse, instruction3));
				foreach (Instruction item in block2.Instructions)
				{
					method.Body.Instructions.Add(item);
				}
				method.Body.Instructions.Add(ControlFlowObfuscation.Calc(block2.PlusRand + num3, local.Type));
				method.Body.Instructions.Add(OpCodes.Stloc.ToInstruction(local));
				method.Body.Instructions.Add(instruction3);
			}
			method.Body.Instructions.Add(OpCodes.Ldloc.ToInstruction(local));
			method.Body.Instructions.Add(ControlFlowObfuscation.Calc(num2 + num3, local.Type));
			method.Body.Instructions.Add(Instruction.Create(OpCodes.Ceq));
			method.Body.Instructions.Add(Instruction.Create(OpCodes.Brfalse, instruction2));
			method.Body.Instructions.Add(Instruction.Create(OpCodes.Br, blocks.Single(x => x.Number == blocks.Count - 1).Instructions[0]));
			method.Body.Instructions.Add(instruction2);

			foreach (Instruction item2 in blocks.Single(x => x.Number == blocks.Count - 1).Instructions)
			{
				method.Body.Instructions.Add(item2);
			}
			method.Body.InitLocals = true;
		}

		// Token: 0x060000B6 RID: 182 RVA: 0x00009414 File Offset: 0x00007614
		public static List<Block> Randomize(List<Block> input)
		{
			List<Block> list = new List<Block>();
			foreach (Block item in input)
			{
				list.Insert(ControlFlowObfuscation.rnd.Next(0, list.Count), item);
			}
			return list;
		}

		// Token: 0x060000B7 RID: 183 RVA: 0x0000947C File Offset: 0x0000767C
		public static Instruction Calc(int value, TypeSig sig)
		{
			if (sig == sig.Module.CorLibTypes.Double)
			{
				return Instruction.Create(OpCodes.Ldc_R8, (double)value);
			}
			return Instruction.Create(OpCodes.Ldc_I4, value);
		}

		// Token: 0x0400001F RID: 31
		public static Random rnd = new Random();
	}
}
