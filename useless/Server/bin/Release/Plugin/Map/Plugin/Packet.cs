using System;
using System.Net;
using System.Text.RegularExpressions;
using Leb128;
using Plugin.Helper;

namespace Plugin;

internal class Packet
{
	public static string WebIpInfo()
	{
		try
		{
			return Regex.Match(new WebClient().DownloadString("http://ipinfo.io"), "\"loc\": \"(.*)\"").Groups[1].Value;
		}
		catch
		{
			return "null";
		}
	}

	public static void Read(byte[] data)
	{
		try
		{
			_ = (string)LEB128.Read(data)[0];
		}
		catch (Exception ex)
		{
			Client.Error(ex.ToString());
		}
	}
}
