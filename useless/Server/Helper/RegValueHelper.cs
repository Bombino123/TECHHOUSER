using System;
using Microsoft.Win32;

namespace Server.Helper
{
	// Token: 0x02000077 RID: 119
	public class RegValueHelper
	{
		// Token: 0x06000290 RID: 656 RVA: 0x00023F41 File Offset: 0x00022141
		public static bool IsDefaultValue(string valueName)
		{
			return string.IsNullOrEmpty(valueName);
		}

		// Token: 0x06000291 RID: 657 RVA: 0x00023F49 File Offset: 0x00022149
		public static string GetName(string valueName)
		{
			if (!RegValueHelper.IsDefaultValue(valueName))
			{
				return valueName;
			}
			return RegValueHelper.DEFAULT_REG_VALUE;
		}

		// Token: 0x06000292 RID: 658 RVA: 0x00023F5C File Offset: 0x0002215C
		public static string RegistryValueToString(RegistrySeeker.RegValueData value)
		{
			switch (value.Kind)
			{
			case RegistryValueKind.String:
			case RegistryValueKind.ExpandString:
				return ByteConverter.ToString(value.Data);
			case RegistryValueKind.Binary:
				if (value.Data.Length == 0)
				{
					return "(zero-length binary value)";
				}
				return BitConverter.ToString(value.Data).Replace("-", " ").ToLower();
			case RegistryValueKind.DWord:
			{
				uint num = ByteConverter.ToUInt32(value.Data);
				return string.Format("0x{0:x8} ({1})", num, num);
			}
			case RegistryValueKind.MultiString:
				return string.Join(" ", ByteConverter.ToStringArray(value.Data));
			case RegistryValueKind.QWord:
			{
				ulong num2 = ByteConverter.ToUInt64(value.Data);
				return string.Format("0x{0:x8} ({1})", num2, num2);
			}
			}
			return string.Empty;
		}

		// Token: 0x0400017E RID: 382
		private static string DEFAULT_REG_VALUE = "(Default)";
	}
}
