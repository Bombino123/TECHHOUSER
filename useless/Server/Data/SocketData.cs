using System;
using System.Threading.Tasks;

namespace Server.Data
{
	// Token: 0x020000C8 RID: 200
	internal class SocketData
	{
		// Token: 0x1700005F RID: 95
		// (get) Token: 0x0600064C RID: 1612 RVA: 0x0005BB3B File Offset: 0x00059D3B
		// (set) Token: 0x0600064D RID: 1613 RVA: 0x0005BB42 File Offset: 0x00059D42
		public static long Connects { get; private set; }

		// Token: 0x17000060 RID: 96
		// (get) Token: 0x0600064E RID: 1614 RVA: 0x0005BB4A File Offset: 0x00059D4A
		// (set) Token: 0x0600064F RID: 1615 RVA: 0x0005BB51 File Offset: 0x00059D51
		public static long Sents { get; private set; }

		// Token: 0x17000061 RID: 97
		// (get) Token: 0x06000650 RID: 1616 RVA: 0x0005BB59 File Offset: 0x00059D59
		// (set) Token: 0x06000651 RID: 1617 RVA: 0x0005BB60 File Offset: 0x00059D60
		public static long Recives { get; private set; }

		// Token: 0x17000062 RID: 98
		// (get) Token: 0x06000652 RID: 1618 RVA: 0x0005BB68 File Offset: 0x00059D68
		// (set) Token: 0x06000653 RID: 1619 RVA: 0x0005BB6F File Offset: 0x00059D6F
		public static long Sent { get; private set; }

		// Token: 0x17000063 RID: 99
		// (get) Token: 0x06000654 RID: 1620 RVA: 0x0005BB77 File Offset: 0x00059D77
		// (set) Token: 0x06000655 RID: 1621 RVA: 0x0005BB7E File Offset: 0x00059D7E
		public static long Recive { get; private set; }

		// Token: 0x06000656 RID: 1622 RVA: 0x0005BB86 File Offset: 0x00059D86
		public static void ConnectsPluse()
		{
			Task.Run(delegate()
			{
				object obj = SocketData.syncConnects;
				lock (obj)
				{
					SocketData.Connects += 1L;
				}
			});
		}

		// Token: 0x06000657 RID: 1623 RVA: 0x0005BBAD File Offset: 0x00059DAD
		public static void ConnectsMinuse()
		{
			Task.Run(delegate()
			{
				object obj = SocketData.syncConnects;
				lock (obj)
				{
					SocketData.Connects -= 1L;
				}
			});
		}

		// Token: 0x06000658 RID: 1624 RVA: 0x0005BBD4 File Offset: 0x00059DD4
		public static void SentData(long count)
		{
			Task.Run(delegate()
			{
				object obj = SocketData.syncSents;
				lock (obj)
				{
					SocketData.Sents += count;
				}
			});
			Task.Run(delegate()
			{
				object obj = SocketData.syncSent;
				lock (obj)
				{
					SocketData.Sent += count;
				}
			});
		}

		// Token: 0x06000659 RID: 1625 RVA: 0x0005BC05 File Offset: 0x00059E05
		public static void ReciveData(long count)
		{
			Task.Run(delegate()
			{
				object obj = SocketData.syncRecives;
				lock (obj)
				{
					SocketData.Recives += count;
				}
			});
			Task.Run(delegate()
			{
				object obj = SocketData.syncRecive;
				lock (obj)
				{
					SocketData.Recive += count;
				}
			});
		}

		// Token: 0x0600065A RID: 1626 RVA: 0x0005BC36 File Offset: 0x00059E36
		public static void Clear()
		{
			Task.Run(delegate()
			{
				object obj = SocketData.syncSent;
				lock (obj)
				{
					SocketData.Sent = 0L;
				}
				obj = SocketData.syncRecive;
				lock (obj)
				{
					SocketData.Recive = 0L;
				}
			});
		}

		// Token: 0x04000570 RID: 1392
		private static object syncSent = new object();

		// Token: 0x04000571 RID: 1393
		private static object syncSents = new object();

		// Token: 0x04000572 RID: 1394
		private static object syncRecives = new object();

		// Token: 0x04000573 RID: 1395
		private static object syncRecive = new object();

		// Token: 0x04000574 RID: 1396
		private static object syncConnects = new object();
	}
}
