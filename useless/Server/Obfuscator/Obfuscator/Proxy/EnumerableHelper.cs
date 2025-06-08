using System;
using System.Collections.Generic;
using System.Linq;

namespace Obfuscator.Obfuscator.Proxy
{
	// Token: 0x0200000A RID: 10
	public static class EnumerableHelper
	{
		// Token: 0x06000027 RID: 39 RVA: 0x0000408C File Offset: 0x0000228C
		public static TE Random<TE>(IEnumerable<TE> input)
		{
			TE[] array = (input as TE[]) ?? input.ToArray<TE>();
			return array.ElementAt(EnumerableHelper.R.Next(array.Length));
		}

		// Token: 0x04000010 RID: 16
		private static readonly Random R = new Random();
	}
}
