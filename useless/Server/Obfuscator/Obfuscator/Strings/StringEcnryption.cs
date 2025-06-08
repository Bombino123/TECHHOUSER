using System;
using System.Linq;
using System.Text;
using dnlib.DotNet;
using dnlib.DotNet.Emit;
using Obfuscator.Helper;
using Obfuscator.Obfuscator.Strings.Runtime;

namespace Obfuscator.Obfuscator.Strings
{
	// Token: 0x02000004 RID: 4
	internal class StringEcnryption
	{
		// Token: 0x06000012 RID: 18 RVA: 0x00002AE8 File Offset: 0x00000CE8
		private static MethodDef InjectMethod(ModuleDef module, string methodName)
		{
			MethodDef result = (MethodDef)InjectHelper.Inject(ModuleDefMD.Load(typeof(DecryptionHelper).Module).ResolveTypeDef(MDToken.ToRID(typeof(DecryptionHelper).MetadataToken)), module.GlobalType, module).Single((IDnlibDef method) => method.Name == methodName);
			foreach (MethodDef methodDef in module.GlobalType.Methods)
			{
				if (methodDef.Name == ".ctor")
				{
					module.GlobalType.Remove(methodDef);
					break;
				}
			}
			return result;
		}

		// Token: 0x06000013 RID: 19 RVA: 0x00002BB4 File Offset: 0x00000DB4
		private static string Encrypt(string dataPlain)
		{
			string result;
			try
			{
				result = Convert.ToBase64String(Encoding.UTF8.GetBytes(dataPlain));
			}
			catch (Exception)
			{
				result = null;
			}
			return result;
		}

		// Token: 0x06000014 RID: 20 RVA: 0x00002BEC File Offset: 0x00000DEC
		public static void Execute(ModuleDef module)
		{
			MethodDef methodDef = StringEcnryption.InjectMethod(module, "Decrypt_Base64");
			foreach (TypeDef typeDef in module.Types)
			{
				if (!typeDef.IsGlobalModuleType && !(typeDef.Name == "Resources") && !(typeDef.Name == "Settings"))
				{
					foreach (MethodDef methodDef2 in typeDef.Methods)
					{
						if (methodDef2.HasBody && methodDef2 != methodDef)
						{
							methodDef2.Body.KeepOldMaxStack = true;
							for (int i = 0; i < methodDef2.Body.Instructions.Count; i++)
							{
								if (methodDef2.Body.Instructions[i].OpCode == OpCodes.Ldstr)
								{
									string dataPlain = methodDef2.Body.Instructions[i].Operand.ToString();
									methodDef2.Body.Instructions[i].Operand = StringEcnryption.Encrypt(dataPlain);
									methodDef2.Body.Instructions.Insert(i + 1, new Instruction(OpCodes.Call, methodDef));
								}
							}
							methodDef2.Body.SimplifyBranches();
							methodDef2.Body.OptimizeBranches();
						}
					}
				}
			}
		}
	}
}
