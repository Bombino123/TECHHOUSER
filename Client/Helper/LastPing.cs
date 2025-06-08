using System;
using System.Threading;

namespace Client.Helper
{
	// Token: 0x0200000D RID: 13
	public class LastPing
	{
		// Token: 0x06000048 RID: 72 RVA: 0x00004C38 File Offset: 0x00002E38
		public LastPing(Client client)
		{
			this.ticks = DateTime.Now.Ticks;
			this.timer = new Timer(new TimerCallback(this.Check), client, 0, 6000);
		}

		// Token: 0x06000049 RID: 73 RVA: 0x00004C80 File Offset: 0x00002E80
		private int DiffSeconds(long startTime, DateTime endTime)
		{
			return (int)Math.Abs(new TimeSpan(endTime.Ticks - startTime).TotalSeconds);
		}

		// Token: 0x0600004A RID: 74 RVA: 0x00004CB0 File Offset: 0x00002EB0
		private void Check(object obj)
		{
			if (this.DiffSeconds(this.ticks, DateTime.Now) > 60)
			{
				((Client)obj).Disconnect();
			}
		}

		// Token: 0x0600004B RID: 75 RVA: 0x00004CD8 File Offset: 0x00002ED8
		public void Disconnect()
		{
			if (this.timer != null)
			{
				this.timer.Dispose();
			}
		}

		// Token: 0x0600004C RID: 76 RVA: 0x00004CF0 File Offset: 0x00002EF0
		public void Last()
		{
			this.ticks = DateTime.Now.Ticks;
		}

		// Token: 0x04000030 RID: 48
		private Timer timer;

		// Token: 0x04000031 RID: 49
		private long ticks;
	}
}
