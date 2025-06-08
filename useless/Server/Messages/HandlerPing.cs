using System;
using Server.Connectings;

namespace Server.Messages
{
	// Token: 0x02000068 RID: 104
	internal class HandlerPing
	{
		// Token: 0x06000208 RID: 520 RVA: 0x000212CE File Offset: 0x0001F4CE
		public static void Read(Clients client, object[] objects)
		{
			if (client.Tag == null)
			{
				client.Disconnect();
				return;
			}
			client.lastPing.Last();
			client.Send(new object[]
			{
				"Pong"
			});
		}
	}
}
