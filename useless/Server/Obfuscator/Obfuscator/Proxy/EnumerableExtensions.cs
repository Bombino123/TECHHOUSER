using System;
using System.Collections.Generic;

namespace Obfuscator.Obfuscator.Proxy
{
	// Token: 0x0200000B RID: 11
	public static class EnumerableExtensions
	{
		// Token: 0x06000028 RID: 40 RVA: 0x000040BD File Offset: 0x000022BD
		public static T Random<T>(this IEnumerable<T> input)
		{
			return EnumerableHelper.Random<T>(input);
		}
	}
}
