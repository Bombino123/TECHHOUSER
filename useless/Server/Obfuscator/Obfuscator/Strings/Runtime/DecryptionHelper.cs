using System;
using System.Text;

namespace Obfuscator.Obfuscator.Strings.Runtime
{
	// Token: 0x02000005 RID: 5
	internal static class DecryptionHelper
	{
		// Token: 0x06000016 RID: 22 RVA: 0x00002DB4 File Offset: 0x00000FB4
		public static string Decrypt_Base64(string dataEnc)
		{
			return Encoding.UTF8.GetString(Convert.FromBase64String(dataEnc));
		}
	}
}
