using System;
using System.Windows.Forms;
using Server.Connectings;
using Server.Forms;

namespace Server.Messages
{
	// Token: 0x02000044 RID: 68
	internal class HandlerBotSpeaker
	{
		// Token: 0x060001B9 RID: 441 RVA: 0x0001C084 File Offset: 0x0001A284
		public static void Read(Clients client, object[] objects)
		{
			string a = (string)objects[1];
			if (!(a == "Connect"))
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
				FormSpeakerBot form = (FormSpeakerBot)client.Tag;
				form.Invoke(new MethodInvoker(delegate()
				{
					form.rjComboBox1.Items.Clear();
					int i = 2;
					while (i < objects.Length)
					{
						form.rjComboBox1.Items.Add((string)objects[i++]);
					}
					if (form.rjComboBox1.Items.Count > 0)
					{
						form.rjComboBox1.SelectedIndex = 0;
					}
				}));
				return;
			}
			else
			{
				FormSpeakerBot form = (FormSpeakerBot)Application.OpenForms["BotSpeaker:" + (string)objects[2]];
				if (form == null)
				{
					client.Disconnect();
					return;
				}
				form.Invoke(new MethodInvoker(delegate()
				{
					form.Text = "Speaker Bot [" + (string)objects[2] + "]";
					form.client = client;
					form.richTextBox1.Enabled = true;
					form.rjComboBox1.Enabled = true;
					form.trackBar1.Enabled = true;
					form.trackBar2.Enabled = true;
					form.rjButton1.Enabled = true;
				}));
				client.Tag = form;
				client.Hwid = (string)objects[2];
				return;
			}
		}
	}
}
