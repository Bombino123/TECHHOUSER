using System.Collections.Generic;
using System.IO;

namespace Plugin.Helper;

internal class Manager
{
	public static List<string> Names = new List<string>();

	public static bool cut = false;

	public static void Copy(string st)
	{
		cut = false;
		Names.Clear();
		string[] array = st.Split(new char[1] { ';' });
		foreach (string item in array)
		{
			Names.Add(item);
		}
	}

	public static void Cut(string st)
	{
		cut = true;
		Names.Clear();
		string[] array = st.Split(new char[1] { ';' });
		foreach (string item in array)
		{
			Names.Add(item);
		}
	}

	public static void Paste(string path)
	{
		if (Names.Count <= 0)
		{
			return;
		}
		foreach (string name in Names)
		{
			if (cut)
			{
				if (Directory.Exists(name))
				{
					Directory.Move(name, Path.Combine(path, Path.GetFileName(name)));
				}
				else
				{
					File.Move(name, Path.Combine(path, Path.GetFileName(name)));
				}
			}
			else if (Directory.Exists(name))
			{
				CopyDirectory(name, Path.Combine(path, Path.GetFileName(name)));
			}
			else
			{
				File.Copy(name, Path.Combine(path, Path.GetFileName(name)));
			}
		}
	}

	public static void CopyDirectory(string sourceFolder, string destFolder)
	{
		if (!Directory.Exists(destFolder))
		{
			Directory.CreateDirectory(destFolder);
		}
		string[] files = Directory.GetFiles(sourceFolder);
		foreach (string text in files)
		{
			string fileName = Path.GetFileName(text);
			string destFileName = Path.Combine(destFolder, fileName);
			File.Copy(text, destFileName);
		}
		files = Directory.GetDirectories(sourceFolder);
		foreach (string text2 in files)
		{
			string fileName2 = Path.GetFileName(text2);
			string destFolder2 = Path.Combine(destFolder, fileName2);
			CopyDirectory(text2, destFolder2);
		}
	}
}
