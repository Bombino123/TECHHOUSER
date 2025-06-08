using System;
using System.Linq;
using dnlib.DotNet;
using dnlib.DotNet.Emit;
using Obfuscator.Helper;
using Obfuscator.Obfuscator.Anti.Runtime;

namespace Obfuscator.Obfuscator.Anti
{
	// Token: 0x0200002F RID: 47
	public static class AntiDebug
	{
		// Token: 0x060000C5 RID: 197 RVA: 0x00009E78 File Offset: 0x00008078
		public static void Execute(ModuleDef module)
		{
			ModuleDefMD moduleDefMD = ModuleDefMD.Load(typeof(AntiDebugSafe).Module);
			MethodDef methodDef = module.GlobalType.FindOrCreateStaticConstructor();
			MethodDef method2 = (MethodDef)InjectHelper.Inject(moduleDefMD.ResolveTypeDef(MDToken.ToRID(typeof(AntiDebugSafe).MetadataToken)), module.GlobalType, module).Single((IDnlibDef method) => method.Name == "Initialize");
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
