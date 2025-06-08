using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Win32;

namespace Server.Helper
{
	// Token: 0x02000079 RID: 121
	public static class RegistryKeyHelper
	{
		// Token: 0x06000299 RID: 665 RVA: 0x00024224 File Offset: 0x00022424
		public static bool AddRegistryKeyValue(RegistryHive hive, string path, string name, string value, bool addQuotes = false)
		{
			bool result;
			try
			{
				using (RegistryKey registryKey = RegistryKey.OpenBaseKey(hive, RegistryView.Registry64).OpenWritableSubKeySafe(path))
				{
					if (registryKey == null)
					{
						result = false;
					}
					else
					{
						if (addQuotes && !value.StartsWith("\"") && !value.EndsWith("\""))
						{
							value = "\"" + value + "\"";
						}
						registryKey.SetValue(name, value);
						result = true;
					}
				}
			}
			catch (Exception)
			{
				result = false;
			}
			return result;
		}

		// Token: 0x0600029A RID: 666 RVA: 0x000242B4 File Offset: 0x000224B4
		public static RegistryKey OpenReadonlySubKey(RegistryHive hive, string path)
		{
			RegistryKey result;
			try
			{
				result = RegistryKey.OpenBaseKey(hive, RegistryView.Registry64).OpenSubKey(path, false);
			}
			catch
			{
				result = null;
			}
			return result;
		}

		// Token: 0x0600029B RID: 667 RVA: 0x000242EC File Offset: 0x000224EC
		public static bool DeleteRegistryKeyValue(RegistryHive hive, string path, string name)
		{
			bool result;
			try
			{
				using (RegistryKey registryKey = RegistryKey.OpenBaseKey(hive, RegistryView.Registry64).OpenWritableSubKeySafe(path))
				{
					if (registryKey == null)
					{
						result = false;
					}
					else
					{
						registryKey.DeleteValue(name, true);
						result = true;
					}
				}
			}
			catch (Exception)
			{
				result = false;
			}
			return result;
		}

		// Token: 0x0600029C RID: 668 RVA: 0x0002434C File Offset: 0x0002254C
		public static bool IsDefaultValue(string valueName)
		{
			return string.IsNullOrEmpty(valueName);
		}

		// Token: 0x0600029D RID: 669 RVA: 0x00024354 File Offset: 0x00022554
		public static RegistrySeeker.RegValueData[] AddDefaultValue(List<RegistrySeeker.RegValueData> values)
		{
			if (!values.Any((RegistrySeeker.RegValueData value) => RegistryKeyHelper.IsDefaultValue(value.Name)))
			{
				values.Add(RegistryKeyHelper.GetDefaultValue());
			}
			return values.ToArray();
		}

		// Token: 0x0600029E RID: 670 RVA: 0x0002438E File Offset: 0x0002258E
		public static RegistrySeeker.RegValueData[] GetDefaultValues()
		{
			return new RegistrySeeker.RegValueData[]
			{
				RegistryKeyHelper.GetDefaultValue()
			};
		}

		// Token: 0x0600029F RID: 671 RVA: 0x000243A0 File Offset: 0x000225A0
		public static RegistrySeeker.RegValueData CreateRegValueData(string name, RegistryValueKind kind, object value = null)
		{
			RegistrySeeker.RegValueData regValueData = new RegistrySeeker.RegValueData
			{
				Name = name,
				Kind = kind
			};
			if (value == null)
			{
				regValueData.Data = new byte[0];
			}
			else
			{
				switch (regValueData.Kind)
				{
				case RegistryValueKind.String:
				case RegistryValueKind.ExpandString:
					regValueData.Data = ByteConverter.GetBytes((string)value);
					break;
				case RegistryValueKind.Binary:
					regValueData.Data = (byte[])value;
					break;
				case RegistryValueKind.DWord:
					regValueData.Data = ByteConverter.GetBytes((uint)((int)value));
					break;
				case RegistryValueKind.MultiString:
					regValueData.Data = ByteConverter.GetBytes((string[])value);
					break;
				case RegistryValueKind.QWord:
					regValueData.Data = ByteConverter.GetBytes((ulong)((long)value));
					break;
				}
			}
			return regValueData;
		}

		// Token: 0x060002A0 RID: 672 RVA: 0x0002446B File Offset: 0x0002266B
		private static RegistrySeeker.RegValueData GetDefaultValue()
		{
			return RegistryKeyHelper.CreateRegValueData(RegistryKeyHelper.DEFAULT_VALUE, RegistryValueKind.String, null);
		}

		// Token: 0x0400017F RID: 383
		private static string DEFAULT_VALUE = string.Empty;
	}
}
