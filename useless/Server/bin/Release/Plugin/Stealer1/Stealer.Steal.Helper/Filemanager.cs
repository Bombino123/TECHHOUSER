using System.IO;
using System.Linq;

namespace Stealer.Steal.Helper;

internal class Filemanager
{
	public static void RecursiveDelete(string path)
	{
		DirectoryInfo directoryInfo = new DirectoryInfo(path);
		if (directoryInfo.Exists)
		{
			DirectoryInfo[] directories = directoryInfo.GetDirectories();
			for (int i = 0; i < directories.Length; i++)
			{
				RecursiveDelete(directories[i].FullName);
			}
			directoryInfo.Delete(recursive: true);
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
			string text2 = Path.Combine(destFolder, fileName);
			if (File.Exists(text2))
			{
				File.Delete(text2);
			}
			File.Copy(text, text2);
		}
		files = Directory.GetDirectories(sourceFolder);
		foreach (string text3 in files)
		{
			string fileName2 = Path.GetFileName(text3);
			string text4 = Path.Combine(destFolder, fileName2);
			if (Directory.Exists(text4))
			{
				Directory.Delete(text4, recursive: true);
			}
			CopyDirectory(text3, text4);
		}
	}

	public static long DirectorySize(string path)
	{
		DirectoryInfo directoryInfo = new DirectoryInfo(path);
		return directoryInfo.GetFiles().Sum((FileInfo fi) => fi.Length) + directoryInfo.GetDirectories().Sum((DirectoryInfo di) => DirectorySize(di.FullName));
	}
}
