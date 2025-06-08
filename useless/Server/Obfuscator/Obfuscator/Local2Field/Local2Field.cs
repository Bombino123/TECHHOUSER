using System;
using System.Collections.Generic;
using System.Linq;
using dnlib.DotNet;
using dnlib.DotNet.Emit;
using Obfuscator.Helper;

namespace Obfuscator.Obfuscator.Local2Field
{
	// Token: 0x02000020 RID: 32
	internal class Local2Field
	{
		// Token: 0x06000074 RID: 116 RVA: 0x00005F74 File Offset: 0x00004174
		public static void Execute(ModuleDef moduleDef)
		{
			var nonGlobalTypes = moduleDef.Types.Where(x => x != moduleDef.GlobalType);
			foreach (TypeDef typeDef in nonGlobalTypes)
			{
				foreach (MethodDef meth in from x in typeDef.Methods
				where x.HasBody && x.Body.HasInstructions && !x.IsConstructor
				select x)
				{
					Local2Field._convertedLocals = new Dictionary<Local, FieldDef>();
					Local2Field.Process(moduleDef, meth);
				}
			}
		}

		// Token: 0x06000075 RID: 117 RVA: 0x00006060 File Offset: 0x00004260
		private static void Process(ModuleDef module, MethodDef meth)
		{
			meth.Body.SimplifyMacros(meth.Parameters);
			foreach (Instruction instruction in meth.Body.Instructions)
			{
				Local local = instruction.Operand as Local;
				if (local != null)
				{
					FieldDef fieldDef;
					if (!Local2Field._convertedLocals.ContainsKey(local))
					{
						fieldDef = new FieldDefUser(Methods.GenerateString(), new FieldSig(local.Type), FieldAttributes.FamANDAssem | FieldAttributes.Family | FieldAttributes.Static);
						module.GlobalType.Fields.Add(fieldDef);
						Local2Field._convertedLocals.Add(local, fieldDef);
					}
					else
					{
						fieldDef = Local2Field._convertedLocals[local];
					}
					OpCode opCode = instruction.OpCode;
					Code? code = (opCode != null) ? new Code?(opCode.Code) : null;
					if (code == null)
					{
						goto IL_12B;
					}
					switch (code.GetValueOrDefault())
					{
					case Code.Ldloc:
						instruction.OpCode = OpCodes.Ldsfld;
						break;
					case Code.Ldloca:
						instruction.OpCode = OpCodes.Ldsflda;
						break;
					case Code.Stloc:
						instruction.OpCode = OpCodes.Stsfld;
						break;
					default:
						goto IL_12B;
					}
					IL_132:
					instruction.Operand = fieldDef;
					continue;
					IL_12B:
					instruction.OpCode = null;
					goto IL_132;
				}
			}
			Local2Field._convertedLocals.ToList<KeyValuePair<Local, FieldDef>>().ForEach(delegate(KeyValuePair<Local, FieldDef> x)
			{
				meth.Body.Variables.Remove(x.Key);
			});
			Local2Field._convertedLocals = new Dictionary<Local, FieldDef>();
		}

		// Token: 0x04000016 RID: 22
		private static Dictionary<Local, FieldDef> _convertedLocals = new Dictionary<Local, FieldDef>();
	}
}
