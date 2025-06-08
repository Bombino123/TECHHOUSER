using System;

namespace Server.Helper.Bulider
{
	// Token: 0x02000090 RID: 144
	internal class EncryptString
	{
		// Token: 0x060003D2 RID: 978 RVA: 0x00029CC0 File Offset: 0x00027EC0
		public string Encrypt(string text)
		{
			string text2 = "";
			foreach (char c in text)
			{
				for (int j = 0; j < this.enc.Length; j++)
				{
					if (this.enc[j] == c)
					{
						text2 += this.dec[j].ToString();
						break;
					}
				}
			}
			return text2;
		}

		// Token: 0x0400020C RID: 524
		public string enc = "%enc%";

		// Token: 0x0400020D RID: 525
		public string dec = "%dec%";
	}
}
