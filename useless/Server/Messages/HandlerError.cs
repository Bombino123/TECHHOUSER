using System;
using System.Drawing;
using Server.Connectings;
using Server.Helper;

namespace Server.Messages
{
	// Token: 0x0200005D RID: 93
	internal class HandlerError
	{
		// Token: 0x060001F0 RID: 496 RVA: 0x0001F42E File Offset: 0x0001D62E
		public static void Read(Clients client, object[] objects)
		{
			Console.WriteLine("Error: " + (string)objects[1]);
			Methods.AppendLogs(client.IP, "Error: " + (string)objects[1], Color.Red);
		}
	}
}
