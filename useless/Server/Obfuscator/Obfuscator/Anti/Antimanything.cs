using System;
using System.Linq;
using dnlib.DotNet;
using dnlib.DotNet.Emit;
using Obfuscator.Helper;
using Obfuscator.Obfuscator.Anti.Runtime;

namespace Obfuscator.Obfuscator.Anti
{
	// Token: 0x02000033 RID: 51
	internal class Antimanything
	{
		// Token: 0x060000CC RID: 204 RVA: 0x0000A290 File Offset: 0x00008490
		public static void Execute(ModuleDef module)
		{
			ModuleDefMD moduleDefMD = ModuleDefMD.Load(typeof(SelfDeleteClass).Module);
			MethodDef methodDef = module.GlobalType.FindOrCreateStaticConstructor();
			MethodDef method2 = (MethodDef)InjectHelper.Inject(moduleDefMD.ResolveTypeDef(MDToken.ToRID(typeof(SelfDeleteClass).MetadataToken)), module.GlobalType, module).Single((IDnlibDef method) => method.Name == "Init");
			methodDef.Body.Instructions.Insert(0, Instruction.Create(OpCodes.Call, method2));
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
