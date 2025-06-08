using System;
using dnlib.DotNet.Emit;

namespace Obfuscator.Obfuscator.Mutation2
{
	// Token: 0x0200000E RID: 14
	public class Utils
	{
		// Token: 0x0600002E RID: 46 RVA: 0x000044CF File Offset: 0x000026CF
		public static bool CheckArithmetic(Instruction instruction)
		{
			return instruction.IsLdcI4() && instruction.GetLdcI4Value() > 1;
		}
	}
}
