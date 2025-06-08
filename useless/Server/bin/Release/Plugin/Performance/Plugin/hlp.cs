using System;
using System.Diagnostics;
using System.Management;
using Microsoft.VisualBasic.Devices;

namespace Plugin;

internal class hlp
{
	public static PerformanceCounter CPUCounter = new PerformanceCounter();

	public static PerformanceCounter RAMCounter = new PerformanceCounter();

	public static PerformanceCounter SysUpTime = new PerformanceCounter();

	public static string MaxClockSpeed()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		string result = string.Empty;
		try
		{
			ManagementObjectEnumerator enumerator = new ManagementObjectSearcher("select * from Win32_Processor").Get().GetEnumerator();
			if (enumerator.MoveNext())
			{
				result = ((ManagementBaseObject)(ManagementObject)enumerator.Current)["MaxClockSpeed"].ToString();
			}
		}
		catch
		{
			result = "????";
		}
		return result;
	}

	public static string NumberOfCores()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		string result = string.Empty;
		try
		{
			ManagementObjectEnumerator enumerator = new ManagementObjectSearcher("select * from Win32_Processor").Get().GetEnumerator();
			if (enumerator.MoveNext())
			{
				result = ((ManagementBaseObject)(ManagementObject)enumerator.Current)["NumberOfCores"].ToString();
			}
		}
		catch (Exception)
		{
			result = "????";
		}
		return result;
	}

	public static string NumberOfLogicalProcessors()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		string result = string.Empty;
		try
		{
			ManagementObjectEnumerator enumerator = new ManagementObjectSearcher("select * from Win32_Processor").Get().GetEnumerator();
			if (enumerator.MoveNext())
			{
				result = ((ManagementBaseObject)(ManagementObject)enumerator.Current)["NumberOfLogicalProcessors"].ToString();
			}
		}
		catch (Exception)
		{
			result = "????";
		}
		return result;
	}

	public static string CPU()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		string result = "";
		try
		{
			ManagementObjectEnumerator enumerator = new ManagementObjectSearcher("select * from Win32_Processor").Get().GetEnumerator();
			if (enumerator.MoveNext())
			{
				result = ((ManagementBaseObject)(ManagementObject)enumerator.Current)["Name"].ToString();
			}
		}
		catch (Exception)
		{
			result = "????";
		}
		return result;
	}

	public static string RAM()
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		checked
		{
			try
			{
				string result = null;
				ulong totalPhysicalMemory = ((ServerComputer)new Computer()).Info.TotalPhysicalMemory;
				if (totalPhysicalMemory > 1073741824)
				{
					result = ((double)totalPhysicalMemory / 1073741824.0).ToString();
					result = result.Remove(4, result.Length - 4) + " GB";
				}
				else if (totalPhysicalMemory > 1048576)
				{
					result = ((double)totalPhysicalMemory / 1048576.0).ToString();
					result = result.Remove(4, result.Length - 4) + " MB";
				}
				return result;
			}
			catch (Exception)
			{
				return "????";
			}
		}
	}

	public static string RAMSPEED()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		string text = string.Empty;
		try
		{
			ManagementObjectEnumerator enumerator = new ManagementObjectSearcher("select * from Win32_PhysicalMemory").Get().GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					text = ((ManagementBaseObject)(ManagementObject)enumerator.Current)["Speed"].ToString();
				}
			}
			finally
			{
				((IDisposable)enumerator)?.Dispose();
			}
		}
		catch (Exception)
		{
			text = "????";
		}
		return text.ToString();
	}
}
