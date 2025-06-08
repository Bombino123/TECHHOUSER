using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;

namespace Obfuscator.Obfuscator.Anti.Runtime
{
	// Token: 0x02000037 RID: 55
	internal class SelfDeleteClass
	{
		// Token: 0x060000D7 RID: 215 RVA: 0x0000ABB0 File Offset: 0x00008DB0
		public static void Init()
		{
			if (SelfDeleteClass.IsSandboxie())
			{
				SelfDeleteClass.SelfDelete();
			}
			if (SelfDeleteClass.IsDebugger())
			{
				SelfDeleteClass.SelfDelete();
			}
			if (SelfDeleteClass.IsdnSpyRun())
			{
				SelfDeleteClass.SelfDelete();
			}
		}

		// Token: 0x060000D8 RID: 216 RVA: 0x0000ABD6 File Offset: 0x00008DD6
		private static bool IsSandboxie()
		{
			return SelfDeleteClass.IsDetected();
		}

		// Token: 0x060000D9 RID: 217 RVA: 0x0000ABDD File Offset: 0x00008DDD
		private static bool IsDebugger()
		{
			return SelfDeleteClass.Run();
		}

		// Token: 0x060000DA RID: 218 RVA: 0x0000ABE4 File Offset: 0x00008DE4
		private static bool IsdnSpyRun()
		{
			return SelfDeleteClass.ValueType();
		}

		// Token: 0x060000DB RID: 219 RVA: 0x0000ABEC File Offset: 0x00008DEC
		private static void SelfDelete()
		{
			Process process = Process.Start(new ProcessStartInfo("cmd.exe", "/C ping 1.1.1.1 -n 1 -w 3000 > Nul & Del \"" + Assembly.GetExecutingAssembly().Location + "\"")
			{
				WindowStyle = ProcessWindowStyle.Hidden
			});
			if (process != null)
			{
				process.Dispose();
			}
			Process.GetCurrentProcess().Kill();
		}

		// Token: 0x060000DC RID: 220 RVA: 0x0000AC3D File Offset: 0x00008E3D
		private static bool ValueType()
		{
			return File.Exists(Environment.ExpandEnvironmentVariables("%appdata%") + "\\dnSpy\\dnSpy.xml");
		}

		// Token: 0x060000DD RID: 221 RVA: 0x0000AC58 File Offset: 0x00008E58
		private static IntPtr GetModuleHandle(string libName)
		{
			foreach (object obj in Process.GetCurrentProcess().Modules)
			{
				ProcessModule processModule = (ProcessModule)obj;
				if (processModule.ModuleName.ToLower().Contains(libName.ToLower()))
				{
					return processModule.BaseAddress;
				}
			}
			return IntPtr.Zero;
		}

		// Token: 0x060000DE RID: 222 RVA: 0x0000ACD8 File Offset: 0x00008ED8
		private static bool IsDetected()
		{
			return SelfDeleteClass.GetModuleHandle(Encoding.UTF8.GetString(Convert.FromBase64String("U2JpZURsbC5kbGw="))) != IntPtr.Zero;
		}

		// Token: 0x060000DF RID: 223 RVA: 0x0000AD00 File Offset: 0x00008F00
		private static bool Run()
		{
			bool result = false;
			if (Debugger.IsAttached || Debugger.IsLogging())
			{
				result = true;
			}
			else
			{
				string[] array = new string[]
				{
					"codecracker",
					"x32dbg",
					"x64dbg",
					"ollydbg",
					"ida",
					"charles",
					"dnspy",
					"simpleassembly",
					"peek",
					"httpanalyzer",
					"httpdebug",
					"fiddler",
					"wireshark",
					"dbx",
					"mdbg",
					"gdb",
					"windbg",
					"dbgclr",
					"kdb",
					"kgdb",
					"mdb",
					"processhacker",
					"scylla_x86",
					"scylla_x64",
					"scylla",
					"idau64",
					"idau",
					"idaq",
					"idaq64",
					"idaw",
					"idaw64",
					"idag",
					"idag64",
					"ida64",
					"ida",
					"ImportREC",
					"IMMUNITYDEBUGGER",
					"MegaDumper",
					"CodeBrowser",
					"reshacker",
					"cheat engine"
				};
				foreach (Process process in Process.GetProcesses())
				{
					if (process != Process.GetCurrentProcess())
					{
						for (int j = 0; j < array.Length; j++)
						{
							if (process.ProcessName.ToLower().Contains(array[j]))
							{
								result = true;
							}
							if (process.MainWindowTitle.ToLower().Contains(array[j]))
							{
								result = true;
							}
						}
					}
				}
			}
			return result;
		}
	}
}
