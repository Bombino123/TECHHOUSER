using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using dnlib.DotNet;
using dnlib.DotNet.Emit;

namespace Obfuscator.Obfuscator.Mutation2
{
	// Token: 0x02000015 RID: 21
	public class Func : iMutation
	{
		// Token: 0x17000001 RID: 1
		// (get) Token: 0x06000048 RID: 72 RVA: 0x00004A11 File Offset: 0x00002C11
		// (set) Token: 0x06000049 RID: 73 RVA: 0x00004A19 File Offset: 0x00002C19
		public FieldDef Decryptor { get; set; }

		// Token: 0x0600004A RID: 74 RVA: 0x00004A24 File Offset: 0x00002C24
		public void Process(MethodDef method, ref int index)
		{
			int ldcI4Value = method.Body.Instructions[index].GetLdcI4Value();
			method.Body.Instructions[index].OpCode = OpCodes.Ldsfld;
			method.Body.Instructions[index].Operand = this.Decryptor;
			IList<Instruction> instructions = method.Body.Instructions;
			int num = index + 1;
			index = num;
			instructions.Insert(num, new Instruction(OpCodes.Ldc_I4, ldcI4Value));
			IList<Instruction> instructions2 = method.Body.Instructions;
			num = index + 1;
			index = num;
			instructions2.Insert(num, new Instruction(OpCodes.Callvirt, method.Module.Import(typeof(Func<int, int>).GetMethod("Invoke"))));
			index -= 2;
		}

		// Token: 0x0600004B RID: 75 RVA: 0x00004AF4 File Offset: 0x00002CF4
		public FieldDef CreateProperField(TypeDef type)
		{
			TypeDef typeDef = ModuleDefMD.Load(typeof(FuncMutation).Module).ResolveTypeDef(MDToken.ToRID(typeof(FuncMutation).MetadataToken));
			FieldDef fieldDef = typeDef.Fields.FirstOrDefault((FieldDef x) => x.Name == "prao");
			fieldDef.DeclaringType = null;
			type.Fields.Add(fieldDef);
			MethodDef methodDef = typeDef.FindMethod("RET");
			methodDef.DeclaringType = null;
			type.Methods.Add(methodDef);
			MethodDef methodDef2 = type.FindOrCreateStaticConstructor();
			methodDef2.Body.Instructions.Insert(0, new Instruction(OpCodes.Ldnull));
			methodDef2.Body.Instructions.Insert(1, new Instruction(OpCodes.Ldftn, methodDef));
			methodDef2.Body.Instructions.Insert(2, new Instruction(OpCodes.Newobj, type.Module.Import(typeof(Func<int, int>).GetConstructors().First<ConstructorInfo>())));
			methodDef2.Body.Instructions.Insert(3, new Instruction(OpCodes.Stsfld, fieldDef));
			methodDef2.Body.Instructions.Insert(4, new Instruction(OpCodes.Nop));
			return fieldDef;
		}

		// Token: 0x0600004C RID: 76 RVA: 0x00004C3D File Offset: 0x00002E3D
		public void Prepare(TypeDef type)
		{
			this.Decryptor = this.CreateProperField(type);
		}

		// Token: 0x0600004D RID: 77 RVA: 0x00004C4C File Offset: 0x00002E4C
		public bool Supported(Instruction instr)
		{
			return Utils.CheckArithmetic(instr);
		}
	}
}
