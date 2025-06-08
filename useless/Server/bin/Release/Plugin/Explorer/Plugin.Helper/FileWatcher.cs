using System;
using System.IO;
using Leb128;

namespace Plugin.Helper;

internal class FileWatcher
{
	public static FileSystemWatcher fileSystemWatcher;

	public static void Start(string path)
	{
		Stop();
		fileSystemWatcher = new FileSystemWatcher();
		fileSystemWatcher.Path = path;
		fileSystemWatcher.EnableRaisingEvents = true;
		fileSystemWatcher.Created += OnChanged;
		fileSystemWatcher.Deleted += OnChanged;
		fileSystemWatcher.Renamed += OnRenamed;
	}

	public static void Stop()
	{
		if (fileSystemWatcher != null)
		{
			fileSystemWatcher.Created -= OnChanged;
			fileSystemWatcher.Deleted -= OnChanged;
			fileSystemWatcher.Renamed -= OnRenamed;
			fileSystemWatcher.Dispose();
		}
	}

	private static void OnChanged(object source, FileSystemEventArgs e)
	{
		if (e.ChangeType == WatcherChangeTypes.Created)
		{
			if (Directory.Exists(e.FullPath))
			{
				Client.Send(LEB128.Write(new object[5]
				{
					"Explorer",
					"CreatedDir",
					Path.GetFileName(e.FullPath),
					new FileInfo(e.FullPath).LastWriteTime.ToString(),
					new FileInfo(e.FullPath).Attributes.ToString()
				}));
			}
			else if (File.Exists(e.FullPath))
			{
				Client.Send(LEB128.Write(new object[7]
				{
					"Explorer",
					"CreatedFile",
					Methods.GetIcon(e.FullPath),
					Path.GetFileName(e.FullPath),
					new FileInfo(e.FullPath).LastWriteTime.ToString(),
					new FileInfo(e.FullPath).Attributes.ToString(),
					new FileInfo(e.FullPath).Length
				}));
			}
		}
		if (e.ChangeType == WatcherChangeTypes.Deleted)
		{
			Client.Send(LEB128.Write(new object[3]
			{
				"Explorer",
				"Deleted",
				Path.GetFileName(e.FullPath)
			}));
		}
		Console.WriteLine($"Файл или папка {e.ChangeType}: {e.FullPath}");
	}

	private static void OnRenamed(object source, RenamedEventArgs e)
	{
		Console.WriteLine("Файл или папка переименована: " + e.OldFullPath + " переименовано в " + e.FullPath);
		if (Directory.Exists(e.FullPath))
		{
			Client.Send(LEB128.Write(new object[6]
			{
				"Explorer",
				"Renamed",
				Path.GetFileName(e.OldFullPath),
				Path.GetFileName(e.FullPath),
				new FileInfo(e.FullPath).LastWriteTime.ToString(),
				new FileInfo(e.FullPath).Attributes.ToString()
			}));
		}
		else
		{
			Client.Send(LEB128.Write(new object[8]
			{
				"Explorer",
				"Renamed",
				Path.GetFileName(e.OldFullPath),
				Path.GetFileName(e.FullPath),
				new FileInfo(e.FullPath).LastWriteTime.ToString(),
				new FileInfo(e.FullPath).Attributes.ToString(),
				Methods.GetIcon(e.FullPath),
				new FileInfo(e.FullPath).Length
			}));
		}
	}
}
