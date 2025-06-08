using System.IO;
using System.Linq;

namespace RageStealer.Helper;

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

	public static long DirectorySize(string path)
	{
		DirectoryInfo directoryInfo = new DirectoryInfo(path);
		return directoryInfo.GetFiles().Sum((FileInfo fi) => fi.Length) + directoryInfo.GetDirectories().Sum((DirectoryInfo di) => DirectorySize(di.FullName));
	}
}
