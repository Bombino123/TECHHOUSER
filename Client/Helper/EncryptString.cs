using System;

namespace Client.Helper
{
	// Token: 0x0200000A RID: 10
	internal class EncryptString
	{
		// Token: 0x06000031 RID: 49 RVA: 0x00003B0C File Offset: 0x00001D0C
		public static string Decode(string text)
		{
			string text2 = "";
			foreach (char c in text)
			{
				for (int j = 0; j < EncryptString.dec.Length; j++)
				{
					if (EncryptString.dec[j] == c)
					{
						text2 += EncryptString.enc[j].ToString();
						break;
					}
				}
			}
			return text2;
		}

		// Token: 0x0400002D RID: 45
		public static string enc = "%enc%";

		// Token: 0x0400002E RID: 46
		public static string dec = "%dec%";
	}
}
