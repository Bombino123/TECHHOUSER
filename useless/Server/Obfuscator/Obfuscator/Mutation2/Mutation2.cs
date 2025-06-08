using System;
using System.Collections.Generic;
using dnlib.DotNet;
using Obfuscator.Helper;

namespace Obfuscator.Obfuscator.Mutation2
{
	// Token: 0x0200001B RID: 27
	public class Mutation2
	{
		// Token: 0x06000066 RID: 102 RVA: 0x000055A0 File Offset: 0x000037A0
		public static void Execute(ModuleDef Module)
		{
			foreach (TypeDef typeDef in Module.Types)
			{
				foreach (iMutation iMutation in Mutation2.Tasks)
				{
					iMutation.Prepare(typeDef);
				}
				for (int i = 0; i < typeDef.Methods.Count; i++)
				{
					MethodDef methodDef = typeDef.Methods[i];
					if (methodDef.HasBody && !methodDef.IsConstructor)
					{
						methodDef.Body.SimplifyBranches();
						for (int j = 0; j < methodDef.Body.Instructions.Count; j++)
						{
							iMutation iMutation2 = Mutation2.Tasks[Methods.Random.Next(Mutation2.Tasks.Count)];
							if (iMutation2.Supported(methodDef.Body.Instructions[j]))
							{
								iMutation2.Process(methodDef, ref j);
							}
						}
					}
				}
			}
		}

		// Token: 0x04000013 RID: 19
		private static List<iMutation> Tasks = new List<iMutation>
		{
			new Abs(),
			new VariableMutation(),
			new ComparerMutation(),
			new MulToShift()
		};
	}
}
