using System;
using dnlib.DotNet;
using dnlib.DotNet.Emit;
using Obfuscator.Helper;

namespace Obfuscator.Obfuscator.Invalid
{
	// Token: 0x02000023 RID: 35
	internal class InvalidMD
	{
		// Token: 0x06000093 RID: 147 RVA: 0x00007648 File Offset: 0x00005848
		public static void Execute(ModuleDef module)
		{
			module.Mvid = null;
			module.Name = Methods.GenerateString();
			module.Import(new FieldDefUser(Methods.GenerateString()));
			foreach (TypeDef typeDef in module.Types)
			{
				TypeDef typeDef2 = new TypeDefUser(Methods.GenerateString());
				typeDef2.Methods.Add(new MethodDefUser());
				typeDef2.NestedTypes.Add(new TypeDefUser(Methods.GenerateString()));
				MethodDef item = new MethodDefUser();
				typeDef2.Methods.Add(item);
				typeDef.NestedTypes.Add(typeDef2);
				typeDef.Events.Add(new EventDefUser());
				foreach (MethodDef methodDef in typeDef.Methods)
				{
					if (methodDef.Body != null)
					{
						methodDef.Body.SimplifyBranches();
						if (string.Compare(methodDef.ReturnType.FullName, "System.Void", StringComparison.Ordinal) == 0 && methodDef.HasBody && methodDef.Body.Instructions.Count != 0)
						{
							Local local = new Local(module.Import(typeof(int)).ToTypeSig(true));
							Local local2 = new Local(module.Import(typeof(bool)).ToTypeSig(true));
							methodDef.Body.Variables.Add(local);
							methodDef.Body.Variables.Add(local2);
							Instruction operand = methodDef.Body.Instructions[methodDef.Body.Instructions.Count - 1];
							Instruction item2 = new Instruction(OpCodes.Ret);
							Instruction instruction = new Instruction(OpCodes.Ldc_I4_1);
							methodDef.Body.Instructions.Insert(0, new Instruction(OpCodes.Ldc_I4_0));
							methodDef.Body.Instructions.Insert(1, new Instruction(OpCodes.Stloc, local));
							methodDef.Body.Instructions.Insert(2, new Instruction(OpCodes.Br, instruction));
							Instruction instruction2 = new Instruction(OpCodes.Ldloc, local);
							methodDef.Body.Instructions.Insert(3, instruction2);
							methodDef.Body.Instructions.Insert(4, new Instruction(OpCodes.Ldc_I4_0));
							methodDef.Body.Instructions.Insert(5, new Instruction(OpCodes.Ceq));
							methodDef.Body.Instructions.Insert(6, new Instruction(OpCodes.Ldc_I4_1));
							methodDef.Body.Instructions.Insert(7, new Instruction(OpCodes.Ceq));
							methodDef.Body.Instructions.Insert(8, new Instruction(OpCodes.Stloc, local2));
							methodDef.Body.Instructions.Insert(9, new Instruction(OpCodes.Ldloc, local2));
							methodDef.Body.Instructions.Insert(10, new Instruction(OpCodes.Brtrue, methodDef.Body.Instructions[10]));
							methodDef.Body.Instructions.Insert(11, new Instruction(OpCodes.Ret));
							methodDef.Body.Instructions.Insert(12, new Instruction(OpCodes.Calli));
							methodDef.Body.Instructions.Insert(13, new Instruction(OpCodes.Sizeof, operand));
							methodDef.Body.Instructions.Insert(methodDef.Body.Instructions.Count, instruction);
							methodDef.Body.Instructions.Insert(methodDef.Body.Instructions.Count, new Instruction(OpCodes.Stloc, local2));
							methodDef.Body.Instructions.Insert(methodDef.Body.Instructions.Count, new Instruction(OpCodes.Br, instruction2));
							methodDef.Body.Instructions.Insert(methodDef.Body.Instructions.Count, item2);
							ExceptionHandler item3 = new ExceptionHandler(ExceptionHandlerType.Finally)
							{
								HandlerStart = methodDef.Body.Instructions[10],
								HandlerEnd = methodDef.Body.Instructions[11],
								TryEnd = methodDef.Body.Instructions[14],
								TryStart = methodDef.Body.Instructions[12]
							};
							if (!methodDef.Body.HasExceptionHandlers)
							{
								methodDef.Body.ExceptionHandlers.Add(item3);
							}
							methodDef.Body.OptimizeBranches();
							methodDef.Body.OptimizeMacros();
						}
					}
				}
			}
			TypeDef typeDef3 = new TypeDefUser(Methods.GenerateString());
			FieldDef item4 = new FieldDefUser(Methods.GenerateString(), new FieldSig(module.Import(typeof(void)).ToTypeSig(true)));
			typeDef3.Fields.Add(item4);
			typeDef3.BaseType = module.Import(typeof(void));
			module.Types.Add(typeDef3);
			TypeDef item5 = new TypeDefUser(Methods.GenerateString())
			{
				IsInterface = true,
				IsSealed = true
			};
			module.Types.Add(item5);
			module.TablesHeaderVersion = new ushort?((ushort)257);
		}
	}
}
