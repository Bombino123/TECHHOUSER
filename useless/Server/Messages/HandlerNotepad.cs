using System;
using System.Windows.Forms;
using Server.Connectings;
using Server.Forms;

namespace Server.Messages
{
	// Token: 0x0200004F RID: 79
	internal class HandlerNotepad
	{
		// Token: 0x060001D4 RID: 468 RVA: 0x0001D218 File Offset: 0x0001B418
		public static void Read(Clients client, object[] objects)
		{
			string a = (string)objects[1];
			if (!(a == "Connect"))
			{
				if (!(a == "Error"))
				{
					return;
				}
				if (client.Tag == null)
				{
					client.Disconnect();
					return;
				}
				FormNotepad form = (FormNotepad)client.Tag;
				form.Invoke(new MethodInvoker(delegate()
				{
					RichTextBox richTextBox = form.richTextBox1;
					richTextBox.Text = richTextBox.Text + "Error: " + (string)objects[2] + "\n";
				}));
				return;
			}
			else
			{
				FormNotepad form = (FormNotepad)Application.OpenForms["Notepad:" + (string)objects[2]];
				if (form == null)
				{
					client.Disconnect();
					return;
				}
				form.Invoke(new MethodInvoker(delegate()
				{
					form.Text = "Notepad [" + (string)objects[2] + "]";
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
