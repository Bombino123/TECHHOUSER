using System;
using System.Linq;

namespace Obfuscator.Helper
{
	// Token: 0x02000003 RID: 3
	internal class Methods
	{
		// Token: 0x0600000C RID: 12 RVA: 0x00002A2D File Offset: 0x00000C2D
		public static string GenerateString()
		{
			return Methods.GenerateString((byte)Methods.Random.Next(10, 26));
		}

		// Token: 0x0600000D RID: 13 RVA: 0x00002A44 File Offset: 0x00000C44
		public static string GenerateString(byte length)
		{
			char c = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz"[Methods.Random.Next("ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz".Length)];
			char[] value = (from s in Enumerable.Repeat<string>("ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz", (int)(length - 1))
			select s[Methods.Random.Next(s.Length)]).ToArray<char>();
			return c.ToString() + new string(value);
		}

		// Token: 0x0600000E RID: 14 RVA: 0x00002AB9 File Offset: 0x00000CB9
		public static bool GenerateBool()
		{
			return Methods.GenerateBool(2);
		}

		// Token: 0x0600000F RID: 15 RVA: 0x00002AC1 File Offset: 0x00000CC1
		public static bool GenerateBool(byte probability)
		{
			return Methods.Random.Next((int)probability) == 1;
		}

		// Token: 0x04000001 RID: 1
		private const string Ascii = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";

		// Token: 0x04000002 RID: 2
		private const string number = "1234567890";

		// Token: 0x04000003 RID: 3
		public static readonly Random Random = new Random();
	}
}
