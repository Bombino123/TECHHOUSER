using System;
using Server.Connectings;

namespace Server.Messages
{
	// Token: 0x0200004B RID: 75
	internal class HandlerReportWindow
	{
		// Token: 0x060001CC RID: 460 RVA: 0x0001CF94 File Offset: 0x0001B194
		public static void Read(Clients client, object[] objects)
		{
			Clients[] array = Program.form.ClientsAll();
			client.Hwid = (string)objects[1];
			foreach (Clients clients in array)
			{
				if (clients.Hwid == client.Hwid)
				{
					clients.ReportWindow = client;
					return;
				}
			}
			client.Disconnect();
		}
	}
}
