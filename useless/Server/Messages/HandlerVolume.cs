using System;
using System.Windows.Forms;
using Server.Connectings;
using Server.Forms;

namespace Server.Messages
{
	// Token: 0x0200004E RID: 78
	internal class HandlerVolume
	{
		// Token: 0x060001D2 RID: 466 RVA: 0x0001D0B8 File Offset: 0x0001B2B8
		public static void Read(Clients client, object[] objects)
		{
			string a = (string)objects[1];
			if (!(a == "Connect"))
			{
				if (!(a == "Volume"))
				{
					return;
				}
				if (client.Tag == null)
				{
					client.Disconnect();
					return;
				}
				FormVolumeControl form = (FormVolumeControl)client.Tag;
				form.Invoke(new MethodInvoker(delegate()
				{
					form.trackBar1.Value = (int)objects[2];
				}));
				return;
			}
			else
			{
				FormVolumeControl form = (FormVolumeControl)Application.OpenForms["Volume:" + (string)objects[2]];
				if (form == null)
				{
					client.Disconnect();
					return;
				}
				form.Invoke(new MethodInvoker(delegate()
				{
					form.Text = "Volume [" + (string)objects[2] + "]";
					form.client = client;
					form.trackBar1.Enabled = true;
					form.trackBar1.Value = (int)objects[3];
				}));
				client.Tag = form;
				client.Hwid = (string)objects[2];
				return;
			}
		}
	}
}
