using System;
using System.Linq;
using dnlib.DotNet;
using dnlib.DotNet.Emit;
using Obfuscator.Helper;
using Obfuscator.Obfuscator.Anti.Runtime;

namespace Obfuscator.Obfuscator.Anti
{
	// Token: 0x02000030 RID: 48
	internal class AntiDump
	{
		// Token: 0x060000C6 RID: 198 RVA: 0x00009F70 File Offset: 0x00008170
		public static void Execute(ModuleDef mod)
		{
			ModuleDefMD moduleDefMD = ModuleDefMD.Load(typeof(AntiDumpRun).Module);
			MethodDef methodDef = mod.GlobalType.FindOrCreateStaticConstructor();
			MethodDef method2 = (MethodDef)InjectHelper.Inject(moduleDefMD.ResolveTypeDef(MDToken.ToRID(typeof(AntiDumpRun).MetadataToken)), mod.GlobalType, mod).Single((IDnlibDef method) => method.Name == "Initialize");
			methodDef.Body.Instructions.Insert(0, Instruction.Create(OpCodes.Call, method2));
			foreach (MethodDef methodDef2 in mod.GlobalType.Methods)
			{
				if (!(methodDef2.Name != ".ctor"))
				{
					mod.GlobalType.Remove(methodDef2);
					break;
				}
			}
		}
	}
}
