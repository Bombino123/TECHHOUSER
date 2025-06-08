using System;
using System.Collections;
using System.Windows.Forms;
using Server.Connectings;
using Server.Forms;

namespace Server.Messages
{
	// Token: 0x02000045 RID: 69
	internal class HandlerDeviceManager
	{
		// Token: 0x060001BB RID: 443 RVA: 0x0001C1E4 File Offset: 0x0001A3E4
		public static void Read(Clients client, object[] objects)
		{
			string a = (string)objects[1];
			if (!(a == "Connect"))
			{
				if (!(a == "Error"))
				{
					if (!(a == "List"))
					{
						if (!(a == "Status"))
						{
							return;
						}
						if (client.Tag == null)
						{
							client.Disconnect();
							return;
						}
						FormDeviceManager form = (FormDeviceManager)client.Tag;
						form.Invoke(new MethodInvoker(delegate()
						{
							form.materialLabel1.Text = "Succues status";
							foreach (object obj in ((IEnumerable)form.dataGridView2.Rows))
							{
								DataGridViewRow dataGridViewRow = (DataGridViewRow)obj;
								if (dataGridViewRow.Cells[0].Value as string == (string)objects[2])
								{
									dataGridViewRow.Cells[2].Value = (string)objects[3];
									break;
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
						FormDeviceManager form = (FormDeviceManager)client.Tag;
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
					FormDeviceManager form = (FormDeviceManager)client.Tag;
					form.Invoke(new MethodInvoker(delegate()
					{
						form.materialLabel1.Text = "Error: " + (string)objects[2];
					}));
					return;
				}
			}
			else
			{
				FormDeviceManager form = (FormDeviceManager)Application.OpenForms["DeviceManager:" + (string)objects[2]];
				if (form == null)
				{
					client.Disconnect();
					return;
				}
				form.Invoke(new MethodInvoker(delegate()
				{
					form.Text = "Device Manager [" + (string)objects[2] + "]";
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
