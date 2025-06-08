using System;
using Microsoft.Win32;

namespace Server.Helper
{
	// Token: 0x02000078 RID: 120
	public static class RegistryKeyExtensions
	{
		// Token: 0x06000295 RID: 661 RVA: 0x0002405C File Offset: 0x0002225C
		public static string RegistryTypeToString(this RegistryValueKind valueKind, object valueData)
		{
			if (valueData == null)
			{
				return "(value not set)";
			}
			switch (valueKind)
			{
			case RegistryValueKind.String:
			case RegistryValueKind.ExpandString:
				return valueData.ToString();
			case RegistryValueKind.Binary:
				if (((byte[])valueData).Length != 0)
				{
					return BitConverter.ToString((byte[])valueData).Replace("-", " ").ToLower();
				}
				return "(zero-length binary value)";
			case RegistryValueKind.DWord:
				return string.Format("0x{0} ({1})", ((uint)((int)valueData)).ToString("x8"), ((uint)((int)valueData)).ToString());
			case RegistryValueKind.MultiString:
				return string.Join(" ", (string[])valueData);
			case RegistryValueKind.QWord:
				return string.Format("0x{0} ({1})", ((ulong)((long)valueData)).ToString("x8"), ((ulong)((long)valueData)).ToString());
			}
			return string.Empty;
		}

		// Token: 0x06000296 RID: 662 RVA: 0x00024150 File Offset: 0x00022350
		public static RegistryKey OpenReadonlySubKeySafe(this RegistryKey key, string name)
		{
			RegistryKey result;
			try
			{
				result = key.OpenSubKey(name, false);
			}
			catch
			{
				result = null;
			}
			return result;
		}

		// Token: 0x06000297 RID: 663 RVA: 0x00024180 File Offset: 0x00022380
		public static RegistryKey OpenWritableSubKeySafe(this RegistryKey key, string name)
		{
			RegistryKey result;
			try
			{
				result = key.OpenSubKey(name, true);
			}
			catch
			{
				result = null;
			}
			return result;
		}

		// Token: 0x06000298 RID: 664 RVA: 0x000241B0 File Offset: 0x000223B0
		public static string RegistryTypeToString(this RegistryValueKind valueKind)
		{
			switch (valueKind)
			{
			case RegistryValueKind.Unknown:
				return "(Unknown)";
			case RegistryValueKind.String:
				return "REG_SZ";
			case RegistryValueKind.ExpandString:
				return "REG_EXPAND_SZ";
			case RegistryValueKind.Binary:
				return "REG_BINARY";
			case RegistryValueKind.DWord:
				return "REG_DWORD";
			case RegistryValueKind.MultiString:
				return "REG_MULTI_SZ";
			case RegistryValueKind.QWord:
				return "REG_QWORD";
			}
			return "REG_NONE";
		}
	}
}
