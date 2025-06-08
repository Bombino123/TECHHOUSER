using System;
using System.Threading;

namespace Client.Helper
{
	// Token: 0x0200000F RID: 15
	public static class MutexControl
	{
		// Token: 0x0600005C RID: 92 RVA: 0x00005494 File Offset: 0x00003694
		public static bool CreateMutex(string mtx)
		{
			MutexControl.currentApp = new Mutex(false, mtx, out createdNew);
			return MutexControl.createdNew;
		}

		// Token: 0x0600005D RID: 93 RVA: 0x000054AC File Offset: 0x000036AC
		public static void Exit()
		{
			if (MutexControl.currentApp != null)
			{
				MutexControl.currentApp.Dispose();
			}
		}

		// Token: 0x04000033 RID: 51
		public static Mutex currentApp;

		// Token: 0x04000034 RID: 52
		public static bool createdNew;
	}
}
