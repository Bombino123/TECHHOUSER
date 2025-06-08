using System;
using dnlib.DotNet;

namespace Obfuscator.Obfuscator.Anti
{
	// Token: 0x02000032 RID: 50
	internal class AntiDe4dot
	{
		// Token: 0x060000CA RID: 202 RVA: 0x0000A1B8 File Offset: 0x000083B8
		public static void Execute(AssemblyDef mod)
		{
			foreach (ModuleDef moduleDef in mod.Modules)
			{
				InterfaceImplUser item = new InterfaceImplUser(moduleDef.GlobalType);
				for (int i = 0; i < 1; i++)
				{
					TypeDefUser typeDefUser = new TypeDefUser(string.Empty, string.Format("Form{0}", i), moduleDef.CorLibTypes.GetTypeRef("System", "Attribute"));
					InterfaceImplUser item2 = new InterfaceImplUser(typeDefUser);
					moduleDef.Types.Add(typeDefUser);
					typeDefUser.Interfaces.Add(item2);
					typeDefUser.Interfaces.Add(item);
				}
			}
		}
	}
}
