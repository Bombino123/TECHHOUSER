using System;
using System.Collections.Generic;
using dnlib.DotNet;
using dnlib.DotNet.Emit;
using Obfuscator.Helper;

namespace Obfuscator.Obfuscator.Junk
{
	// Token: 0x02000021 RID: 33
	internal class Junks
	{
		// Token: 0x06000078 RID: 120 RVA: 0x00006214 File Offset: 0x00004414
		public static MethodDefUser MethodGenerator(ModuleDef module, CilBody instr, MethodAttributes attributes)
		{
			MethodImplAttributes implFlags = MethodImplAttributes.IL;
			return new MethodDefUser(Methods.GenerateString(), MethodSig.CreateStatic(module.CorLibTypes.Void), implFlags, attributes)
			{
				Body = instr
			};
		}

		// Token: 0x06000079 RID: 121 RVA: 0x0000624C File Offset: 0x0000444C
		public static MethodDefUser GetStringFunction(ModuleDef module, CilBody cilBody, MethodAttributes attributes)
		{
			MethodImplAttributes implFlags = MethodImplAttributes.IL;
			return new MethodDefUser(Methods.GenerateString(), MethodSig.CreateStatic(module.CorLibTypes.String), implFlags, attributes)
			{
				Body = cilBody
			};
		}

		// Token: 0x0600007A RID: 122 RVA: 0x00006284 File Offset: 0x00004484
		public static MethodDefUser GetIntFunction(ModuleDef module, CilBody instr, MethodAttributes attributes)
		{
			MethodImplAttributes implFlags = MethodImplAttributes.IL;
			return new MethodDefUser(Methods.GenerateString(), MethodSig.CreateStatic(module.CorLibTypes.Int32), implFlags, attributes)
			{
				Body = instr
			};
		}

		// Token: 0x0600007B RID: 123 RVA: 0x000062BC File Offset: 0x000044BC
		public static MethodDefUser GetLongFunction(ModuleDef module, CilBody instr, MethodAttributes attributes)
		{
			MethodImplAttributes implFlags = MethodImplAttributes.IL;
			return new MethodDefUser(Methods.GenerateString(), MethodSig.CreateStatic(module.CorLibTypes.Int64), implFlags, attributes)
			{
				Body = instr
			};
		}

		// Token: 0x0600007C RID: 124 RVA: 0x000062F4 File Offset: 0x000044F4
		public static FieldDef CreateString(ModuleDef module, FieldAttributes attributes)
		{
			FieldSig signature = new FieldSig(module.CorLibTypes.String);
			return new FieldDefUser(Methods.GenerateString(), signature, attributes);
		}

		// Token: 0x0600007D RID: 125 RVA: 0x00006324 File Offset: 0x00004524
		public static FieldDef CreateInt32(ModuleDef module, FieldAttributes attributes)
		{
			FieldSig signature = new FieldSig(module.CorLibTypes.Int32);
			return new FieldDefUser(Methods.GenerateString(), signature, attributes);
		}

		// Token: 0x0600007E RID: 126 RVA: 0x00006354 File Offset: 0x00004554
		public static FieldDef CreateInt64(ModuleDef module, FieldAttributes attributes)
		{
			FieldSig signature = new FieldSig(module.CorLibTypes.Int64);
			return new FieldDefUser(Methods.GenerateString(), signature, attributes);
		}

		// Token: 0x0600007F RID: 127 RVA: 0x00006384 File Offset: 0x00004584
		public static FieldDef CreateBool(ModuleDef module, FieldAttributes attributes)
		{
			FieldSig signature = new FieldSig(module.CorLibTypes.Boolean);
			return new FieldDefUser(Methods.GenerateString(), signature, attributes);
		}

		// Token: 0x06000080 RID: 128 RVA: 0x000063B3 File Offset: 0x000045B3
		public static TypeDef CreateNewClass()
		{
			return new TypeDefUser(Methods.GenerateString())
			{
				Attributes = (TypeAttributes.Abstract | TypeAttributes.BeforeFieldInit),
				Namespace = Methods.GenerateString()
			};
		}

		// Token: 0x06000081 RID: 129 RVA: 0x000063DF File Offset: 0x000045DF
		public static MethodDef cctor(ModuleDef module)
		{
			return new MethodDefUser(".cctor", MethodSig.CreateInstance(module.CorLibTypes.Void), MethodImplAttributes.IL, MethodAttributes.Private | MethodAttributes.Static | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName);
		}

		// Token: 0x06000082 RID: 130 RVA: 0x00006406 File Offset: 0x00004606
		public static MethodDef ctor(ModuleDef module)
		{
			return new MethodDefUser(".ctor", MethodSig.CreateInstance(module.CorLibTypes.Void), MethodImplAttributes.IL, MethodAttributes.FamANDAssem | MethodAttributes.Family | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName);
		}

		// Token: 0x06000083 RID: 131 RVA: 0x00006430 File Offset: 0x00004630
		public static FieldAttributes RandomFieldAttributes(bool Static)
		{
			FieldAttributes fieldAttributes = FieldAttributes.Private;
			switch (Junks.random.Next(0, 3))
			{
			case 0:
				fieldAttributes = FieldAttributes.Public;
				break;
			case 1:
				fieldAttributes = FieldAttributes.Private;
				break;
			case 2:
				fieldAttributes = FieldAttributes.Literal;
				break;
			}
			if (Static)
			{
				fieldAttributes |= FieldAttributes.Static;
			}
			return fieldAttributes;
		}

		// Token: 0x06000084 RID: 132 RVA: 0x00006474 File Offset: 0x00004674
		public static MethodAttributes RandomMethodAttributes(bool Static)
		{
			MethodAttributes methodAttributes = MethodAttributes.Private;
			switch (Junks.random.Next(0, 3))
			{
			case 0:
				methodAttributes = MethodAttributes.Public;
				break;
			case 1:
				methodAttributes = MethodAttributes.Private;
				break;
			case 2:
				methodAttributes = MethodAttributes.Virtual;
				break;
			}
			if (Static)
			{
				methodAttributes |= MethodAttributes.Static;
			}
			return methodAttributes | MethodAttributes.HideBySig | MethodAttributes.PrivateScope;
		}

		// Token: 0x06000085 RID: 133 RVA: 0x000064C0 File Offset: 0x000046C0
		public static CilBody GeneratorBody(ModuleDef module, string TypeMethod)
		{
			CilBody cilBody = new CilBody();
			if (TypeMethod == "String")
			{
				cilBody.Instructions.Add(Instruction.Create(OpCodes.Nop));
				cilBody.Variables.Add(new Local(module.CorLibTypes.String, Methods.GenerateString()));
				cilBody.Instructions.Add(Instruction.Create(OpCodes.Ldstr, Methods.GenerateString()));
				cilBody.Instructions.Add(Instruction.Create(OpCodes.Stloc_0));
				cilBody.Instructions.Add(Instruction.Create(OpCodes.Ldloc_0));
			}
			if (TypeMethod == "Int32")
			{
				cilBody.Instructions.Add(Instruction.Create(OpCodes.Nop));
				cilBody.Variables.Add(new Local(module.CorLibTypes.Int32, Methods.GenerateString()));
				cilBody.Instructions.Add(Instruction.Create(OpCodes.Ldc_I4, Junks.random.Next(int.MinValue, int.MaxValue)));
				cilBody.Instructions.Add(Instruction.Create(OpCodes.Stloc_0));
				cilBody.Instructions.Add(Instruction.Create(OpCodes.Ldloc_0));
			}
			if (TypeMethod == "Int64")
			{
				cilBody.Instructions.Add(Instruction.Create(OpCodes.Nop));
				cilBody.Variables.Add(new Local(module.CorLibTypes.Int64, Methods.GenerateString()));
				cilBody.Instructions.Add(Instruction.Create(OpCodes.Ldc_I4, Junks.random.Next(int.MinValue, int.MaxValue)));
				cilBody.Instructions.Add(Instruction.Create(OpCodes.Stloc_0));
				cilBody.Instructions.Add(Instruction.Create(OpCodes.Ldloc_0));
			}
			if (TypeMethod != "Void")
			{
				cilBody.Instructions.Add(Instruction.Create(OpCodes.Ret));
			}
			return cilBody;
		}

		// Token: 0x06000086 RID: 134 RVA: 0x000066B0 File Offset: 0x000048B0
		public static void Execute(ModuleDef module)
		{
			int num = Junks.random.Next(20, 100);
			for (int i = 0; i < num; i++)
			{
				TypeDef typeDef = Junks.CreateNewClass();
				bool flag = false;
				MethodDef methodDef;
				if (Junks.random.Next(0, 3) == 1)
				{
					flag = true;
					methodDef = Junks.cctor(module);
				}
				else
				{
					methodDef = Junks.ctor(module);
				}
				if (Junks.random.Next(0, 8) == 1)
				{
					typeDef.Namespace = Methods.GenerateString();
				}
				for (int j = 0; j < 4; j++)
				{
					for (int k = 0; k < Junks.random.Next(0, 2); k++)
					{
						typeDef.Fields.Add(Junks.CreateInt32(module, Junks.RandomFieldAttributes(flag)));
					}
					for (int l = 0; l < Junks.random.Next(0, 2); l++)
					{
						typeDef.Methods.Add(Junks.GetIntFunction(module, Junks.GeneratorBody(module, "Int32"), Junks.RandomMethodAttributes(flag)));
					}
					for (int m = 0; m < Junks.random.Next(0, 2); m++)
					{
						typeDef.Fields.Add(Junks.CreateBool(module, Junks.RandomFieldAttributes(flag)));
					}
					for (int n = 0; n < Junks.random.Next(0, 2); n++)
					{
						typeDef.Methods.Add(Junks.GetStringFunction(module, Junks.GeneratorBody(module, "String"), Junks.RandomMethodAttributes(flag)));
					}
					for (int num2 = 0; num2 < Junks.random.Next(0, 2); num2++)
					{
						typeDef.Fields.Add(Junks.CreateString(module, Junks.RandomFieldAttributes(flag)));
					}
					for (int num3 = 0; num3 < Junks.random.Next(0, 2); num3++)
					{
						typeDef.Fields.Add(Junks.CreateInt64(module, Junks.RandomFieldAttributes(flag)));
					}
					for (int num4 = 0; num4 < Junks.random.Next(0, 2); num4++)
					{
						MethodDef item = Junks.MethodGenerator(module, Junks.GeneratorBody(module, "Void"), Junks.RandomMethodAttributes(flag));
						if (flag)
						{
							Junks.AllVoid.Add(item);
						}
						typeDef.Methods.Add(item);
					}
				}
				typeDef.Methods.Add(methodDef);
				methodDef.Body = new CilBody();
				if (!flag)
				{
					MemberRefUser mr = new MemberRefUser(module, ".ctor", MethodSig.CreateInstance(module.CorLibTypes.Void), module.CorLibTypes.Object.TypeDefOrRef);
					methodDef.Body.Instructions.Add(OpCodes.Ldarg_0.ToInstruction());
					methodDef.Body.Instructions.Add(OpCodes.Call.ToInstruction(mr));
				}
				foreach (FieldDef fieldDef in typeDef.Fields)
				{
					if (fieldDef.FieldType.TypeName == "Int64")
					{
						methodDef.Body.Instructions.Add(Instruction.Create(OpCodes.Ldc_I8, (long)Junks.random.Next(int.MinValue, int.MaxValue)));
						methodDef.Body.Instructions.Add(Instruction.Create(OpCodes.Stsfld, fieldDef));
					}
					if (fieldDef.FieldType.TypeName == "Boolean")
					{
						if (Junks.random.Next(0, 2) == 1)
						{
							methodDef.Body.Instructions.Add(Instruction.Create(OpCodes.Ldc_I4_1));
						}
						else
						{
							methodDef.Body.Instructions.Add(Instruction.Create(OpCodes.Ldc_I4_0));
						}
						methodDef.Body.Instructions.Add(Instruction.Create(OpCodes.Stsfld, fieldDef));
					}
					if (fieldDef.FieldType.TypeName == "String")
					{
						methodDef.Body.Instructions.Add(Instruction.Create(OpCodes.Ldstr, Methods.GenerateString()));
						methodDef.Body.Instructions.Add(Instruction.Create(OpCodes.Stsfld, fieldDef));
					}
					if (fieldDef.FieldType.TypeName == "Int32")
					{
						methodDef.Body.Instructions.Add(Instruction.Create(OpCodes.Ldc_I4, Junks.random.Next(int.MinValue, int.MaxValue)));
						methodDef.Body.Instructions.Add(Instruction.Create(OpCodes.Stsfld, fieldDef));
					}
				}
				methodDef.Body.Instructions.Add(OpCodes.Ret.ToInstruction());
				module.Types.Add(typeDef);
			}
			for (int num5 = 0; num5 < Junks.random.Next(Junks.AllVoid.Count); num5++)
			{
				MethodDef methodDef2 = Junks.AllVoid[Junks.random.Next(Junks.AllVoid.Count)];
				MethodDef method = Junks.AllVoid[Junks.random.Next(Junks.AllVoid.Count)];
				methodDef2.Body.Instructions.Add(Instruction.Create(OpCodes.Call, method));
				methodDef2.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));
			}
			Junks.AllVoid.Clear();
		}

		// Token: 0x04000017 RID: 23
		public static Random random = new Random();

		// Token: 0x04000018 RID: 24
		public static List<MethodDef> AllVoid = new List<MethodDef>();
	}
}
