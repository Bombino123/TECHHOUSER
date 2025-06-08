using System;
using System.Threading;

namespace Server.Connectings
{
	// Token: 0x020000CB RID: 203
	public class LastPing
	{
		// Token: 0x0600068C RID: 1676 RVA: 0x0005C5AC File Offset: 0x0005A7AC
		public LastPing(Clients client)
		{
			this.client = client;
			this.lastPing = DateTime.Now;
			this.timer = new Timer(new TimerCallback(this.Check), null, 1, 2000);
		}

		// Token: 0x0600068D RID: 1677 RVA: 0x0005C5E4 File Offset: 0x0005A7E4
		private double DiffSeconds(DateTime startTime, DateTime endTime)
		{
			return Math.Abs(new TimeSpan(endTime.Ticks - startTime.Ticks).TotalSeconds);
		}

		// Token: 0x0600068E RID: 1678 RVA: 0x0005C612 File Offset: 0x0005A812
		private void Check(object obj)
		{
			if (this.DiffSeconds(this.lastPing, DateTime.Now) > (double)Program.form.settings.second)
			{
				this.client.Disconnect();
			}
		}

		// Token: 0x0600068F RID: 1679 RVA: 0x0005C642 File Offset: 0x0005A842
		public void Disconnect()
		{
			Timer timer = this.timer;
			if (timer == null)
			{
				return;
			}
			timer.Dispose();
		}

		// Token: 0x06000690 RID: 1680 RVA: 0x0005C654 File Offset: 0x0005A854
		public void Last()
		{
			this.lastPing = DateTime.Now;
		}

		// Token: 0x0400058F RID: 1423
		private Timer timer;

		// Token: 0x04000590 RID: 1424
		public Clients client;

		// Token: 0x04000591 RID: 1425
		private DateTime lastPing;
	}
}
