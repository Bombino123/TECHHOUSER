using System;
using Microsoft.Win32;

namespace Client.Helper
{
	// Token: 0x02000013 RID: 19
	internal class SetRegistry
	{
		// Token: 0x0600006F RID: 111 RVA: 0x00005B7C File Offset: 0x00003D7C
		public static bool CheckValue(string name)
		{
			try
			{
				using (RegistryKey registryKey = Registry.CurrentUser.CreateSubKey(Config.RegKey, RegistryKeyPermissionCheck.ReadWriteSubTree))
				{
					if (registryKey.GetValue(name) != null)
					{
						return true;
					}
				}
			}
			catch
			{
			}
			return false;
		}

		// Token: 0x06000070 RID: 112 RVA: 0x00005BEC File Offset: 0x00003DEC
		public static void SetValue(string name, string value)
		{
			using (RegistryKey registryKey = Registry.CurrentUser.CreateSubKey(Config.RegKey, RegistryKeyPermissionCheck.ReadWriteSubTree))
			{
				if (SetRegistry.CheckValue(name))
				{
					registryKey.DeleteValue(name);
				}
				registryKey.SetValue(name, value);
			}
		}

		// Token: 0x06000071 RID: 113 RVA: 0x00005C48 File Offset: 0x00003E48
		public static string GetValue(string name)
		{
			if (!SetRegistry.CheckValue(name))
			{
				return null;
			}
			string result;
			using (RegistryKey registryKey = Registry.CurrentUser.CreateSubKey(Config.RegKey))
			{
				result = (string)registryKey.GetValue(name);
			}
			return result;
		}
	}
}
