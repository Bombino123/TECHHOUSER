using System;

namespace Server.Data
{
	// Token: 0x020000C7 RID: 199
	public class Settings
	{
		// Token: 0x04000567 RID: 1383
		public string[] Ports;

		// Token: 0x04000568 RID: 1384
		public bool Start;

		// Token: 0x04000569 RID: 1385
		public int second = 35;

		// Token: 0x0400056A RID: 1386
		public string WebHook;

		// Token: 0x0400056B RID: 1387
		public bool WebHookNewConnect;

		// Token: 0x0400056C RID: 1388
		public bool WebHookConnect;

		// Token: 0x0400056D RID: 1389
		public bool AutoStealer;

		// Token: 0x0400056E RID: 1390
		public int Style;

		// Token: 0x0400056F RID: 1391
		public string linkMiner = "http://%IP%/ack";
	}
}
