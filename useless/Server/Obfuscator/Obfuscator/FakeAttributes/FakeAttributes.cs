using System;
using System.Collections.Generic;
using dnlib.DotNet;

namespace Obfuscator.Obfuscator.FakeAttributes
{
	// Token: 0x02000028 RID: 40
	internal class FakeAttributes
	{
		// Token: 0x060000A6 RID: 166 RVA: 0x00008AB4 File Offset: 0x00006CB4
		public static void Execute(ModuleDef module)
		{
			foreach (string s in new List<string>
			{
				"ZYXDNGuarder",
				"HVMRuntm.dll"
			})
			{
				TypeDef typeDef = new TypeDefUser("Attributes", s, module.Import(typeof(Attribute)));
				typeDef.Attributes = TypeAttributes.NotPublic;
				module.Types.Add(typeDef);
			}
			TypeDef typeDef2 = module.Types[new Random().Next(0, module.Types.Count)];
		}
	}
}
